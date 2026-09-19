namespace geniussolution.Obfuscator.VM_Generation
{
    public class VMStrings
    {
        public static string VMP1 = @"
local BitXOR = BXOR or function(a,b)
    local p,c=1,0
    while a>0 and b>0 do
        local ra,rb=a%2,b%2
        if ra~=rb then c=c+p end
        a,b,p=(a-ra)/2,(b-rb)/2,p*2
    end
    if a<b then a=b end
    while a>0 do
        local ra=a%2
        if ra>0 then c=c+p end
        a,p=(a-ra)/2,p*2
    end
    return c
end

local function gBit(Bit, Start, End)
	if End then
		local Res = (Bit / 2 ^ (Start - 1)) % 2 ^ ((End - 1) - (Start - 1) + 1);

		return Res - Res % 1;
	else
		local Plc = 2 ^ (Start - 1);

        return (Bit % (Plc + Plc) >= Plc) and 1 or 0;
	end;
end;

local Pos = 1;

local XOR_KEY = XOR_KEY_REPLACE

local function gBits32()
    local W, X, Y, Z = Byte(ByteString, Pos, Pos + 3);

	W = BitXOR(W, XOR_KEY)
	X = BitXOR(X, XOR_KEY)
	Y = BitXOR(Y, XOR_KEY)
	Z = BitXOR(Z, XOR_KEY)

    Pos	= Pos + 4;
    return (Z*16777216) + (Y*65536) + (X*256) + W;
end;

local function gBits8()
    local F = BitXOR(Byte(ByteString, Pos, Pos), XOR_KEY);
    Pos = Pos + 1;
    return F;
end;

local BitBAND = BAnd or function(a, b)
	local result = 0;
	local bitPosition = 1;
	while a > 0 and b > 0 do
		local bitA = a % 2;
		local bitB = b % 2;
		if bitA == 1 and bitB == 1 then
			result = result + bitPosition;
		end;
		a = Floor(a / 2);
		b = Floor(b / 2);
		bitPosition = bitPosition * 2;
	end;
	return result;
end;

local __INTDIV__ = function(a,b)
    return Floor(a / b);
end;

local MOD = 2^32;

local BitBOR = BOR or function(a, b)
	local result = 0;
	local bitPosition = 1;
	while a > 0 or b > 0 do
		local bitA = a % 2;
		local bitB = b % 2;
		if bitA == 1 or bitB == 1 then
			result = result + bitPosition;
		end;
		a = Floor(a / 2);
		b = Floor(b / 2);
		bitPosition = bitPosition * 2;
	end;
	return result;
end;

local BitLSHIFT,BitRSHIFT;
BitRSHIFT = BRSHIFT or function(value, shift)
	if shift < 0 then return BitLSHIFT(value,-shift) end
	return Floor(value % MOD / 2^shift)
end;

BitLSHIFT = BLSHIFT or function(value, shift)
	if shift < 0 then return BitRSHIFT(value,-shift) end
	return (value * 2^shift) % MOD;
end;

local function gBits16()
    local W, X = Byte(ByteString, Pos, Pos + 2);

	W = BitXOR(W, XOR_KEY)
	X = BitXOR(X, XOR_KEY)

    Pos	= Pos + 2;
    return (X*256) + W;
end;

local BitNOT = function(x)
  return (-1 - x) % MOD
end

local function readU24()
	local val = 0;

	for i = 0, 2 do
		val = BitBOR(val, BitLSHIFT(BitXOR(Byte(ByteString, Pos, Pos), XOR_KEY), 8 * i));
		Pos = Pos + 1;
	end;
	return val;
end;

local Cache = {};
local function gLEB128()
	local result = 0;
	local shift = 0;
	local byteValue;
	repeat
		byteValue = BitXOR(Byte(ByteString, Pos, Pos), XOR_KEY);
		Pos = Pos + 1;
		result = result + BitBAND(byteValue, 127) * 2 ^ shift;
		shift = shift + 7;
	until BitBAND(byteValue, 128) == 0;
	return result;
end;

local PersistentStacks = {}
local function gFloat()
	local Left,Right = gBits32(),gBits32();
	if Left == 0 and Right == 0 then
		return 0;
	end;

	local IsNormal = 1;
	local Mantissa = (gBit(Right, 1, 20) * (2 ^ 32)) + Left;
	local Exponent = gBit(Right, 21, 31);
	local Sign = ((-1) ^ gBit(Right, 32));

	if (Exponent == 0) then
		if (Mantissa == 0) then
			return Sign * 0;
		else
			Exponent = 1;
			IsNormal = 0;
		end;
	elseif (Exponent == 2047) then
		if (Mantissa == 0) then
			return Sign * (1 / 0);
		else
			return Sign * (0 / 0); -- +-Q/Nan
		end;
	end;

    return Sign * 2 ^ (Exponent - 1023) * (IsNormal + (Mantissa / (2 ^ 52)))
end;

local gSizet = gBits32;
local function gString()
    local Len = gSizet();
    if (Len == 0) then
        return '';
    end;

    local Str = Sub(ByteString, Pos, Pos + Len - 1);
    Pos = Pos + Len;

	local FStr = '';
	for Idx = 1, #Str do
		--FStr = FStr .. Char(Byte(Sub(Str, Idx, Idx)))
		FStr = FStr .. Char(BitXOR(Byte(Sub(Str, Idx, Idx)), XOR_KEY));
	end

    return FStr;
end;

local gInt = gBits32;
local function _R(...) return {...}, Select('#', ...) end

local function gCrashConstant()
	return Setmetatable({}, {
		['\95\95\105\110\100\101\120'] = function() while true do end end,
		['\95\95\110\101\119\105\110\100\101\120'] = function() while true do end end,
		['\95\95\116\111\115\116\114\105\110\103'] = function() while true do end end,
	})
end

local function Deserialize()
    local Chunk ={};
	local Inst = {};
";

        public static string VMP2 = @"

local function Wrap(Chunk, Upvalues)
	VM_REPLACE_3

	return function(...)

		VM_REPLACE_1

		VM_REPLACE_2

		while true do
			Inst		= Instr[InstrPoint];
			Enum		= Inst[OP_ENUM];
			--BinaryTree_Insert";

        public static string VMP3 = @"
			InstrPoint	= InstrPoint + 1;
		end;
    end;
end;
return Wrap(Deserialize(), {})();
";
    }
}