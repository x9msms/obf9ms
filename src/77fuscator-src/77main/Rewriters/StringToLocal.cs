using Loretta.CodeAnalysis.Lua;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class StringToLocal : LuaSyntaxWalker
{
    public static Random rand = new();

    private static Encoding _fuckingLua = Encoding.GetEncoding(28591);
    public int SLen = 0;

    public string Name;

    public static string TurnBack(byte[] bytes)
    {
        List<char> decrypted = new List<char>();

        for (int i = 0; i < bytes.Length; i++)
        {
            char c = Convert.ToChar(bytes[i]);
            decrypted.Add(c);
        }

        return new string(decrypted.ToArray());
    }

    public static List<string> Rewrite(string input)
    {
        int CurrentIndex = 1;
        Regex regex = new Regex(@"(?<!--)(?<!--\[\[)(['""])((?:(?!\1)[^\\]|\\.)*)\1|\[(=*)\[(.*?)\]\3\]");
        List<string> output = new List<string>();

        var matches = regex.Matches(input);
        List<string> FuncStr = new List<string>();
        int indDiff = 0;

        List<string> ToExclude = new List<string>();
        Regex patternsss = new Regex(@"_77Watermark\(""([^""]*)""\)");
        var matches2 = patternsss.Matches(input);

        foreach (Match match in matches2)
        {
            ToExclude.Add(match.Groups[1].Value);

            matches2 = patternsss.Matches(input);
        }
        foreach (Match match in matches2)
        {
            ToExclude.Add(match.Groups[1].Value);

            matches2 = patternsss.Matches(input);
        }
        foreach (Match m in matches)
        {
            string value = m.Groups[2].Value + m.Groups[4].Value;
            if (ToExclude.Contains(value))
            {
                continue;
            }
            string before = input.Substring(0, m.Index + indDiff);
            string after = input.Substring(m.Index + indDiff + m.Length);

            string nStr = before + "(Strings_Table[" + CurrentIndex + "])";
            nStr += after;

            indDiff += nStr.Length - input.Length;
            input = nStr;
            //input = input.Replace("\"" + value + "\"", "(Strings_Table[" + CurrentIndex + "])", StringComparison.CurrentCulture).Replace("\'" + value + "\'", "(Strings_Table[" + CurrentIndex + "])", StringComparison.CurrentCulture);
            //input =   input;

            FuncStr.Add(value);
            CurrentIndex++;
            matches = regex.Matches(input);
        }

        string FullListFunc = "";
        for (var I = 0; I < FuncStr.Count; I++)
        {
            if (Regex.IsMatch(FuncStr[I], @"\\[n07xr\[\]]") || Regex.IsMatch(FuncStr[I], @"\\") || Regex.IsMatch(FuncStr[I], @"<font") || Regex.IsMatch(FuncStr[I], @"\\xF"))
            {
                FullListFunc += "\"" + FuncStr[I] + "\"" + ",";
            }
            else
            {
                FullListFunc += "[==[" + FuncStr[I] + "]==]" + ",";
            }
        }
        output.Add(input);
        output.Add(FullListFunc);
        return output;
    }
}