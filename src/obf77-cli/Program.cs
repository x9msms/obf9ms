using geniussolution;
using geniussolution.Obfuscator;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Obf77Cli
{
    /// <summary>
    /// Thin, Linux-friendly CLI wrapper around _77F.Obfuscate().
    ///
    /// Usage:
    ///   obf77 &lt;input.lua&gt; &lt;output.lua&gt; [settings.json]
    ///   obf77 --stdin &lt;output.lua&gt; [settings.json]      (reads Lua source from stdin)
    ///   obf77 --version
    ///
    /// Everything happens inside a scratch directory under TMPDIR (/tmp on Vercel),
    /// because the obfuscator writes several side-files relative to the CWD and
    /// Vercel's /var/task is read-only.
    ///
    /// Environment:
    ///   OBF77_SCRATCH        use this directory instead of a fresh temp one
    ///   OBF77_KEEP_SCRATCH   set to 1 to leave the scratch dir behind (debugging)
    ///   DARKLUA_CONFIG       explicit path to darkluaconfig.json
    /// </summary>
    internal static class Program
    {
        private const string Version = "1.0.0";

        private static int Main(string[] args)
        {
            // 77main calls Encoding.GetEncoding(28591) in nine places but never
            // registers the code-pages provider, which throws on .NET Core / 5+.
            // Fixed here so the upstream library does not have to be touched.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Stream stdout = Console.OpenStandardOutput();
            Stream stderr = Console.OpenStandardError();

            try
            {
                if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
                {
                    Usage(stdout);
                    return args.Length == 0 ? 2 : 0;
                }

                if (args[0] == "--version")
                {
                    Write(stdout, Version + "\n");
                    return 0;
                }

                bool fromStdin = args[0] == "--stdin";
                if (args.Length < 2)
                {
                    Write(stderr, fromStdin
                        ? "ERR: --stdin requires an output path\n"
                        : "ERR: expected <input> <output> [settings.json]\n");
                    return 2;
                }

                string outPath = Path.GetFullPath(args[1]);
                string settingsPath = args.Length > 2 ? args[2] : null;

                ObfuscationSettings settings = LoadSettings(settingsPath, out string settingsError);
                if (settingsError != null)
                {
                    Write(stderr, "ERR: " + settingsError + "\n");
                    return 2;
                }

                string source = fromStdin ? Console.In.ReadToEnd() : ReadInput(args[0]);
                if (source == null)
                {
                    Write(stderr, "ERR: cannot read input '" + args[0] + "'\n");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(source))
                {
                    Write(stderr, "ERR: empty input\n");
                    return 1;
                }

                string scratch = Environment.GetEnvironmentVariable("OBF77_SCRATCH");
                if (string.IsNullOrEmpty(scratch))
                {
                    scratch = Path.Combine(
                        Path.GetTempPath(),
                        "obf77-" + Guid.NewGuid().ToString("N"));
                }
                Directory.CreateDirectory(scratch);

                try
                {
                    // _77F.Obfuscate shells out to `darklua process --config
                    // darkluaconfig.json ...` with a hard-coded relative config path,
                    // so the config has to sit in the process CWD.
                    string cfg = FindDarkluaConfig();
                    if (cfg != null)
                        File.Copy(cfg, Path.Combine(scratch, "darkluaconfig.json"), true);

                    string inputFile = Path.Combine(scratch, "input.lua");
                    File.WriteAllText(inputFile, source);

                    // darklua is resolved from PATH and the side-files are written
                    // relative to the CWD, so we really do have to chdir.
                    string previousCwd = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(scratch);

                    bool ok;
                    string error;
                    try
                    {
                        ok = _77F.Obfuscate(scratch, inputFile, settings, out error);
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(previousCwd);
                    }

                    string produced = Path.Combine(scratch, "out.lua");

                    if (!ok || !File.Exists(produced))
                    {
                        Write(stderr,
                            "ERR: obfuscation failed: " +
                            (string.IsNullOrEmpty(error) ? "no output produced" : error) + "\n");
                        return 1;
                    }

                    string outDir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrEmpty(outDir))
                        Directory.CreateDirectory(outDir);

                    File.Copy(produced, outPath, true);
                    Write(stdout, outPath + "\n");
                    return 0;
                }
                finally
                {
                    if (Environment.GetEnvironmentVariable("OBF77_KEEP_SCRATCH") != "1")
                        TryDelete(scratch);
                    else
                        Write(stderr, "SCRATCH: " + scratch + "\n");
                }
            }
            catch (Exception ex)
            {
                Write(stderr, "ERR: " + ex.GetType().Name + ": " + ex.Message + "\n");
                return 1;
            }
        }

        private static void Write(Stream s, string text)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(text);
            s.Write(bytes, 0, bytes.Length);
            s.Flush();
        }

        private static void Usage(Stream s)
        {
            Write(s,
                "obf77 " + Version + " - 77fuscator CLI (Linux)\n" +
                "\n" +
                "usage:\n" +
                "  obf77 <input.lua> <output.lua> [settings.json]\n" +
                "  obf77 --stdin <output.lua> [settings.json]\n" +
                "  obf77 --version\n" +
                "\n" +
                "settings.json keys (all optional):\n" +
                "  EncryptStrings         bool   (default true)\n" +
                "  DecryptTableLen        int    (default 500)\n" +
                "  ExtraCompression       bool   (default true)\n" +
                "  EnhancedSecurity       bool   (default true)\n" +
                "  DynamicOpcodeStructure bool   (default false)\n" +
                "  Watermark              string (default \"77fuscator 0.6.1 EARLY BUILD\")\n");
        }

        private static string ReadInput(string path)
        {
            if (path == "-") return Console.In.ReadToEnd();
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Locates darkluaconfig.json: DARKLUA_CONFIG, then next to the executable,
        /// then in the current directory.
        /// </summary>
        private static string FindDarkluaConfig()
        {
            string env = Environment.GetEnvironmentVariable("DARKLUA_CONFIG");
            if (!string.IsNullOrEmpty(env) && File.Exists(env)) return env;

            string baseDir = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "darkluaconfig.json"),
                Path.Combine(baseDir, "assets", "darkluaconfig.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "darkluaconfig.json"),
            };
            foreach (string c in candidates)
                if (File.Exists(c)) return c;
            return null;
        }

        private static ObfuscationSettings LoadSettings(string path, out string error)
        {
            error = null;
            ObfuscationSettings s = new ObfuscationSettings();
            if (string.IsNullOrEmpty(path)) return s;

            if (!File.Exists(path))
            {
                error = "settings file not found: " + path;
                return s;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("EncryptStrings", out var v) &&
                        v.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        s.EncryptStrings = v.GetBoolean();

                    if (root.TryGetProperty("DecryptTableLen", out var n) &&
                        n.ValueKind == JsonValueKind.Number)
                        s.DecryptTableLen = n.GetInt32();

                    if (root.TryGetProperty("ExtraCompression", out var c) &&
                        c.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        s.ExtraCompression = c.GetBoolean();

                    if (root.TryGetProperty("EnhancedSecurity", out var e) &&
                        e.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        s.EnhancedSecurity = e.GetBoolean();

                    if (root.TryGetProperty("DynamicOpcodeStructure", out var dp) &&
                        dp.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        s.DynamicOpcodeStructure = dp.GetBoolean();

                    if (root.TryGetProperty("Watermark", out var w) &&
                        w.ValueKind == JsonValueKind.String)
                        s.Watermark = w.GetString();
                }
            }
            catch (Exception ex)
            {
                error = "invalid settings json: " + ex.Message;
            }
            return s;
        }

        private static void TryDelete(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
            catch { /* best effort */ }
        }
    }
}
