using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace geniussolution.Obfuscator.Encryption
{
    public class Decryptor
    {
        public int[] Table;
        public int SLen = 0;

        public string Name;

        public string XorString(byte key, string input)
        {
            var sb = new StringBuilder(input.Length);

            foreach (var c in input)
                sb.Append((char)((byte)c ^ key));

            var result = sb.ToString();

            return result;
        }

        public string Encrypt(byte[] bytes)
        {
            List<byte> encrypted = new List<byte>();

            int L = Table.Length;

            for (var index = 0; index < bytes.Length; index++)
                encrypted.Add((byte)(bytes[index] ^ Table[index % L]));

            return $"(DecryptStringForm(\"{string.Join("", Table.Select(t => "\\" + t.ToString()))}\",\"{string.Join("", encrypted.Select(t => "\\" + t.ToString()))}\",{Table.Length}))";
        }

        public Decryptor(string name, int maxLen)
        {
            Random r = new Random();

            Name = name;
            Table = Enumerable.Repeat(0, maxLen).Select(i => r.Next(0, 256)).ToArray();
        }
    }

    public class ConstantEncryption
    {
        private string _src;
        private ObfuscationSettings _settings;
        private Encoding _fuckingLua = Encoding.GetEncoding(28591);

        public static string String(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        }

        public Decryptor GenerateG(MatchCollection matches)
        {
            int len = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                int l = matches[i].Length;
                if (l > len)
                    len = l;
            }

            return new Decryptor(String(50), len);
        }

        public static byte[] UnescapeLuaString(string str)
        {
            List<byte> bytes = new List<byte>();

            int i = 0;
            while (i < str.Length)
            {
                char cur = str[i++];
                if (cur == '\\')
                {
                    char next = str[i++];

                    switch (next)
                    {
                        case 'a':
                            bytes.Add((byte)'\a');
                            break;

                        case 'b':
                            bytes.Add((byte)'\b');
                            break;

                        case 'f':
                            bytes.Add((byte)'\f');
                            break;

                        case 'n':
                            bytes.Add((byte)'\n');
                            break;

                        case 'r':
                            bytes.Add((byte)'\r');
                            break;

                        case 't':
                            bytes.Add((byte)'\t');
                            break;

                        case 'v':
                            bytes.Add((byte)'\v');
                            break;

                        default:
                            {
                                if (!char.IsDigit(next))
                                    bytes.Add((byte)next);
                                else
                                {
                                    string s = next.ToString();
                                    for (int j = 0; j < 2; j++, i++)
                                    {
                                        if (i == str.Length)
                                            break;

                                        char n = str[i];
                                        if (char.IsDigit(n))
                                            s = s + n;
                                        else
                                            break;
                                    }

                                    bytes.Add((byte)int.Parse(s));
                                }

                                break;
                            }
                    }
                }
                else
                    bytes.Add((byte)cur);
            }

            return bytes.ToArray();
        }

        public string EncryptStrings()
        {
            const string encRegex = @"(['""])?(?(1)((?:[^\\]|\\.)*?)\1|\[(=*)\[(.*?)\]\3\])";

            if (_settings.EncryptStrings)
            {
                Regex r = new Regex(encRegex, RegexOptions.Singleline | RegexOptions.Compiled);

                int indDiff = 0;
                var matches = r.Matches(_src);

                Decryptor dec = GenerateG(matches);

                foreach (Match m in matches)
                {
                    string before = _src.Substring(0, m.Index + indDiff);
                    string after = _src.Substring(m.Index + indDiff + m.Length);

                    string captured = m.Groups[2].Value + m.Groups[4].Value;

                    string nStr = before + dec.Encrypt(m.Groups[2].Value != "" ? UnescapeLuaString(captured) : _fuckingLua.GetBytes(captured));
                    nStr += after;

                    indDiff += nStr.Length - _src.Length;
                    _src = nStr;
                }
            }

            return _src;
        }

        public ConstantEncryption(ObfuscationSettings settings, string source)
        {
            _settings = settings;
            _src = source;
        }
    }
}