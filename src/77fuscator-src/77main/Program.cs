using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using geniussolution.Obfuscator;

using geniussolution.Obfuscator.Encryption;
using geniussolution.Obfuscator.VM_Generation;
using geniussolution.Rewriters;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Experimental;
using LorettaTest;
using LuaCompiler_Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace geniussolution
{
    public static class _77F
    {
        public static Random Random = new Random();
        private static Encoding _fuckingLua = Encoding.GetEncoding(28591);

        public static string String(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        }

        public static bool Obfuscate(string path, string input, ObfuscationSettings settings, out string error)
        {
            var source = File.Exists(input) ? File.ReadAllText(input) : input;
            try
            {
                string l = Path.Combine(path, "luac.out");
                error = "";

                var FoldedRoot = LuaSyntaxTree.ParseText(source.Normalize()!, new LuaParseOptions(LuaSyntaxOptions.All)).Minify().GetRoot().NormalizeWhitespace().ConstantFold(ConstantFoldingOptions.Default);
                var FinalFold = FoldedRoot.ToFullString().Replace("\\u{0020}", " ");

                var NewestSource = LuaSyntaxTree.ParseText(FinalFold!, new LuaParseOptions(LuaSyntaxOptions.All));
                //NewestSource = NewestSource.WithRootAndOptions(new LuaIntegerSolver().Visit(NewestSource.GetRoot()), new LuaParseOptions(LuaSyntaxOptions.All));
                File.WriteAllText("input_test.lua", NewestSource.GetRoot().ConstantFold(ConstantFoldingOptions.Default).ToFullString());
                //File.WriteAllText(input, NewestSource.GetRoot().ConstantFold(ConstantFoldingOptions.Default).ToFullString());
                File.WriteAllText(input, NewestSource.GetRoot().ConstantFold(ConstantFoldingOptions.Default).ToFullString());

                Process proc = new Process
                {
                    StartInfo =
                           {
                               FileName  = $"darklua",
                               Arguments = "process --config darkluaconfig.json \"" + input + "\" \"" + input + "\"",
                               UseShellExecute = false,
                               RedirectStandardError = true,
                               RedirectStandardOutput = true
                           }
                };

                string err = "";

                proc.OutputDataReceived += (sender, args) => { err += args.Data; };
                proc.ErrorDataReceived += (sender, args) => { err += args.Data; };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

                error = err;

                if (!File.Exists(input))
                    return false;

                var inputsrc = File.Exists(input) ? File.ReadAllText(input) : input;
                var WHateverList = StringToLocal.Rewrite(inputsrc);

                var FINAL = @"
local cocaine = bit and bit.bxor or bit32 and bit32.bxor;
local GetXOR = cocaine or function(a,b)
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

local DecryptStringForm = function(fVariable, input, TableLen)
	local cache = SUPER_SECRET_GLOBAL_FOR_THE_CACHE;
	local cached = cache[input];
	if cached then
		return cached;
	end;
	local c = '';
	local e = string.sub;
	local h = string.char;
	local t = {};
	for j = 0, 255 do
		local x = h(j);
		t[j] = x;
		t[x] = j;
	end;
	local f = fVariable;
	for g = 1, #input do
		local x = (g - 1) % TableLen + 1;
		c = c .. t[GetXOR(t[e(input, g, g)], t[e(f, x, x)])];
	end;
	cache[input] = c;
	return c;
end;
local Strings_Table = { geniussolutionValuesPreloadHere };

".Replace("geniussolutionValuesPreloadHere", new ConstantEncryption(settings, WHateverList[1]).EncryptStrings()) + WHateverList[0];
                File.WriteAllText(input, FINAL);

                //var headChunk = new Deserializer(File.ReadAllBytes(l)).DecodeFile();
                var headChunk = new Deserializer(LuaCompiler.Compile(FINAL)).DecodeFile();
                var ChunkForMacros = new Deserializer(LuaCompiler.Compile(source)).DecodeFile();
                var allInstructions = new List<Instruction>();
                var allPrototypes = new HashSet<Chunk>();
                var prototypeId = 0;
                var stack = new Stack<Chunk>();
                stack.Push(headChunk);

                while (stack.Count > 0)
                {
                    ++prototypeId;

                    var currentPrototype = stack.Pop();
                    currentPrototype.PrototypeId = prototypeId;
                    allInstructions.AddRange(currentPrototype.Instructions);

                    foreach (var proto in currentPrototype.Functions) stack.Push(proto);
                }

                foreach (var instruction in allInstructions)
                {
                    instruction.Untouched = true;
                }

                BSGenerator.ObfuscateControlFlow(allPrototypes);

                new BSGenerator().EnableSecurity(headChunk, settings);

                stack = new Stack<Chunk>();
                stack.Push(headChunk);

                while (stack.Count > 0)
                {
                    ++prototypeId;

                    var currentPrototype = stack.Pop();
                    currentPrototype.PrototypeId = prototypeId;

                    allPrototypes.Add(currentPrototype);

                    foreach (var proto in currentPrototype.Functions) stack.Push(proto);
                }

                void OutputListing(Chunk prototype, string depth)
                {
                    Console.WriteLine(depth + "-");
                    for (var index = 0; index < prototype.Instructions.Count; index++)
                    {
                        var x = prototype.Instructions[index];
                        Console.WriteLine("{5}[{4}]: {0} {1} {2} {3} | {6} {7} {8}", x.OpCode, x.A, x.B, x.C, index, depth,
                            x.ConstantReference?.Data, x.KBReference?.Data, x.KCReference?.Data);
                    }

                    foreach (var proto in prototype.Functions) OutputListing(proto, depth + "\t");
                }

                //OutputListing(headChunk, "");

                ObfuscationContext context = new ObfuscationContext(headChunk);
                var vm = new Generator(context).GenerateVM(settings);
                var syntaxTree = LuaSyntaxTree.ParseText(vm, options: new LuaParseOptions(LuaSyntaxOptions.Lua51), path: path);

                File.WriteAllText("out_unmin.lua", syntaxTree.GetRoot().ToFullString().Normalize());
                File.WriteAllText(input, source);

                if (settings.ExtraCompression)
                {
                    var syntaxTree2 = LuaSyntaxTree.ParseText(Compression.Create(syntaxTree.Minify().GetRoot().ToFullString().Normalize(), settings),
                        new LuaParseOptions(LuaSyntaxOptions.All));

                    File.WriteAllText("out_unmin_compressed.lua", syntaxTree2.GetRoot().ToFullString().Normalize());

                    var Tree = GenericFlattener.CreateMainFlow(new Flattener(true).Visit(syntaxTree2.GetRoot()).NormalizeWhitespace());

                    var modifiedTree2 = LuaSyntaxTree.Create((LuaSyntaxNode)Tree);

                    syntaxTree = modifiedTree2.Minify();

                    var treestr = $"do local WatermarkVar = [[{settings.Watermark}]]; " + syntaxTree.GetRoot().ToFullString() + " end;";

                    syntaxTree = LuaSyntaxTree.ParseText(treestr);
                }

                File.WriteAllText(path + "/out.lua", syntaxTree.Minify().GetRoot().ToFullString().Normalize());

                error = "FINE";

                return true;
            }
            catch (Exception e)
            {
                File.WriteAllText(input, source);
                Console.WriteLine("ERROR");
                Console.WriteLine(e);

                error = e.ToString();
                return false;
            }
        }
    }
}