#!/usr/bin/env bash
#
# Rebuilds everything under api/vendor/ from source.
#
#   bash scripts/build-linux.sh
#
# Needs: .NET SDK 8.0+, gcc, make, curl, unzip.
# Takes a few minutes. Verifies the result by obfuscating a script and, if a
# Lua 5.1 interpreter is available, by running the output.
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VENDOR="$REPO_ROOT/api/vendor"
CLI_PROJ="$REPO_ROOT/src/obf77-cli/obf77-cli.csproj"
LUA_VERSION="5.1.5"
DARKLUA_VERSION="0.19.0"
WORK="${TMPDIR:-/tmp}/obf9ms-build.$$"

say()  { printf '\n\033[1;36m==> %s\033[0m\n' "$*"; }
die()  { printf '\n\033[1;31mERROR: %s\033[0m\n' "$*" >&2; exit 1; }

cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT

# ---------------------------------------------------------------- preflight
say "Checking toolchain"
command -v gcc    >/dev/null || die "gcc not found"
command -v make   >/dev/null || die "make not found"
command -v curl   >/dev/null || die "curl not found"
command -v unzip  >/dev/null || die "unzip not found"

if command -v dotnet >/dev/null; then
  DOTNET=dotnet
elif [ -x "$HOME/.dotnet/dotnet" ]; then
  DOTNET="$HOME/.dotnet/dotnet"
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
else
  die "dotnet SDK 8.0+ not found (tried PATH and \$HOME/.dotnet).
Install with:
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0"
fi
"$DOTNET" --version
[ -f "$CLI_PROJ" ] || die "missing $CLI_PROJ"

mkdir -p "$WORK" "$VENDOR"

# ------------------------------------------------------- 1) .NET obfuscator
say "Publishing obf77 (self-contained single-file, linux-x64)"
"$DOTNET" publish "$CLI_PROJ" \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -o "$WORK/publish" >/dev/null
install -m 0755 "$WORK/publish/obf77" "$VENDOR/obf77"
printf '    obf77  %s\n' "$(du -h "$VENDOR/obf77" | cut -f1)"

# ------------------------------------------------- 2) Lua 5.1.5 native lib
# 77main does [DllImport("LuaCompiler-O.dll")] against 26 stock Lua 5.1 C API
# symbols (luaL_newstate, lua_call, ...). The Windows LuaCompiler-O.dll shipped
# with the original archive is just Lua 5.1.5 recompiled - its bytecode is
# byte-identical to `luac` 5.1.5 apart from a stripped source name and no debug
# info - so a plain Linux build of 5.1.5 is a drop-in replacement.
say "Building Lua $LUA_VERSION as libLuaCompiler-O.so"
curl -sSL "https://www.lua.org/ftp/lua-$LUA_VERSION.tar.gz" -o "$WORK/lua.tar.gz"
tar -xzf "$WORK/lua.tar.gz" -C "$WORK"
mkdir -p "$WORK/objs"
cd "$WORK/lua-$LUA_VERSION/src"
for f in l*.c lauxlib.c linit.c; do
  case "$f" in lua.c|luac.c|print.c) continue ;; esac
  gcc -O2 -fPIC -DLUA_USE_LINUX -c "$f" -o "$WORK/objs/${f%.c}.o"
done
gcc -shared -o "$WORK/libLuaCompiler-O.so" "$WORK/objs"/*.o -ldl -lm
install -m 0755 "$WORK/libLuaCompiler-O.so" "$VENDOR/libLuaCompiler-O.so"
cd "$REPO_ROOT"

# .NET's native library probing does NOT append ".so" to a name that already
# ends in ".dll", so keep the exact DllImport name as a symlink next to the
# binary. Git stores symlinks, so this survives clone/deploy.
ln -sf libLuaCompiler-O.so "$VENDOR/LuaCompiler-O.dll"
printf '    libLuaCompiler-O.so  %s\n' "$(du -h "$VENDOR/libLuaCompiler-O.so" | cut -f1)"

# ------------------------------------------------------------- 3) darklua
say "Downloading darklua $DARKLUA_VERSION (linux-x86_64)"
curl -sSL "https://github.com/seaofvoices/darklua/releases/download/v$DARKLUA_VERSION/darklua-linux-x86_64.zip" \
  -o "$WORK/darklua.zip"
unzip -o -q "$WORK/darklua.zip" -d "$WORK/darklua"
install -m 0755 "$WORK/darklua/darklua" "$VENDOR/darklua"
"$VENDOR/darklua" --version

# darkluaconfig.json is read from the process CWD by a hard-coded relative path
# inside _77F.Obfuscate, so it has to travel with the binary.
if [ ! -f "$VENDOR/darkluaconfig.json" ]; then
  die "darkluaconfig.json missing from $VENDOR - copy it from the original archive"
fi

# ----------------------------------------------------------- 4) verification
say "Verifying: obfuscating a test script"
TESTDIR="$WORK/test"; mkdir -p "$TESTDIR"
cat > "$TESTDIR/in.lua" <<'LUA'
local playerName = "Builderman"
local function greet(who)
    return "Hello, " .. who .. "!"
end
print(greet(playerName))
for i = 1, 3 do print(i * 2) end
LUA

cd "$TESTDIR"
if env -i PATH="$VENDOR:/usr/bin:/bin" HOME="$TESTDIR" TMPDIR="$TESTDIR" \
     "$VENDOR/obf77" in.lua out.lua >run.log 2>&1; then
  printf '    ok: %s -> %s\n' "$(du -h in.lua | cut -f1)" "$(du -h out.lua | cut -f1)"
else
  cat run.log
  die "obf77 failed - see output above"
fi

# Run the result if a Lua 5.1 interpreter is reachable.
LUA_BIN=""
for c in lua5.1 lua "$WORK/lua-$LUA_VERSION/src/lua"; do
  command -v "$c" >/dev/null 2>&1 && { LUA_BIN="$c"; break; }
  [ -x "$c" ] && { LUA_BIN="$c"; break; }
done
if [ -n "$LUA_BIN" ]; then
  say "Verifying: running the obfuscated output with $LUA_BIN"
  EXPECTED="$(timeout 30 "$LUA_BIN" in.lua)"
  ACTUAL="$(timeout 60 "$LUA_BIN" out.lua)"
  if [ "$EXPECTED" = "$ACTUAL" ]; then
    printf '    identical output:\n%s\n' "$(echo "$ACTUAL" | sed 's/^/      /')"
  else
    printf 'expected:\n%s\nactual:\n%s\n' "$EXPECTED" "$ACTUAL"
    die "obfuscated output does not match the original"
  fi
else
  printf '    (no Lua 5.1 interpreter found - skipped runtime check)\n'
fi

cd "$REPO_ROOT"
say "Done"
printf 'api/vendor/ contents:\n'
ls -la "$VENDOR"
printf '\nRemember to keep the exec bit in git:\n'
printf '  git update-index --chmod=+x api/vendor/obf77 api/vendor/darklua\n'
