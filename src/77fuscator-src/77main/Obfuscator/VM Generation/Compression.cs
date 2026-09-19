using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace geniussolution.Obfuscator.VM_Generation
{
    public class Compression
    {
        private static readonly Encoding LuaEncoding = Encoding.GetEncoding(28591);

        private static List<int> Compress(IEnumerable<byte> uncompressed)
        {
            var dictionary = new Dictionary<string, int>();
            for (var i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            var w = string.Empty;
            var compressed = new List<int>();

            foreach (var b in uncompressed)
            {
                var wc = w + (char)b;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    compressed.Add(dictionary[w]);
                    dictionary.Add(wc, dictionary.Count);
                    w = ((char)b).ToString();
                }
            }

            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }

        private static string ToBase36(ulong value)
        {
            const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder(13);
            do
            {
                sb.Insert(0, base36[(byte)(value % 36)]);
                value /= 36;
            } while (value != 0);

            return sb.ToString();
        }

        private static string CompressedToString(List<int> compressed)
        {
            var sb = new StringBuilder();

            foreach (var n in compressed.Select(i => ToBase36((ulong)i)))
            {
                sb.Append(ToBase36((ulong)n.Length));
                sb.Append(n);
            }

            return sb.ToString();
        }

        private static Encoding enc = Encoding.GetEncoding(28591);

        public static string UglifyFunc(string[] func, bool usespecial = false)
        {
            string uglified = "GetFEnv()[";

            var c = enc.GetBytes(func[0]);
            for (int i = 0; i < c.Length; i++)
            {
                if (RandomNumberGenerator.GetInt32(3) != 0)
                {
                    uglified += $"Char({c[i]})";
                }
                else
                {
                    string Bullshit = "\\";

                    foreach (var _ in enc.GetBytes(Convert.ToChar(c[i]).ToString()))
                    {
                        Bullshit += _ + "\\";
                    }

                    uglified += $"\"{Bullshit.Remove(Bullshit.Length - 1)}\"";
                }

                if (i != c.Length - 1)
                {
                    uglified += "..";
                }
            }
            uglified += "]";

            if (func.Count() == 2)
            {
                uglified += "[";

                var d = enc.GetBytes(func[1]);
                for (int i = 0; i < d.Length; i++)
                {
                    if (RandomNumberGenerator.GetInt32(3) != 0)
                    {
                        if (RandomNumberGenerator.GetInt32(3) != 0 && usespecial)
                        {
                            var Byted7 = 55;

                            if (d[i] == Byted7)
                            {
                                uglified += $"Char(Byte(ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),1, 1))))";
                            }
                            else if (d[i] > Byted7)
                            {
                                var Final = d[i] - Byted7;
                                uglified += $"Char(Byte(ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),1, 1))) + " + Final.ToString() + ")";
                            }
                            else if (d[i] > Byted7)
                            {
                                var Final = Byted7 - d[i];
                                uglified += $"Char(Byte(ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),1, 1))) - " + Final.ToString() + ")";
                            }
                        }
                        else
                        {
                            string Bullshit = "\\";

                            foreach (var _ in enc.GetBytes(Convert.ToChar(d[i]).ToString()))
                            {
                                Bullshit += _ + "\\";
                            }

                            uglified += $"\"{Bullshit.Remove(Bullshit.Length - 1)}\"";
                        }
                    }
                    else
                    {
                        string Bullshit = "\\";

                        foreach (var _ in enc.GetBytes(Convert.ToChar(d[i]).ToString()))
                        {
                            Bullshit += _ + "\\";
                        }

                        uglified += $"\"{Bullshit.Remove(Bullshit.Length - 1)}\"";
                    }

                    if (i != d.Length - 1)
                    {
                        uglified += "..";
                    }
                }
                uglified += "]";
            }

            return uglified;
        }

        public static string Create(string script, ObfuscationSettings settings)
        {
            string env = "getfenv or function() return _ENV end";

            string Bullshit = "\\";

            foreach (var c in enc.GetBytes("_VERSION"))
            {
                Bullshit += c + "\\";
            }

            var decompression = $"""

                            local GetFEnv = {env};
                            local Char = string.char;
                            local Ver = GetFEnv()["{Bullshit.Remove(Bullshit.Length - 1)}"];

                            local BitXOR = (({UglifyFunc(["bit"])} and {UglifyFunc(["bit", "bxor"])}) or ({UglifyFunc(["bit32"])} and {UglifyFunc(["bit32", "bxor"])})) or function(a,b)
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

                            local Byte = {UglifyFunc(["string", "byte"])};
                            local Sub = {UglifyFunc(["string", "sub"])};
                            local ToNumber = {UglifyFunc(["tonumber"])};

                            local Gsub = {UglifyFunc(["string", "gsub"], true)}
                            local Concat = {UglifyFunc(["table", "concat"], true)};
                            local LDExp = {UglifyFunc(["math", "ldexp"], true)};
                            local Setmetatable = {UglifyFunc(["setmetatable"], true)};
                            local Select  = {UglifyFunc(["select"], true)};
                            local Floor = {UglifyFunc(["math", "floor"], true)}

                            local Unpack = {UglifyFunc(["unpack"], true)} or {UglifyFunc(["table", "unpack"], true)};
                            local LoadString = {UglifyFunc(["loadstring"], true)} or {UglifyFunc(["load"], true)};

                            """;

            decompression +=
                "local function decompress(b)local c,d,e=\"\",\"\",{}local f=Byte(ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),1, 1)) - ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),2, 2))) + 207;local g={}for h=0,f-1 do g[h]=Char(h)end;local i=ToNumber(Sub(ToNumber(Byte(Sub(Ver,1, 1))),1, 1));local function k()local l=ToNumber(Sub(b, i,i),36)i=i+1;local m=ToNumber(Sub(b, i,i+l-1),36)i=i+l;return m end;c=Char(k())e[1]=c;while i<#b do local n=k()if g[n]then d=g[n]else d=c..Sub(c, 1,1)end;g[f]=c..Sub(d, 1,1)e[#e+1],c,f=d,d,f+1 end;return Concat(e)end;";
            decompression += "local ByteString=decompress('77FUS|" + CompressedToString(Compress(LuaEncoding.GetBytes(script))) +
                             "');\n";

            decompression +=
                "return LoadString(ByteString)(Byte,Char,Sub,Concat,LDExp,GetFEnv,Setmetatable,Select,Unpack,ToNumber,next,table.insert,Floor, BitXOR, (bit and bit.bor) or (bit32 and bit32.bor), (bit and bit.band) or (bit32 and bit32.band),Gsub,math.abs, (bit and bit.rshift) or (bit32 and bit32.rshift), (bit and bit.lshift) or (bit32 and bit32.lshift), ...)";

            return decompression;
        }
    }
}