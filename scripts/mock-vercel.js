// Minimal stand-in for the Vercel Node runtime so the API handler can be
// exercised locally: wraps http.ServerResponse with .status() / .json() / .send()
const http = require('http');
const path = require('path');
const handler = require(path.join(__dirname, '..', 'api', 'obfuscate.js'));

function wrap(res) {
  res.statusCode = 200;
  res.status = function (c) { res.statusCode = c; return res; };
  res.json = function (o) {
    if (!res.getHeader('content-type')) res.setHeader('content-type', 'application/json');
    return res.end(JSON.stringify(o, null, 2));
  };
  res.send = function (b) { return res.end(b); };
  return res;
}

const server = http.createServer(function (req, rawRes) {
  let raw = '';
  req.on('data', function (c) { raw += c; });
  req.on('end', async function () {
    const res = wrap(rawRes);
    let body = raw;
    const ct = req.headers['content-type'] || '';
    if (ct.indexOf('application/json') !== -1 && raw) {
      try { body = JSON.parse(raw); } catch (e) { /* keep raw string */ }
    }
    const url = new URL(req.url, 'http://localhost');
    const query = {};
    url.searchParams.forEach(function (v, k) { query[k] = v; });
    try {
      await handler({ method: req.method, query: query, body: body, headers: req.headers, url: req.url }, res);
    } catch (e) {
      if (!res.writableEnded) res.status(500).json({ ok: false, error: String(e && e.stack || e) });
    }
  });
});

const PORT = process.env.PORT || 8899;
server.listen(PORT, '0.0.0.0', function () {
  console.log('mock vercel runtime listening on 0.0.0.0:' + PORT);
});
