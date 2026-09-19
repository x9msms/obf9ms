/**
 * obf9ms - 77fuscator as a Vercel Serverless Function
 * ----------------------------------------------------
 * The obfuscator itself is a C#/.NET 8 program (see /src). Vercel cannot run
 * .NET natively, so this Node function shells out to a self-contained
 * linux-x64 .NET binary that lives in ./vendor.
 *
 * POST /api/obfuscate
 *   body: { code: string, settings?: object, filename?: string }
 *         (or raw text/Lua body)
 *   ->  200 { ok:true, code, size:{input,output}, elapsedMs }
 *   ->  400 { ok:false, error }  bad input
 *   ->  500 { ok:false, error }  obfuscator failed
 *
 * GET /api/obfuscate            -> usage + limits
 * GET /api/obfuscate?health=1   -> runtime self-check
 *
 * Hard limits worth knowing:
 *   - Vercel Hobby caps function duration at 10s (maxDuration:60 below only
 *     takes effect on Pro). A typical small script takes ~3.5s including the
 *     .NET cold start, so keep input small on Hobby.
 *   - Lua 5.1 itself allows at most 200 local variables per function; past
 *     that the upstream compiler aborts with
 *     "main function has more than 200 local variables".
 */

const { spawn } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');
const crypto = require('crypto');

const VENDOR = path.join(__dirname, 'vendor');
const BINARY = path.join(VENDOR, 'obf77');

const MAX_BODY_BYTES = 512 * 1024; // 512 KB of Lua source
const MAX_OUTPUT_BYTES = 12 * 1024 * 1024;
const KILL_AFTER_MS = 55_000;

const DEFAULT_SETTINGS = {
  EncryptStrings: true,
  DecryptTableLen: 500,
  ExtraCompression: true,
  EnhancedSecurity: true,
  DynamicOpcodeStructure: false,
  Watermark: '77fuscator 0.6.1 EARLY BUILD',
};

module.exports = async function handler(req, res) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
  res.setHeader('Access-Control-Allow-Methods', 'GET,POST,OPTIONS');
  if (req.method === 'OPTIONS') return res.status(204).end();

  if (req.method !== 'GET' && req.method !== 'POST') {
    return res.status(405).json({ ok: false, error: 'use GET or POST' });
  }

  if (req.method === 'GET') {
    const query = req.query || {};
    if (query.health === '1' || query.health === 'true') {
      return res.status(200).json(await healthCheck());
    }
    return res.status(200).json(usage());
  }

  // ---------------------------------------------------------------- POST
  let body = req.body;
  if (typeof body === 'string') {
    try { body = JSON.parse(body); } catch { body = { code: body }; }
  }
  if (Buffer.isBuffer(body)) body = { code: body.toString('utf8') };
  body = body || {};

  const code = typeof body.code === 'string' ? body.code : '';
  if (!code.trim()) {
    return res.status(400).json({
      ok: false,
      error: 'missing "code" field (raw Lua source, as a string)',
    });
  }
  if (Buffer.byteLength(code, 'utf8') > MAX_BODY_BYTES) {
    return res.status(413).json({
      ok: false,
      error: `input too large (max ${MAX_BODY_BYTES} bytes)`,
    });
  }

  const settings = { ...DEFAULT_SETTINGS, ...(body.settings || {}) };

  let work = null;
  const started = Date.now();
  try {
    work = fs.mkdtempSync(path.join(realTmp(), 'obf9ms-'));
    const inputFile = path.join(work, 'input.lua');
    const outputFile = path.join(work, 'output.lua');
    const settingsFile = path.join(work, 'settings.json');

    fs.writeFileSync(inputFile, code, 'utf8');
    fs.writeFileSync(settingsFile, JSON.stringify(settings), 'utf8');

    const bin = await ensureExecutable(BINARY, work);
    const result = await runObfuscator(bin, work, inputFile, outputFile, settingsFile);

    if (!result.ok) {
      const classified = classifyError(result.error);
      return res.status(classified.status).json({
        ok: false,
        error: classified.message,
        // Only surface internals on 5xx - a syntax problem in the user's own
        // script should not leak .NET stack frames.
        ...(classified.status >= 500 ? { detail: (result.stderr || '').slice(-2000) } : {}),
        elapsedMs: Date.now() - started,
      });
    }

    const outStat = fs.statSync(outputFile);
    if (outStat.size > MAX_OUTPUT_BYTES) {
      return res.status(413).json({
        ok: false,
        error: `output too large (${outStat.size} bytes)`,
      });
    }

    const obfuscated = fs.readFileSync(outputFile, 'utf8');
    const filename =
      typeof body.filename === 'string' && /^[\w.-]+\.lua$/.test(body.filename)
        ? body.filename
        : 'obfuscated.lua';

    if (body.download === true || body.download === '1') {
      res.setHeader('Content-Type', 'text/plain; charset=utf-8');
      res.setHeader('Content-Disposition', `attachment; filename="${filename}"`);
      return res.status(200).send(obfuscated);
    }

    return res.status(200).json({
      ok: true,
      code: obfuscated,
      filename,
      size: { input: Buffer.byteLength(code, 'utf8'), output: outStat.size },
      elapsedMs: Date.now() - started,
    });
  } catch (err) {
    return res.status(500).json({
      ok: false,
      error: `${err.name || 'Error'}: ${err.message}`,
      elapsedMs: Date.now() - started,
    });
  } finally {
    if (work) fs.rmSync(work, { recursive: true, force: true });
  }
};

// Vercel Node runtime config. maxDuration > 10s requires the Pro plan;
// on Hobby Vercel silently clamps it to 10s.
module.exports.config = {
  api: {
    maxDuration: 60,
    bodyParser: { sizeLimit: '1mb' },
  },
};


/**
 * Maps an obf77 failure onto an HTTP status + a message that is safe to show.
 * Bad user input is a 400; anything else is a 500.
 */
function classifyError(raw) {
  const text = String(raw || '');

  if (/timeout after/i.test(text)) {
    return {
      status: 504,
      message:
        "obfuscation timed out - the script is too large for this plan's " +
        'function duration limit. Try splitting it, or disable ExtraCompression.',
    };
  }

  // Hard limits of Lua 5.1 itself. Checked before the generic parse-error rule
  // because these messages also carry a `[string "C#"]:NNN:` prefix.
  if (/more than 200 local variables/i.test(text)) {
    return {
      status: 400,
      message:
        'Lua 5.1 allows at most 200 local variables per function. Wrap parts of ' +
        'the script in do...end blocks or move them into functions.',
    };
  }
  if (/more than 60 nested functions|too many local variables|chunk has too many/i.test(text)) {
    return { status: 400, message: 'Lua 5.1 limit hit: ' + firstMeaningfulLine(text) };
  }
  if (/Upvalues cannot be present in appended code/i.test(text)) {
    return {
      status: 400,
      message: 'the script uses upvalues in a way the anti-tamper pass rejects.',
    };
  }

  // Loretta (the Lua parser/minifier) throws on source it cannot handle.
  if (
    /thought to be unreachable/i.test(text) ||
    /RenamingRewriter|Minify\(|LuaSyntaxTree|SyntaxTree/i.test(text) ||
    /\[string "C#"\]:\d+:/i.test(text)
  ) {
    return {
      status: 400,
      message:
        'the input is not valid Lua 5.1, or uses syntax the obfuscator cannot ' +
        'rewrite. ' + firstMeaningfulLine(text),
    };
  }

  if (/spawn failed/i.test(text)) {
    return { status: 500, message: 'runtime is broken: ' + text.slice(0, 300) };
  }

  return { status: 500, message: text.slice(0, 400) || 'obfuscation failed' };
}

function firstMeaningfulLine(text) {
  const lines = String(text)
    .split('\n')
    .map(function (l) { return l.trim(); })
    .filter(Boolean);

  // Prefer the .NET exception line ("System.X: message"), which is the only
  // part that says anything about the user's input.
  const exc = lines.filter(function (l) { return /^[A-Za-z.]*Exception:/.test(l); })[0];
  const pick = exc || lines.filter(function (l) { return !/^at\s/.test(l); })[0] || '';
  return pick.replace(/^(ERR:\s*)?(obfuscation failed:\s*)?/, '').replace(/\s+at\s.*$/, '').slice(0, 300);
}

// ----------------------------------------------------------------- helpers

/** /tmp is the only writable path on Vercel; os.tmpdir() usually already is. */
function realTmp() {
  const t = process.env.TMPDIR || '/tmp';
  try {
    fs.accessSync(t, fs.constants.W_OK);
    return t;
  } catch {
    return '/tmp';
  }
}

/**
 * Vercel deploys files read-only under /var/task and the exec bit is not
 * always preserved. If we cannot run the binary in place, stage a copy in /tmp.
 */
async function ensureExecutable(bin, work) {
  try {
    fs.accessSync(bin, fs.constants.X_OK);
    return bin;
  } catch {
    const staged = path.join(work, 'obf77.bin');
    await fs.promises.copyFile(bin, staged);
    fs.chmodSync(staged, 0o755);
    return staged;
  }
}

function runObfuscator(bin, work, inputFile, outputFile, settingsFile) {
  return new Promise((resolve) => {
    const child = spawn(
      bin,
      [inputFile, outputFile, settingsFile],
      {
        cwd: work,
        env: {
          PATH: `${VENDOR}:${process.env.PATH || '/usr/bin:/bin'}`,
          HOME: work,
          TMPDIR: work,
          DOTNET_ROOT: '',
          DOTNET_CLI_TELEMETRY_OPTOUT: '1',
          DOTNET_noLogo: '1',
          // Let the .NET single-file bundler extract next to the work dir
          // instead of a path that may not exist on Vercel.
          DOTNET_BUNDLE_EXTRACT_BASE_DIR: path.join(work, '.net'),
          LANG: 'C.UTF-8',
        },
        stdio: ['ignore', 'pipe', 'pipe'],
      }
    );

    let stdout = '';
    let stderr = '';
    let settled = false;

    const finish = (value) => {
      if (settled) return;
      settled = true;
      clearTimeout(timer);
      resolve(value);
    };

    const timer = setTimeout(() => {
      try { child.kill('SIGKILL'); } catch { /* ignore */ }
      finish({ ok: false, error: `timeout after ${KILL_AFTER_MS}ms`, stderr });
    }, KILL_AFTER_MS);

    child.stdout.on('data', (d) => { stdout += d; if (stdout.length > 1e6) stdout = stdout.slice(-1e6); });
    child.stderr.on('data', (d) => { stderr += d; if (stderr.length > 1e6) stderr = stderr.slice(-1e6); });

    child.on('error', (err) =>
      finish({ ok: false, error: `spawn failed: ${err.message}`, stderr })
    );

    child.on('close', (exitCode) => {
      if (exitCode === 0 && fs.existsSync(outputFile)) {
        return finish({ ok: true });
      }
      const lastErr =
        (stderr.match(/ERR:.*$/m) || [])[0] ||
        (stderr.match(/System\.[A-Za-z.]*Exception:.*/m) || [])[0] ||
        stderr.trim().split('\n').slice(-3).join(' | ') ||
        stdout.trim().split('\n').slice(-3).join(' | ') ||
        `obf77 exited with code ${exitCode}`;
      finish({ ok: false, error: lastErr.slice(0, 2000), stderr });
    });
  });
}

async function healthCheck() {
  const report = {
    ok: false,
    vendorDir: VENDOR,
    node: process.version,
    platform: `${process.platform}/${process.arch}`,
    checks: {},
  };
  for (const f of ['obf77', 'darklua', 'darkluaconfig.json', 'libLuaCompiler-O.so', 'LuaCompiler-O.dll']) {
    const p = path.join(VENDOR, f);
    try {
      const st = fs.statSync(p);
      report.checks[f] = { exists: true, bytes: st.size };
    } catch {
      report.checks[f] = { exists: false };
    }
  }
  let work = null;
  try {
    work = fs.mkdtempSync(path.join(realTmp(), 'obf9ms-h-'));
    const inputFile = path.join(work, 'input.lua');
    const outputFile = path.join(work, 'output.lua');
    fs.writeFileSync(inputFile, 'local a = 1\nprint(a + 41)\n', 'utf8');
    const settingsFile = path.join(work, 'settings.json');
    fs.writeFileSync(settingsFile, JSON.stringify(DEFAULT_SETTINGS), 'utf8');
    const bin = await ensureExecutable(BINARY, work);
    const t = Date.now();
    const r = await runObfuscator(bin, work, inputFile, outputFile, settingsFile);
    report.selfTest = {
      ...r,
      stderr: undefined,
      elapsedMs: Date.now() - t,
      outputBytes: fs.existsSync(outputFile) ? fs.statSync(outputFile).size : 0,
    };
    report.ok = !!r.ok;
  } catch (e) {
    report.selfTest = { ok: false, error: e.message };
  } finally {
    if (work) fs.rmSync(work, { recursive: true, force: true });
  }
  return report;
}

function usage() {
  return {
    ok: true,
    name: 'obf9ms',
    engine: '77fuscator 0.6.1 (C#/.NET 8, patched for Linux) + darklua',
    endpoints: {
      'GET  /api/obfuscate': 'this usage document',
      'GET  /api/obfuscate?health=1': 'runtime self-check (runs a real obfuscation)',
      'POST /api/obfuscate': 'obfuscate Lua source',
    },
    request: {
      code: 'string, required - raw Lua 5.1 source',
      settings: {
        EncryptStrings: 'bool, default true',
        DecryptTableLen: 'int, default 500',
        ExtraCompression: 'bool, default true',
        EnhancedSecurity: 'bool, default true',
        DynamicOpcodeStructure: 'bool, default false',
        Watermark: 'string, default "77fuscator 0.6.1 EARLY BUILD"',
      },
      filename: 'string, optional - used for the download name',
      download: 'true to get raw .lua text instead of JSON',
    },
    limits: {
      maxInputBytes: MAX_BODY_BYTES,
      maxOutputBytes: MAX_OUTPUT_BYTES,
      note:
        'Lua 5.1 allows at most 200 locals per function. Vercel Hobby caps ' +
        'execution at 10s; a small script costs ~3.5s including .NET cold start.',
    },
    example:
      'curl -X POST $URL/api/obfuscate -H "content-type: application/json" ' +
      '-d \'{"code":"print(1+1)"}\'',
  };
}
