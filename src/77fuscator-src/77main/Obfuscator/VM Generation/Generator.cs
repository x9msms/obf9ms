using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using geniussolution.Extensions;
using geniussolution.Obfuscator.New_Generation.Dynamic_Structure_Generation;
using geniussolution.Obfuscator.Opcodes;
using geniussolution.Rewriters;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using LorettaTest;
using LuaCompiler_Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace geniussolution.Obfuscator.VM_Generation
{
    public class Generator
    {
        private ObfuscationContext _context;

        public Generator(ObfuscationContext context) =>
            _context = context;

        private bool IsOpcodeUsed(Chunk prototype, VOpcode @virtual)
        {
            var isUsed = false;

            foreach (var instruction in prototype.Instructions.Where(@virtual.IsInstruction))
            {
                _context.InstructionMapping.TryAdd(instruction.OpCode, @virtual);

                instruction.CustomData = new CustomInstructionData { Opcode = @virtual };

                isUsed = true;
            }

            return prototype.Functions.Aggregate(isUsed,
                (current, sPrototype) => current | IsOpcodeUsed(sPrototype, @virtual));
        }

        public Chunk GetChunkRepresentation(string input)
        {
            File.WriteAllText("anti-tamper_INST.lua", input);

            var des = new Deserializer(LuaCompiler.Compile(input));
            var ATList = des.DecodeFile();

            ATList.Instructions.Remove(ATList.Instructions.Last());

            foreach (var instruction in ATList.Instructions)
                if (instruction.OpCode is Opcode.SetUpval or Opcode.GetUpval)
                    throw new Exception("Upvalues cannot be present in appended code");

            ATList.UpdateMappings();

            return ATList;
        }

        private void OutputListing(Chunk prototype, string depth)
        {
            Console.WriteLine(depth + "-");
            for (var index = 0; index < prototype.Instructions.Count; index++)
            {
                var x = prototype.Instructions[index];
                Console.WriteLine("{5}[{4}]: {0} {1} {2} {3} {9} {10} | {6} {7} {8}", x.OpCode, x.A, x.B, x.C, index, depth,
                    x.ConstantReference?.Data, x.KBReference?.Data, x.KCReference?.Data, x.E, x.F);
            }

            foreach (var proto in prototype.Functions) OutputListing(proto, depth + "\t");
        }

        public string GenerateVM(ObfuscationSettings settings)
        {
            Random r = new Random();

            var polymorphicOpcodes = new List<OpPolymorphic>();
            var dynamicOpcodes = new List<OpDynamic>();

            var allInstructions = new List<Instruction>();
            var allPrototypes = new HashSet<Chunk>();

            allInstructions.AddRange(_context.HeadChunk.Instructions);

            var stack = new Stack<Chunk>();

            stack.Push(_context.HeadChunk);
            while (stack.Count > 0)
            {
                var currentPrototype = stack.Pop();
                allInstructions.AddRange(currentPrototype.Instructions);
                allPrototypes.Add(currentPrototype);

                foreach (var proto in currentPrototype.Functions) stack.Push(proto);
            }

            foreach (var prototype in allPrototypes)
            {
                if (prototype is { HasAntiTamper: true, FinalAntiTamperInstruction: { } finalInstruction })
                {
                    prototype.Instructions.Insert(prototype.InstructionMap[finalInstruction] + 1, new Instruction(prototype, Opcode.PrepStackArgs));
                }
                else
                {
                    prototype.Instructions.Insert(0, new Instruction(prototype, Opcode.PrepStackArgs));
                }

                prototype.Instructions.Insert(0, new Instruction(prototype, Opcode.PrepVarargSize));

                var instructions = new List<Instruction>
                {
                    new(prototype, Opcode.PrepEnv),
                    new(prototype, Opcode.PrepTop),
                    new(prototype, Opcode.PrepLUpvals),
                    new(prototype, Opcode.PrepVararg),
                    new(prototype, Opcode.PrepPCount),
                    new(prototype, Opcode.PrepStack),
                    new(prototype, Opcode.PrepArgs),
                    new(prototype, Opcode.PrepConsts),
                    new(prototype, Opcode.PrepProtos),
                    new(prototype, Opcode.PrepArgs),
                    new(prototype, Opcode.PrepParams),
                };

                instructions.Shuffle();

                prototype.Instructions.InsertRange(0, instructions);

                prototype.UpdateMappings();

                foreach (var instruction in prototype.Instructions)
                {
                    instruction.UpdateReferences();
                }
            }

            var virtuals = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(VOpcode)))
                .Select(Activator.CreateInstance)
                .Cast<VOpcode>()
                .ToList();

            virtuals.ForEach(x => IsOpcodeUsed(_context.HeadChunk, x));

            if (settings.DynamicOpcodeStructure)
                virtuals.AddRange(new DynamicOpcodeStructureGenerator().GenerateDynamicOpcodeStructure(allInstructions, _context.HeadChunk, settings));

            foreach (var instruction in allInstructions.Where(instruction => instruction.HasObfuscation && instruction.ObfuscationType is ObfuscationType.Polymorphic))
            {
                var newData = new PolymorphicData();
                var newVirtual = new OpPolymorphic();

                newData.Virtual = newVirtual;
                newVirtual.Owner = instruction;

                instruction.HasPolymorphicData = true;
                instruction.PolymorphicData = newData;

                if (instruction.A > 40)
                {
                    newData.IsASubtract = true;
                    newData.OffsetA = RandomNumberGenerator.GetInt32(1, instruction.A - 5);
                }
                else if (instruction.A <= 1)
                {
                    newData.DontOffsetA = true;
                }
                else
                {
                    newData.OffsetA = RandomNumberGenerator.GetInt32(1, Math.Min(instruction.A, 80));
                }

                if (instruction.B > 40)
                {
                    newData.IsBSubtract = true;
                    newData.OffsetB = RandomNumberGenerator.GetInt32(1, instruction.B - 5);
                }
                else if (instruction.B <= 1)
                {
                    newData.DontOffsetB = true;
                }
                else
                {
                    newData.OffsetB = RandomNumberGenerator.GetInt32(1, Math.Min(instruction.B, 80));
                }

                if (instruction.C > 40)
                {
                    newData.IsCSubtract = true;
                    newData.OffsetC = RandomNumberGenerator.GetInt32(1, instruction.C - 5);
                }
                else if (instruction.C <= 1)
                {
                    newData.DontOffsetC = true;
                }
                else
                {
                    newData.OffsetC = RandomNumberGenerator.GetInt32(1, Math.Min(instruction.C, 80));
                }

                if (instruction.E > 40)
                {
                    newData.IsESubtract = true;
                    newData.OffsetE = RandomNumberGenerator.GetInt32(1, instruction.E - 5);
                }
                else if (instruction.E <= 1)
                {
                    newData.DontOffsetE = true;
                }
                else
                {
                    newData.OffsetE = RandomNumberGenerator.GetInt32(1, Math.Min(instruction.E, 80));
                }

                if (instruction.F > 40)
                {
                    newData.IsFSubtract = true;
                    newData.OffsetF = RandomNumberGenerator.GetInt32(1, instruction.F - 5);
                }
                else if (instruction.F <= 1)
                {
                    newData.DontOffsetF = true;
                }
                else
                {
                    newData.OffsetF = RandomNumberGenerator.GetInt32(1, Math.Min(instruction.F, 80));
                }

                var NumbGenned = RandomNumberGenerator.GetInt32(0, 5);
                var virtualOverride = "";

                if (newData.IsCSubtract)
                    virtualOverride += $"[OP_C] = Inst[OP_C] - {newData.OffsetC}, ";
                else if (newData.DontOffsetC)
                    virtualOverride += "[OP_C] = Inst[OP_C], ";
                else
                    virtualOverride += $"[OP_C] = Inst[OP_C] + {newData.OffsetC}, ";

                if (newData.IsASubtract)
                    virtualOverride += $"[OP_A] = Inst[OP_A] - {newData.OffsetA}, ";
                else if (newData.DontOffsetA)
                    virtualOverride += "[OP_A] = Inst[OP_A], ";
                else
                    virtualOverride += $"[OP_A] = Inst[OP_A] + {newData.OffsetA}, ";

                if (newData.IsBSubtract)
                    virtualOverride += $"[OP_B] = Inst[OP_B] - {newData.OffsetB}, ";
                else if (newData.DontOffsetB)
                    virtualOverride += "[OP_B] = Inst[OP_B], ";
                else
                    virtualOverride += $"[OP_B] = Inst[OP_B] + {newData.OffsetB}, ";

                if (newData.IsESubtract)
                    virtualOverride += $"[OP_E] = Inst[OP_E] - {newData.OffsetE}, ";
                else if (newData.DontOffsetE)
                    virtualOverride += "[OP_E] = Inst[OP_E], ";
                else
                    virtualOverride += $"[OP_E] = Inst[OP_E] + {newData.OffsetE}, ";

                if (newData.IsFSubtract)
                    virtualOverride += $"[OP_F] = Inst[OP_F] - {newData.OffsetF} ";
                else if (newData.DontOffsetF)
                    virtualOverride += "[OP_F] = Inst[OP_F] ";
                else
                    virtualOverride += $"[OP_F] = Inst[OP_F] + {newData.OffsetF} ";

                if (NumbGenned == RandomNumberGenerator.GetInt32(0, 5))
                {
                    var oldOverride = virtualOverride;
                    virtualOverride = "Instr[InstrPoint] = { " + oldOverride + ",[OP_ENUM] = POLY_REPLACE_ENUM";
                }
                else
                {
                    var oldOverride = virtualOverride;
                    virtualOverride = "Instr[InstrPoint] = { [OP_ENUM] = POLY_REPLACE_ENUM, " + oldOverride;
                }

                virtualOverride += "} InstrPoint = InstrPoint - 1;";

                newVirtual.ObfuscatedOverride = virtualOverride;

                virtuals.Add(newVirtual);

                polymorphicOpcodes.Add(newVirtual);
            }
            var shuffledVirtuals = new List<VOpcode>();

            foreach (var instruction in allInstructions.Where(_ => RandomNumberGenerator.GetInt32(1, 16) == RandomNumberGenerator.GetInt32(1, 16)))
            {
                //if (virtuals.Count + shuffledVirtuals.Count + 10 > 200) break;

                if ((instruction.HasObfuscation || instruction.Untouched || instruction.Reloaded || instruction.ObfuscatedBy is not null) && instruction.OpCode is not Opcode.Dynamic)
                    continue;

                if (BSGenerator.BlacklistedOpcodes.Contains(instruction.OpCode))
                    continue;

                instruction.HasObfuscation = true;

                if (instruction.InstructionType == InstructionType.ABC)
                {
                    var NumbGenned = RandomNumberGenerator.GetInt32(0, 6);
                    if (NumbGenned == 3)
                        instruction.ObfuscationType = ObfuscationType.ShuffleAC;
                    else if (NumbGenned == 6)
                        instruction.ObfuscationType = ObfuscationType.ShuffleBC;
                    else
                        instruction.ObfuscationType = ObfuscationType.ShuffleABC;
                }
                else
                {
                    instruction.ObfuscationType = ObfuscationType.ShuffleAB;
                }

                var clonedVirtual = (VOpcode)Activator.CreateInstance(instruction.CustomData!.Opcode.GetType())!;

                if (instruction.ObfuscationType == ObfuscationType.ShuffleAB)
                {
                    var signature = clonedVirtual.OverrideString;
                    signature = signature.Replace("OP_A", "REPLACEMENT_A_OP");
                    signature = signature.Replace("OP_B", "REPLACEMENT_B_OP");

                    signature = signature.Replace("REPLACEMENT_A_OP", "OP_B");
                    signature = signature.Replace("REPLACEMENT_B_OP", "OP_A");

                    clonedVirtual.OverrideString = signature;
                }
                else if (instruction.ObfuscationType == ObfuscationType.ShuffleABC)
                {
                    var signature = clonedVirtual.OverrideString;
                    signature = signature.Replace("OP_A", "REPLACEMENT_A_OP");
                    signature = signature.Replace("OP_B", "REPLACEMENT_B_OP");
                    signature = signature.Replace("OP_C", "REPLACEMENT_C_OP");

                    signature = signature.Replace("REPLACEMENT_A_OP", "OP_C");
                    signature = signature.Replace("REPLACEMENT_B_OP", "OP_A");
                    signature = signature.Replace("REPLACEMENT_C_OP", "OP_B");

                    clonedVirtual.OverrideString = signature;
                }
                else if (instruction.ObfuscationType == ObfuscationType.ShuffleAC)
                {
                    var signature = clonedVirtual.OverrideString;
                    signature = signature.Replace("OP_A", "REPLACEMENT_A_OP");
                    signature = signature.Replace("OP_C", "REPLACEMENT_C_OP");

                    signature = signature.Replace("REPLACEMENT_A_OP", "OP_C");
                    signature = signature.Replace("REPLACEMENT_C_OP", "OP_A");

                    clonedVirtual.OverrideString = signature;
                }
                else if (instruction.ObfuscationType == ObfuscationType.ShuffleBC)
                {
                    var signature = clonedVirtual.OverrideString;
                    signature = signature.Replace("OP_B", "REPLACEMENT_B_OP");
                    signature = signature.Replace("OP_C", "REPLACEMENT_C_OP");

                    signature = signature.Replace("REPLACEMENT_B_OP", "OP_C");
                    signature = signature.Replace("REPLACEMENT_C_OP", "OP_B");

                    clonedVirtual.OverrideString = signature;
                }
                else if (instruction.ObfuscationType == ObfuscationType.ShuffleBAC)
                {
                    var signature = clonedVirtual.OverrideString;
                    signature = signature.Replace("OP_A", "REPLACEMENT_A_OP");
                    signature = signature.Replace("OP_B", "REPLACEMENT_B_OP");
                    signature = signature.Replace("OP_C", "REPLACEMENT_C_OP");

                    signature = signature.Replace("REPLACEMENT_A_OP", "OP_B");
                    signature = signature.Replace("REPLACEMENT_B_OP", "OP_B");
                    signature = signature.Replace("REPLACEMENT_C_OP", "OP_A");

                    clonedVirtual.OverrideString = signature;
                }

                instruction.CustomData = new CustomInstructionData { Opcode = clonedVirtual };

                shuffledVirtuals.Add(clonedVirtual);
            }

            virtuals.AddRange(shuffledVirtuals);

            virtuals.Shuffle();

            //OutputListing(_context.HeadChunk, "");

            for (var i = 0; i < virtuals.Count; i++)
                virtuals[i].VIndex = i;

            foreach (var polymorphicOpcode in polymorphicOpcodes)
            {
                var inst = polymorphicOpcode.Owner;

                var cData = inst.CustomData;

                var virtualOpcode = cData!.Opcode;

                var opCode = virtualOpcode.VIndex;

                polymorphicOpcode.ObfuscatedOverride =
                    polymorphicOpcode.ObfuscatedOverride.Replace("POLY_REPLACE_ENUM", opCode.ToString());
            }

            var vm = "";

            var bs = new Serializer(_context, settings, virtuals).SerializeLChunk(_context.HeadChunk);

            if (settings.ExtraCompression)
                vm += "local ByteString_Full = 'BytecodeReplacementString';";

            vm += "local ByteString = GSub(Sub(ByteString_Full, 7, #ByteString_Full), '..', function(x) return Char(ToNumber(x,16))  end);\n";

            //vm += VMStrings.VMP1.Replace("XOR_KEY", _context.PrimaryXorKey.ToString());
            vm += new XorKeyRewriter(_context, settings).Visit(LuaSyntaxTree.ParseText(VMStrings.VMP1, options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot()).NormalizeWhitespace();

            for (int i = 0; i < (int)ChunkStep.StepCount; i++)
            {
                switch (_context.ChunkSteps[i])
                {
                    case ChunkStep.ParameterCount:
                        vm += "Chunk[PARAMS_CHUNK] = BitXOR(gBits16(),XOR_KEY);";
                        break;

                    case ChunkStep.Constants:
                        vm +=
                            ((LuaSyntaxTree.ParseText($@"
								local ConstCount = gBits32()
    							local Consts = {{}};

								for Idx=1,ConstCount do
									local Type=gBits8();

									if(Type=={_context.ConstantMapping[1]}) then
										Consts[Idx]=(gBits8() ~= 0);
									elseif(Type=={_context.ConstantMapping[2]}) then
										Consts[Idx] = gFloat();
									elseif(Type=={_context.ConstantMapping[3]}) then
										if gBits8() == 1 then
											Consts[Idx] = gCrashConstant()
										else
											Consts[Idx]=gString()
										end;
									end;

								end;
								Chunk[CONST_CHUNK] = Consts;
								", options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot())).NormalizeWhitespace();
                        break;

                    case ChunkStep.Instructions:
                        vm +=
                            (LuaSyntaxTree.ParseText($@"Chunk[INSTR_CHUNK] = {{}};
								for Idx=1,gLEB128() do
									local Descriptor = gBits8();
									if (gBit(Descriptor, 1, 1) == 0) then
										local Type = gBit(Descriptor, 2, 3);
										local Mask = gBit(Descriptor, 4, 6);

										local Inst=
										{{
											[OP_ENUM] = gLEB128(),
											[OP_A] = gBits16(),
										}};

										if (Type == 0) then
											Inst[OP_B] = readU24();
											Inst[OP_C] = readU24();
                                            Inst[OP_E] = gBits16();
                                            Inst[OP_F] = gBits32();
										elseif(Type==1) then
											Inst[OP_B] = gBits32();
										elseif(Type==2) then
											Inst[OP_B] = gBits32() - (2 ^ 16)
										elseif(Type==3) then
											Inst[OP_B] = gBits32() - (2 ^ 16)
											Inst[OP_C] = readU24();
                                            Inst[OP_E] = gBits16();
                                            Inst[OP_F] = gBits32();
										end;

										Chunk[INSTR_CHUNK][Idx]=Inst;
									end
								end;

								", options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot()).NormalizeWhitespace();
                        break;

                    case ChunkStep.Functions:
                        vm += "Chunk[PROTO_CHUNK] = {};for Idx=1,gLEB128() do Chunk[PROTO_CHUNK][Idx-1]=Deserialize();end;";
                        break;
                }
            }

            vm += "return Chunk;end;";

            vm = (new Flattener(true).Visit((LuaSyntaxTree.ParseText(vm, options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot()))).NormalizeWhitespace().ToFullString().Normalize();
            vm += VMStrings.VMP2;

            var vmInit1 = new List<string>
        {
            "local InstrPoint = 1;",
            "local Top;",
            "local Args;",
            "local PCount;"
        };

            vmInit1.Shuffle();

            var vmReplace1 = vmInit1.Aggregate("", (current, replace) => current + replace + "\n");

            vm = vm.Replace("VM_REPLACE_1", vmReplace1);

            var vmInit2 = new List<string>
        {
            "local Vararg;",
            "local Env;",
            "local Lupvals;",
            "local Stk;",
            "local Varargsz;",
            "local Inst;",
            "local Enum;"
        };

            vmInit2.Shuffle();

            var vmReplace2 = vmInit2.Aggregate("", (current, replace) => current + replace + "\n");

            vm = vm.Replace("VM_REPLACE_2", vmReplace2);

            var vmInit3 = new List<string>
            {
                "local Instr = Chunk[INSTR_CHUNK];",

                /*"local Const = Chunk[CONST_CHUNK];",
                "local Proto = Chunk[PROTO_CHUNK];",
                "local Params = Chunk[PARAMS_CHUNK];"*/
				"local Const;",
                "local Proto;",
                "local Params;"
            };

            vmInit3.Shuffle();

            var vmReplace3 = vmInit3.Aggregate("", (current, replace) => current + replace + "\n");

            vm = vm.Replace("VM_REPLACE_3", vmReplace3);

            var treeEntries = virtuals.Select(@virtual => new BinaryTreeEntry
            {
                SortEnum = @virtual.VIndex,
                Enum = (@virtual.VIndex).ToString(),
                Body = (SyntaxFactory.ParseCompilationUnit(@virtual.GetObfuscated(_context)).Statements)
            }).ToList();
            vm += VMStrings.VMP3;
            var usedRegisters = new List<int>();

            int getRegister(bool shift = false)
            {
                int register;

                do
                {
                    register = RandomNumberGenerator.GetInt32(1, shift ? 100 : 256);
                } while (usedRegisters.Contains(register));

                usedRegisters.Add(register);

                return register;
            }

            vm = vm.Replace("--BinaryTree_Insert", CFRewriter.RewriteControlFlow(BinaryTreeGenerator.CreateBinarySearchTree(treeEntries).NormalizeWhitespace()).ToFullString())
             .Replace("OP_ENUM", getRegister().ToString())
             .Replace("OP_A", getRegister().ToString())
             .Replace("OP_B", getRegister().ToString())
             .Replace("OP_BX", getRegister().ToString())
             .Replace("OP_C", getRegister().ToString())
             .Replace("OP_E", getRegister().ToString())
             .Replace("OP_F", getRegister().ToString())
             .Replace("OP_DATA", getRegister().ToString())
             //.Replace("BytecodeReplacementString", "77FUS|" + Convert.ToBase64String(bs))
             //.Replace("BytecodeReplacementString", CompressedToString(Compress(bs)))
             .Replace("CONST_CHUNK", getRegister().ToString())
             .Replace("PROTO_CHUNK", getRegister().ToString())
             .Replace("INSTR_CHUNK", getRegister().ToString())
             .Replace("PARAMS_CHUNK", getRegister().ToString());

            var FinalVM = settings.ExtraCompression
           ? """
              local Byte,Char,Sub,Concat,LDExp,GetFEnv,Setmetatable,Select,Unpack,ToNumber,Next,Insert,Floor,BXOR,BOR,BAnd,GSub,Abs, BRSHIFT, BLSHIFT = ...
              """
           : """
              return(function(ByteString_Full,Byte,Char,Sub,Concat,LDExp,GetFEnv,Setmetatable,Select,Unpack,ToNumber,Next,Insert,Floor,BXOR,BOR,BAnd,GSub,Abs, BRSHIFT, BLSHIFT)
              """;
            FinalVM += vm;

            if (settings.ExtraCompression)
                return GenericFlattener.CreateMainFlow(LuaSyntaxTree.ParseText(FinalVM.Replace("BytecodeReplacementString", "77FUS|" + Convert.ToHexString(bs)), options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot()).ToFullString();
            else
                return "local WatermarkVar = [[" + settings.Watermark + "]];" + GenericFlattener.CreateMainFlow(LuaSyntaxTree.ParseText(FinalVM + "end)('BytecodeReplacementString',string.byte,string.char,string.sub,table.concat,math.ldexp,getfenv or function() return _ENV end,setmetatable,select,(unpack or table.unpack),tonumber,next,table.insert,math.floor, (bit and bit.bxor) or (bit32 and bit32.bxor), (bit and bit.bor) or (bit32 and bit32.bor), (bit and bit.band) or (bit32 and bit32.band),string.gsub,math.abs, (bit and bit.rshift) or (bit32 and bit32.rshift), (bit and bit.lshift) or (bit32 and bit32.lshift));\r\n".Replace("BytecodeReplacementString", "77FUS|" + Convert.ToHexString(bs)), options: new LuaParseOptions(LuaSyntaxOptions.Lua51)).GetRoot()).ToFullString();
        }
    }
}