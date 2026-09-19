# obf9ms

**77fuscator** (Lua 5.1 obfuscator, C#/.NET 8) แพ็กเป็น **HTTP API บน Vercel**

รับโค้ด Lua เข้ามา → คืนโค้ดที่ผ่าน virtualization, เข้ารหัส string และทำ control-flow flattening

```
POST /api/obfuscate
{"code": "print(1+1)"}
→ {"ok":true,"code":"do local a=[[77fuscator...]] ...","size":{"input":12,"output":56613}}
```

---

## สารบัญ

- [ใช้งาน](#ใช้งาน)
- [หน้าเว็บทดสอบ](#หน้าเว็บทดสอบ)
- [Settings](#settings)
- [ข้อจำกัด (อ่านก่อนใช้)](#ข้อจำกัด-อ่านก่อนใช้)
- [สถานะ production (วัดจริง)](#สถานะ-production-วัดจริง)
- [Deploy ขึ้น Vercel](#deploy-ขึ้น-vercel)
- [โครงสร้างรีโพ](#โครงสร้างรีโพ)
- [ปัญหา 3 ตัวที่แก้เพื่อให้รันบน Linux/Vercel ได้](#ปัญหา-3-ตัวที่แก้เพื่อให้รันบน-linuxvercel-ได้)
- [Build ไบนารีใหม่จากซอร์ส](#build-ไบนารีใหม่จากซอร์ส)
- [สถาปัตยกรรม: ทำไมต้องมีไบนารี .NET](#สถาปัตยกรรม-ทำไมต้องมีไบนารี-net)
- [แหล่งที่มาของไฟล์ third-party](#ที่มาของไฟล์-third-party)

---

## ใช้งาน

```bash
# แบบ JSON
curl -X POST https://obf9ms.vercel.app/api/obfuscate \
  -H 'content-type: application/json' \
  -d '{"code":"local a = 1\nprint(a + 41)"}'

# รับเป็นไฟล์ .lua ตรงๆ
curl -X POST https://obf9ms.vercel.app/api/obfuscate \
  -H 'content-type: application/json' \
  -d '{"code":"print(1)","download":true,"filename":"out.lua"}' \
  -o out.lua

# ดูเอกสาร endpoint
curl https://obf9ms.vercel.app/api/obfuscate

# เช็คสุขภาพรันไทม์ (obfuscate จริง 1 ครั้ง แล้วรายงานว่าไบนารี/ไลบรารีครบไหม)
curl 'https://obf9ms.vercel.app/api/obfuscate?health=1'
```

จาก JavaScript:

```js
const r = await fetch('/api/obfuscate', {
  method: 'POST',
  headers: { 'content-type': 'application/json' },
  body: JSON.stringify({ code: 'print(1+1)' }),
});
const { ok, code: obfuscated, error } = await r.json();
```

### response

| field | ความหมาย |
|---|---|
| `ok` | `true` เมื่อสำเร็จ |
| `code` | Lua ที่ obfuscate แล้ว |
| `size.input` / `size.output` | ขนาดเป็นไบต์ |
| `elapsedMs` | เวลาที่ฝั่งเซิร์ฟเวอร์ใช้ |
| `error` | ข้อความผิดพลาด (ไม่มี stack trace ถ้าเป็นความผิดของ input) |
| `detail` | จะโผล่เฉพาะตอน error 5xx เท่านั้น |

### status code

| code | เมื่อไหร่ |
|---|---|
| `200` | สำเร็จ |
| `400` | input ว่าง / ไม่ใช่ Lua 5.1 ที่ใช้ได้ / เกินลิมิตของตัวภาษา |
| `413` | input เกิน 512 KB หรือ output เกิน 12 MB |
| `500` | ตัว obfuscator พัง (มี `detail` ให้ดู) |
| `504` | ทำไม่ทันเวลา (script ใหญ่เกินสำหรับแผนปัจจุบัน) |

---

## หน้าเว็บทดสอบ

เปิด `/` ของแอปที่ deploy แล้ว จะมีหน้าสำหรับวางโค้ด กด obfuscate ดูผลลัพธ์ และดาวน์โหลด `.lua`
ปรับ settings ได้ทุกตัว

---

## Settings

ส่งผ่าน `settings` ใน body — ทุกตัวเป็น optional

| key | ชนิด | ค่าเริ่มต้น | ผล |
|---|---|---|---|
| `EncryptStrings` | bool | `true` | เข้ารหัส string constant ทั้งหมด (XOR + ตารางถอดรหัส) |
| `DecryptTableLen` | int | `500` | ความยาวตาราง key ที่ใช้ถอด string |
| `ExtraCompression` | bool | `true` | minify + flatten ซ้ำ ทำให้เล็กลงแต่ช้าลง |
| `EnhancedSecurity` | bool | `true` | เปิด anti-tamper (ตรวจ `debug.getinfo`, metatable, traceback) |
| `DynamicOpcodeStructure` | bool | `false` | สุ่มโครงสร้าง opcode ต่อการรันหนึ่งครั้ง |
| `Watermark` | string | `"77fuscator 0.6.1 EARLY BUILD"` | ข้อความที่ฝังไว้เป็นตัวแปร local ใน output |

---

## ข้อจำกัด (อ่านก่อนใช้)

วัดจากเครื่อง linux-x64 จริง ไม่ใช่การคาดเดา:

| หัวข้อ | ค่าที่วัดได้ |
|---|---|
| เวลาต่อ 1 คำขอ (script เล็ก) | **~4.3 วินาที** วัดจริงบน Vercel Hobby (รวม .NET cold start) |
| เวลา (script 9.4 KB / 150 locals) | ~3.6 วินาที → output 90 KB |
| ขนาด input สูงสุด | 512 KB (ตั้งไว้ใน `api/obfuscate.js`) |
| ขนาด output สูงสุด | 12 MB |
| อัตราขยาย | input เล็กจะพองเป็น **~55–60 KB เสมอ** เพราะ VM + string table ถูกฝังไปด้วย |

**เรื่องที่ต้องรู้:**

1. **Vercel Hobby จำกัด function ที่ 10 วินาที** — `maxDuration: 60` ใน `vercel.json` จะมีผลเฉพาะแผน Pro
   ตัวเลข 3.5 วิข้างบนจึงพอใช้ได้บน Hobby แต่ถ้า script ใหญ่หรือเครื่อง cold start ช้าอาจชนเพดาน
2. **Lua 5.1 รับ local variable ได้สูงสุด 200 ตัวต่อหนึ่งฟังก์ชัน** — เกินนั้นจะได้
   `main function has more than 200 local variables` ซึ่งเป็นข้อจำกัดของตัวภาษา ไม่ใช่บั๊ก
   วิธีแก้คือห่อโค้ดบางส่วนใน `do ... end` หรือแยกเป็นฟังก์ชัน
3. **ต้องมี RAM พอ** — ตั้ง `memory: 1024` ไว้ใน `vercel.json` แล้ว .NET + darklua + Loretta กินเกิน 256 MB สบายๆ
4. output ต้องการ runtime ที่มี `getfenv` และ `bit32` หรือ `bit` (Lua 5.1 / Luau มีให้)
5. `EnhancedSecurity` จะเรียก `debug.getinfo` / `debug.traceback` — ถ้า environment ที่ไปรันปิด `debug` library ไว้ โค้ดจะติด anti-tamper แล้วค้างที่ `while true do end` ตามออกแบบ

---

## สถานะ production (วัดจริง)

Deploy แล้วที่ **https://obf9ms.vercel.app** — ผลการตรวจบน runtime จริงของ Vercel:

```
GET /api/obfuscate?health=1
  ok       : true
  glibc    : 2.34                 (Amazon Linux)
  node     : v24.20.0  linux/x64
  openssl  : 3.5.7  -> /lib64/libssl.so.3, /lib64/libcrypto.so.3
  selfTest : ok, ~4.4s, output 54,896 bytes
```

| เคส | ผล |
|---|---|
| `POST` script ปกติ | `200` · 135 B → 59,638 B · 4.17s · output รันด้วย Lua 5.1 ได้ผลตรงต้นฉบับทุกบรรทัด |
| request ซ้ำ 3 ครั้ง (warm) | 4.48s / 4.32s / 4.28s |
| `settings.Watermark` กำหนดเอง | `200` · watermark ปรากฏใน output จริง |
| `ExtraCompression: false` | `200` · output ใหญ่ขึ้น (8 B → 71,628 B) ตามคาด |
| `download: true` | `200` · `text/plain` + `Content-Disposition: attachment` |
| Lua ที่ไม่ถูกต้อง | `400` ใน 0.29s |
| input ว่าง | `400` |
| เกิน 200 locals | `400` พร้อมคำอธิบายวิธีแก้ |
| `GET /api/obfuscate` (usage) | `200` ใน 0.20s |

**หัวห้องที่เหลือ:** ~4.3s จากเพดาน 10s ของ Hobby → เหลือ ~5.7s
ถ้า script ใหญ่ขึ้นจนเฉียดเพดาน ทางเลือกคืออัปเกรดเป็น Pro (ได้ `maxDuration: 60` ตามที่ตั้งไว้แล้วใน `vercel.json`)

> ค่าคงที่ ~3s ต่อคำขอคือตอน .NET runtime เริ่มทำงาน ไม่ใช่เวลา obfuscate จริง
> ถ้าอยากลด ให้เปลี่ยนจาก single-file เป็น multi-file publish (วัดในเครื่องแล้วเร็วกว่า ~0.9s
> เพราะไม่ต้อง extract bundle) แลกกับขนาดที่ commit เข้ารีโพเพิ่มจาก 35 MB เป็น 73 MB

---

## Deploy ขึ้น Vercel

รีโพนี้ออนไลน์อยู่แล้วที่ https://obf9ms.vercel.app — ไบนารีทั้งหมด commit ไว้ใน `api/vendor/` (~46 MB)
ดังนั้นถ้าจะ deploy ใหม่ (fork หรือ clone) **ไม่ต้อง build อะไรเพิ่ม**

```bash
npm i -g vercel
vercel login
vercel link          # หรือสร้างโปรเจกต์ใหม่จาก dashboard
vercel deploy --prod
```

หรือ import รีโพนี้ผ่าน https://vercel.com/new — ไม่ต้องตั้ง build command, ไม่ต้องใส่ environment variable

**เช็คหลัง deploy:**

```bash
curl 'https://obf9ms.vercel.app/api/obfuscate?health=1'
```

ต้องได้ `"ok": true` และ `selfTest.ok: true`

> ถ้า `selfTest` พังด้วย `spawn failed` หรือ `Permission denied` แปลว่า exec bit หายตอน deploy —
> โค้ดมี fallback ที่ copy ไบนารีไป `/tmp` แล้ว `chmod 755` ให้อยู่แล้ว (ดู `ensureExecutable`)
> แต่ถ้ายังไม่หาย ให้รัน `git update-index --chmod=+x api/vendor/obf77 api/vendor/darklua` แล้ว push ใหม่

### ทดสอบในเครื่องก่อน deploy

```bash
node scripts/mock-vercel.js     # เปิด API ที่ :8899 โดยจำลอง Vercel req/res
curl -X POST http://127.0.0.1:8899/api/obfuscate \
  -H 'content-type: application/json' -d '{"code":"print(1+1)"}'
```

ต้องใช้ `node >= 20` — ไม่ต้องมี .NET SDK ในเครื่อง เพราะเรียกไบนารีที่ build ไว้แล้ว

---

## โครงสร้างรีโพ

```
obf9ms/
├── api/
│   ├── obfuscate.js            ← Vercel serverless function (Node) — ตัวรับ HTTP
│   └── vendor/                 ← สิ่งที่ deploy ไปด้วย (ถูกระบุใน includeFiles)
│       ├── obf77                   .NET 8 self-contained single-file, linux-x64 (35 MB)
│       ├── libLuaCompiler-O.so     Lua 5.1.5 ที่คอมไพล์เป็น shared library (229 KB)
│       ├── LuaCompiler-O.dll       symlink → libLuaCompiler-O.so (DllImport หาชื่อเดิม)
│       ├── darklua                 darklua 0.19.0 linux-x86_64 (10 MB)
│       └── darkluaconfig.json      rule ของ darklua ที่ 77fuscator ใช้
├── public/
│   └── index.html              ← หน้าเว็บทดสอบ (inline ทั้งหมด ไม่มี dependency)
├── scripts/
│   ├── mock-vercel.js          ← รัน API ในเครื่องโดยจำลอง Vercel runtime
│   └── build-linux.sh          ← build ไบนารีใหม่จากซอร์ส C#
├── src/
│   ├── 77fuscator-src/         ← ซอร์ส C# ต้นฉบับ (patched แล้ว 2 จุด ดูด้านล่าง)
│   │   ├── 77main/                 ไลบรารีหลัก: Bytecode / Obfuscator / Rewriters / VM Generation
│   │   ├── 77main CLI/             CLI ต้นฉบับ (build ไม่ได้ — ดูหมายเหตุ)
│   │   ├── Bot/                    Discord bot
│   │   └── geniussolution.sln
│   └── obf77-cli/              ← CLI ตัวใหม่ที่เขียนขึ้น (บาง, ไม่มี Discord, รับ settings.json)
├── dist/
│   └── 77fuscator NEW src.7z   ← archive ต้นฉบับ 13 MB ที่ดึงมาจาก GitHub
├── vercel.json
├── package.json
└── README.md
```

**หมายเหตุเรื่อง `src/77fuscator-src/`:** โปรเจกต์ `77main CLI` และ `Bot` ในซอร์สต้นฉบับ
reference ไปยังโฟลเดอร์ `IronBrew2` และ `IronBrew2 CLI` ซึ่ง**ไม่มีอยู่ใน archive** ทำให้ build
solution ตรงๆ ไม่ได้ — ไลบรารี `77main` ตัวเดียว build ได้ปกติ (0 error) ดังนั้น
`src/obf77-cli/` จึงถูกเขียนขึ้นใหม่ให้ reference เฉพาะ `77main`

---

## ปัญหา 3 ตัวที่แก้เพื่อให้รันบน Linux/Vercel ได้

ข้อ 1–2 คือบั๊กในซอร์สต้นฉบับ ซึ่ง build ผ่านบน Windows แต่**ถอด bytecode เพี้ยนทั้งหมด**
เมื่อรันบน .NET 8/Linux — แก้ไว้ใน `src/77fuscator-src/77main/` แล้ว
ข้อ 3 เป็นกับดักตอน cross-build สำหรับ Vercel

### 1. `Bytecode Library/Bytecode/Opcode.cs` — enum เรียงไม่ตรง Lua 5.1

กลุ่ม opcode BITWISE (Luau extras) 7 ตัวถูกแทรกไว้**กลาง enum** ระหว่าง `Pow`(17) กับ `Unm`
ทำให้ค่าตั้งแต่ 22 ขึ้นไปเลื่อนไป 5 ตำแหน่ง:

| opcode จริงใน Lua 5.1 | enum เดิมอ่านเป็น | ที่ถูก |
|---|---|---|
| 22 `OP_JMP` | `BRSHFT` | `Jmp` |
| 23 `OP_EQ` | `BNOT` | `Eq` |
| 28 `OP_CALL` | `Unm` | `Call` |
| 30 `OP_RETURN` | `Len` | `Return` |
| 36 `OP_CLOSURE` | `TestSet` | `Closure` |

**ผล:** ทุก instruction ตั้งแต่ opcode 22 ขึ้นไปถูกถอดผิด ทำให้
`BSGenerator.EnableSecurity` หา `Jmp` ที่มี `B == -1` ไม่เจอ แล้วพังด้วย
`System.InvalidOperationException: Sequence contains no matching element`

**การแก้:** ย้ายกลุ่ม BITWISE ไปไว้หลัง `VarArg` เพื่อให้ค่า 0–37 ตรงกับตาราง opcode
มาตรฐานของ Lua 5.1 พอดี

ยืนยันว่า `LuaCompiler-O.dll` emit bytecode มาตรฐานจริงโดยเทียบไบต์ต่อไบต์กับ `luac` 5.1.5:
`while true do end` ให้ `16 80 ff 7f` (JMP sBx=−1) + `1e 00 80 00` (RETURN) เหมือนกันทั้งสองตัว

### 2. `Bytecode Library/Bytecode/Deserializer.cs` — logic endianness กลับด้าน

```csharp
// เดิม
_bigEndian = ReadByte() == 0;
```

`Read()` ใช้ flag นี้ตัดสินว่าจะ reverse ไบต์ไหม:

```csharp
if (factorEndianness && (_bigEndian == BitConverter.IsLittleEndian))
    bytes = bytes.Reverse().ToArray();
```

header ของ Lua 5.1 บอก endianness ที่ไบต์ที่ 6 โดย `1` = little-endian
โค้ดเดิมจึงตั้ง `_bigEndian = true` สำหรับไฟล์ little-endian ซึ่งพอมาเจอกับ
`BitConverter.IsLittleEndian == true` บน x64 แล้วเงื่อนไขเป็นจริง → **reverse ทุกค่าที่อ่าน**
(size, จำนวน instruction, ตัว instruction เอง)

**การแก้:**

```csharp
// ใหม่ — เก็บความหมาย "ไฟล์กับเครื่อง endianness ต่างกัน" ตรงๆ
_bigEndian = (ReadByte() == 1) != BitConverter.IsLittleEndian;
```

### ผลลัพธ์หลังแก้

| ก่อน | หลัง |
|---|---|
| `while true do end` ถอดได้เป็น `BRSHFT` + `Eq` | ถอดได้เป็น `Jmp B=-1 (AsBx)` + `Return` ✓ |
| พังที่ `EnableSecurity` | obfuscate สำเร็จใน ~3.5 วิ |
| — | output รันด้วย Lua 5.1 ได้ผลลัพธ์**ตรงกับต้นฉบับทุกบรรทัด** |

> ส่วนที่ **ไม่ได้** แก้อีกจุด: `Encoding.GetEncoding(28591)` ถูกเรียก 9 แห่งแต่ไม่มีที่ไหน
> ลงทะเบียน `CodePagesEncodingProvider` ซึ่งจะทำให้ throw บน .NET Core — แก้ที่
> `src/obf77-cli/Program.cs` แทน เพื่อไม่ต้องแตะซอร์สเดิม
### 3. `api/vendor/libLuaCompiler-O.so` — ต้องคอมไพล์ให้เก่ากว่า glibc ของ Vercel

อันนี้ไม่ใช่บั๊กในซอร์ส แต่เป็นกับดักตอน build

รอบแรกคอมไพล์ Lua 5.1.5 ด้วย `gcc` บน Debian 13 (glibc 2.41) แล้ว deploy ขึ้น Vercel
ผลคือ health check รายงาน:

```
System.DllNotFoundException: Unable to load shared library 'LuaCompiler-O.dll'
   or one of its dependencies.
```

ทั้งที่ไฟล์อยู่ครบและ symlink ใช้ได้ — สาเหตุคือ symbol versioning:

| ไฟล์ | ต้องการ GLIBC สูงสุด | รันบน Vercel |
|---|---|---|
| `obf77` (.NET) | 2.16 | ✅ Microsoft บิลด์แบบ portable |
| `darklua` | 2.34 | ✅ |
| `libLuaCompiler-O.so` (gcc/Debian 13) | **2.38** | ❌ |

symbol ตัวปัญหา คือ `fmod@GLIBC_2.38`, `exp`/`log`/`pow@GLIBC_2.29`,
`dlopen`/`dlsym`/`dlerror`/`dlclose@GLIBC_2.34` — glibc 2.34 รวม libdl เข้า libc
และ 2.38 เพิ่ม `fmod` เวอร์ชันใหม่ ทำให้คอมไพล์บนดิสโทรใหม่แล้ว bind ไปหาเวอร์ชันใหม่โดยอัตโนมัติ

Vercel ใช้ Amazon Linux ที่มี glibc 2.34 → โหลด `.so` ที่ต้องการ 2.38 ไม่ได้

**การแก้:** คอมไพล์ด้วย `zig cc` ซึ่งกำหนด glibc floor ได้ตรงๆ

```bash
zig cc -target x86_64-linux-gnu.2.26 -O2 -fPIC -DLUA_USE_LINUX -c <file>.c
zig cc -target x86_64-linux-gnu.2.26 -shared -fPIC -o libLuaCompiler-O.so *.o -lm -ldl
objcopy --strip-debug libLuaCompiler-O.so   # 819 KB -> 218 KB
```

ผล: ต้องการแค่ **GLIBC_2.14** (จาก 2.38) และ export ครบทั้ง 26 symbol ที่ `Natives.cs` ใช้

`scripts/build-linux.sh` ทำทั้งหมดนี้ให้แล้ว รวมถึง **ตรวจอัตโนมัติ** ว่า
`.so` ไม่ต้องการ glibc เกิน 2.34 และ symbol ครบ — ถ้าไม่ผ่านจะ `die` ทันที

> เช็คเองได้ว่าไฟล์ที่กำลังจะ deploy ต้องการ glibc เท่าไหร่:
> ```bash
> readelf --dyn-syms -W api/vendor/libLuaCompiler-O.so | grep -oE 'GLIBC_[0-9.]+' | sort -uV
> ```
> และดูว่า Vercel ที่ deploy ไปใช้ glibc อะไรจาก `GET /api/obfuscate?health=1` (ฟิลด์ `glibc`)


---

## Build ไบนารีใหม่จากซอร์ส

ต้องมี .NET SDK 8.0 ขึ้นไป

```bash
bash scripts/build-linux.sh
```

สคริปต์จะ:

1. restore + publish `src/obf77-cli/` เป็น self-contained single-file `linux-x64`
2. copy ไปไว้ที่ `api/vendor/obf77`
3. คอมไพล์ Lua 5.1.5 เป็น `libLuaCompiler-O.so` + สร้าง symlink `LuaCompiler-O.dll`
4. ดาวน์โหลด darklua linux-x86_64
5. ทดสอบ obfuscate จริงหนึ่งครั้งเพื่อยืนยันว่าใช้ได้

**ทำไมต้องใช้ `geniussolution.slim.csproj`:** csproj เดิมของ `77main` reference
Discord.Net + DSharpPlus + SimpleBase + Tsu ไว้ แต่ grep แล้ว**ไม่มีไฟล์ .cs ใดในโปรเจกต์ใช้เลย**
ไฟล์ slim จึงเหลือแค่ Loretta (ตัว parse/rewrite Lua) ซึ่งจำเป็นจริงๆ
publish เล็กลงจาก 81 MB เหลือ 35 MB แบบ multi-file → single-file

---

## สถาปัตยกรรม: ทำไมต้องมีไบนารี .NET

Vercel Serverless Functions รันได้แค่ **Node.js / Python / Go / Ruby** — รัน .NET ตรงๆ ไม่ได้
ทางออกที่ใช้คือให้ Node function เป็นเปลือกบางๆ แล้ว `spawn` ไบนารี .NET ที่ self-contained:

```
HTTP request
   │
   ▼
api/obfuscate.js          (Node — ตรวจ input, จัดการ /tmp, แปลง error เป็น HTTP status)
   │  spawn
   ▼
api/vendor/obf77          (.NET 8 single-file, linux-x64)
   │  ├─ Loretta          parse + minify + constant-fold Lua
   │  ├─ dlopen           LuaCompiler-O.dll → libLuaCompiler-O.so (Lua 5.1.5)
   │  │                     ใช้ string.dump เอา bytecode
   │  ├─ spawn            darklua process --config darkluaconfig.json
   │  ├─ Deserializer     ถอด bytecode เป็น IR
   │  ├─ BSGenerator      control-flow + anti-tamper
   │  └─ Generator        สร้าง VM
   ▼
out.lua
```

รายละเอียดที่ทำให้รันบน Vercel ได้:

- **`/var/task` read-only** — ทุกไฟล์กลางเขียนลง `/tmp` (`fs.mkdtempSync`) แล้วลบใน `finally`
- **exec bit อาจหายตอน deploy** — `ensureExecutable()` จะ `chmod 755` สำเนาใน `/tmp` ให้เอง
- **native library resolution** — .NET หาชื่อที่ตรงกับ `DllImport("LuaCompiler-O.dll")`
  โดยไม่เติม `.so` ให้ จึงใช้ symlink `LuaCompiler-O.dll → libLuaCompiler-O.so` วางข้างๆ ไบนารี
  (ยืนยันแล้วว่ารันได้ใน `env -i` ที่ไม่มี `LD_LIBRARY_PATH`)
- **`darklua` ถูกเรียกแบบ hard-code จาก PATH** (`FileName = "darklua"`) จึงต้อง set
  `PATH` ให้รวม `api/vendor` ตอน spawn
- **`DOTNET_BUNDLE_EXTRACT_BASE_DIR`** ชี้ไป `/tmp` เพื่อให้ single-file bundler extract ได้

---

## ที่มาของไฟล์ third-party

| ไฟล์ | ที่มา |
|---|---|
| `api/vendor/darklua` | [seaofvoices/darklua](https://github.com/seaofvoices/darklua) release `v0.19.0`, asset `darklua-linux-x86_64.zip` |
| `api/vendor/darkluaconfig.json` | คัดลอกจาก archive ต้นฉบับของ 77fuscator |
| `api/vendor/libLuaCompiler-O.so` | คอมไพล์จาก Lua 5.1.5 (lua.org, MIT) ด้วย `gcc -O2 -fPIC -DLUA_USE_LINUX` |
| `src/77fuscator-src/` | ซอร์ส 77fuscator (patched 2 จุดตามที่ระบุไว้ข้างบน) |
| `dist/77fuscator NEW src.7z` | archive ต้นฉบับที่ดึงมาจาก GitHub |

> ไฟล์ `.exe` / `.dll` ของ Windows ที่มากับ archive ต้นฉบับ (`darklua.exe`, `LuaCompiler-O.dll`,
> `luac.exe`, `luau.exe`, `luajit.exe` ฯลฯ) **ไม่ได้ถูก commit เข้ารีโพนี้** เพราะรันบน Vercel ไม่ได้
> และถูกแทนที่ด้วยของเทียบเท่าฝั่ง Linux ทั้งหมดแล้ว
