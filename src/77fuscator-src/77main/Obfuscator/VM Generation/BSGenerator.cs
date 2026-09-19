using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using geniussolution.ControlFlowAns;
using geniussolution.Extensions;
using LuaCompiler_Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace geniussolution.Obfuscator.VM_Generation
{
    public class BSGenerator
    {
        public static readonly HashSet<Opcode> BlacklistedOpcodes = new()
    {
        Opcode.Jmp,
        Opcode.Eq,
        Opcode.Lt,
        Opcode.Le,
        Opcode.TForLoop,
        Opcode.ForLoop,
        Opcode.ForPrep,
        Opcode.Test,
        Opcode.TestSet,
        Opcode.LoadBool,
        Opcode.SetList,
        Opcode.Move,
        Opcode.Closure,
        Opcode.GetUpval,
        Opcode.SetUpval,
        Opcode.GetInstruction,
        Opcode.CallSpec,
        Opcode.SetTableNB,
        Opcode.SetTableNC,
        Opcode.PushAKey,
        Opcode.PushBKey,
        Opcode.Polymorphic,
        Opcode.FuncDecl1,
        Opcode.FuncDecl2,
        Opcode.FuncDecl3,
        Opcode.GetGlobal,
        Opcode.SetGlobal,
        Opcode.LoadConst,
        Opcode.PrepEnv,
        Opcode.PrepTop,
        Opcode.PrepLUpvals,
        Opcode.PrepVararg,
        Opcode.PrepPCount,
        Opcode.PrepStack,
        Opcode.PrepArgs,
        Opcode.PrepConsts,
        Opcode.PrepProtos,
        Opcode.PrepArgs,
        Opcode.PrepParams,
    };

        public HashSet<Chunk> EmbeddedChunks = new();

        public Chunk GetChunkRepresentation(string input)
        {
            File.WriteAllText("anti-tamper.lua", input);

            var des = new Deserializer(LuaCompiler.Compile(input));
            var ATList = des.DecodeFile();

            ATList.Instructions.Remove(ATList.Instructions.Last());

            foreach (var instruction in ATList.Instructions)
                if (instruction.OpCode is Opcode.SetUpval or Opcode.GetUpval)
                    throw new Exception("Upvalues cannot be present in appended code");

            ATList.UpdateMappings();

            return ATList;
        }

        private int getRegister(bool shift = false, List<int> usedRegisters = null)
        {
            int register;

            do
            {
                register = RandomNumberGenerator.GetInt32(190, shift ? 100 : 245);
            } while (usedRegisters.Contains(register));

            usedRegisters.Add(register);

            return register;
        }

        public void SubstituteOpcode(Chunk prototype, int start) // Rewrite later barely works
        {
            for (var i = start; i < prototype.Instructions.Count; i++)
            {
                var instruction = prototype.Instructions[i];
                switch (instruction.OpCode)
                {
                    /*case Opcode.GetTable:
                    {
                        if (RandomNumberGenerator.GetInt32(0, 100) / 100 >= RandomNumberGenerator.GetInt32(35, 60) / 100)
                        {
                            if (instruction.C > 255)
                                continue;

                            var Self = new Instruction(prototype, Opcode.Self)
                            {
                                A = instruction.A,
                                B = instruction.B,
                                C = instruction.C,
                               // Untouched = true,
                                HasObfuscation = true,
                            };

                            var redirectionPattern = new List<Instruction>
                                        {
                                            Self
                                        };
                            prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                            prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                        }
                        break;
                    }*/
                    case Opcode.Jmp:
                        {
                            if (RandomNumberGenerator.GetInt32(0, 5) == RandomNumberGenerator.GetInt32(0, 5))
                            {
                                var ForPrep = new Instruction(prototype, Opcode.ForPrep)
                                {
                                    Chunk = instruction.Chunk,
                                    A = RandomNumberGenerator.GetInt32(-200, -150),
                                    B = instruction.B,
                                    C = instruction.C,
                                    // Untouched = true,
                                    HasObfuscation = true,
                                };

                                var redirectionPattern = new List<Instruction>
                                        {
                                            ForPrep
                                        };
                                prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                                prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                            }
                            break;
                        }

                        /*case Opcode.Add:
                            {
                                if (RandomNumberGenerator.GetInt32(0, 100) / 100 <= RandomNumberGenerator.GetInt32(20, 40) / 100)
                                {
                                    if (instruction.B > 255 && instruction.C <= 255)
                                    {
                                        var ReverseB = new Instruction(prototype, Opcode.ReverseStkCInt)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            Untouched = true,
                                        };

                                        var Sub = new Instruction(prototype, Opcode.Sub)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.OpcodeMutation
                                        };

                                        var redirectionPattern = new List<Instruction>
                                            {
                                                ReverseB,
                                                Sub
                                            };
                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                                        prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                                    }
                                    else if (instruction.B > 255 && instruction.C > 255)
                                    {
                                        Instruction Reverse = new Instruction(prototype, Opcode.ReverseConstCInt)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            Untouched = true,
                                        };

                                        var Sub = new Instruction(prototype, Opcode.Sub)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.OpcodeMutation
                                        };

                                        var redirectionPattern = new List<Instruction>
                                            {
                                                Reverse,
                                                Sub
                                            };
                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                                        prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                                    }
                                    else if (instruction.B <= 255 && instruction.C > 255)
                                    {
                                        var ReverseB = new Instruction(prototype, Opcode.ReverseConstCInt)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            Untouched = true,
                                        };

                                        var Sub = new Instruction(prototype, Opcode.Sub)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.OpcodeMutation
                                        };

                                        var redirectionPattern = new List<Instruction>
                                            {
                                                ReverseB,
                                                Sub
                                            };
                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                                        prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                                    }
                                    else if(instruction.B <= 255 && instruction.C <= 255)
                                    {
                                        var ReverseC = new Instruction(prototype, Opcode.ReverseStkCInt)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            Untouched = true,
                                        };

                                        var Sub = new Instruction(prototype, Opcode.Sub)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.OpcodeMutation
                                        };

                                        var redirectionPattern = new List<Instruction>
                                            {
                                                ReverseC,
                                                Sub
                                            };
                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                                        prototype.Instructions.RemoveAt(prototype.Instructions.IndexOf(instruction));
                                    }
                                }
                                continue;
                            }*/
                }
            }
        }

        private static string XorString(byte key, string input)
        {
            var sb = new StringBuilder(input.Length);

            foreach (var c in input)
                sb.Append((char)((byte)c ^ key));

            var result = sb.ToString();

            return result;
        }

        public void ProtectRegion(Chunk prototype, ObfuscationSettings settings, int start, List<int> encryptionKeys)
        {
            var protectedInstructions = new List<Instruction>();
            var obfuscatedInstructions = new List<Instruction>();
            var obfuscatedGlobalInstructions = new List<Instruction>();
            var ConstantReloading = new List<Instruction>();

            for (var i = start; i < prototype.Instructions.Count; i++)
            {
                var instruction = prototype.Instructions[i];

                switch (instruction.OpCode)
                {
                    /* case Opcode.GetTable when  instruction.Chunk.Constants[instruction.B].Data is string:
                     case Opcode.SetTable when  instruction.Chunk.Constants[instruction.B].Data is string:
                         {
                            ConstantReloading.Add(instruction);
                             break;
                         }*/

                    case Opcode.GetGlobal when settings.EnhancedSecurity:
                        {
                            obfuscatedGlobalInstructions.Add(instruction);
                            break;
                        }

                    default:
                        {
                            if (BlacklistedOpcodes.Contains(instruction.OpCode))
                                break;

                            if (instruction.KBReference is not null || instruction.KCReference is not null)
                                break;

                            if (instruction.A > 255 || instruction.B > 255 || instruction.C > 255)
                                break;

                            if (!instruction.Untouched)
                                break;

                            if (settings.EnhancedSecurity && RandomNumberGenerator.GetInt32(1, 6) == RandomNumberGenerator.GetInt32(1, 6))
                                obfuscatedInstructions.Add(instruction);

                            continue;
                        }
                }
            }

            foreach (var instruction in obfuscatedGlobalInstructions)
            {
                instruction.ObfuscatedBy = instruction; // Probably works
                instruction.ObfuscationType = ObfuscationType.ObfuscatedGlobal;

                var constant = prototype.Constants[instruction.B];

                if (prototype.ObfuscatedEnvironmentNames.TryGetValue((string)constant.Data!, out var obfuscatedGlobalName))
                {
                    constant.Data = obfuscatedGlobalName;
                }
            }

            foreach (var instruction in protectedInstructions)
            {
                Instruction trapInstruction;

                switch (instruction.OpCode)
                {
                    case Opcode.GetTable:
                        {
                            trapInstruction = new Instruction(prototype, Opcode.GetTable)
                            {
                                A = 254,
                                B = 255,
                                C = 255,

                                HasLinkedInstruction = true,
                                Untouched = true
                            };
                            break;
                        }
                    case Opcode.SetTable:
                        {
                            trapInstruction = new Instruction(prototype, Opcode.SetTable)
                            {
                                A = 255,
                                B = 255,
                                C = 255,

                                HasLinkedInstruction = true,
                                Untouched = true
                            };
                            break;
                        }
                    default:
                        {
                            throw new Exception("Unsupported instruction in anti tamper");
                        }
                }

                prototype.Instructions.Insert(prototype.Instructions.IndexOf(instruction), trapInstruction);
            }

            foreach (var instruction in obfuscatedGlobalInstructions)
            {
                instruction.ObfuscatedBy = instruction; // Potentially a dumb hack
                instruction.ObfuscationType = ObfuscationType.ObfuscatedGlobal;

                var constant = prototype.Constants[instruction.B];

                if (prototype.ObfuscatedEnvironmentNames.TryGetValue((string)constant.Data!, out var obfuscatedGlobalName))
                {
                    constant.Data = obfuscatedGlobalName;
                }
            }

            foreach (var instruction in obfuscatedInstructions)
            {
                switch (RandomNumberGenerator.GetInt32(0, 5))
                //switch (5)
                {
                    case 1: /* Redirect */
                        {
                            var usedRegisters = new List<int>();
                            var RedirectInsAKey = getRegister(false, usedRegisters);
                            var getEnumAKey = getRegister(false, usedRegisters);

                            var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                            {
                                A = RedirectInsAKey,
                                B = 0, /* Post-processed in the serializer */
                                /* First instruction has custom handling */
                                HasObfuscation = true,
                                ObfuscationType = ObfuscationType.Redirect
                            };

                            instruction.ObfuscatedBy = redirectedInstruction;

                            var getEnumKey = new Instruction(prototype, Opcode.PushEnumKey)
                            {
                                A = getEnumAKey,
                                Untouched = true
                            };

                            var setTable = new Instruction(prototype, Opcode.SetTableNC)
                            {
                                A = RedirectInsAKey,
                                B = getEnumAKey,
                                C = 0, /* Post-processed in the serializer */
                                Untouched = true
                            };

                            var stackClear = new Instruction(prototype, Opcode.LoadNil)
                            {
                                A = 190,
                                B = RedirectInsAKey > getEnumAKey ? RedirectInsAKey : getEnumAKey,
                                Untouched = true
                            };

                            /*
                            local instrPointer = Instructions[PC + 1];
                            instrPointer[OP_ENUM] = A;
                            */
                            var redirectionPattern = new List<Instruction>
                    {
                        redirectedInstruction,
                        getEnumKey,
                        setTable,
                        stackClear
                    };

                            prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                            break;
                        }
                    case 2: /* Register Reload */
                        {
                            var usedRegisters = new List<int>();
                            var RedirectInsAKey = getRegister(false, usedRegisters);
                            var getAKey_ = getRegister(false, usedRegisters);
                            var getBKey_ = getRegister(false, usedRegisters);

                            var GetBiggest_1 = RedirectInsAKey > getAKey_ ? RedirectInsAKey : getAKey_;
                            var GetBiggest_2 = GetBiggest_1 > getBKey_ ? GetBiggest_1 : getBKey_;

                            var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                            {
                                A = RedirectInsAKey,
                                B = 0, /* Post-processed in the serializer */
                                /* First instruction has custom handling */
                                HasObfuscation = true,
                                ObfuscationType = ObfuscationType.RegisterReload
                            };

                            instruction.ObfuscatedBy = redirectedInstruction;

                            var getAKey = new Instruction(prototype, Opcode.PushAKey)
                            {
                                A = getAKey_,
                                Untouched = true
                            };

                            var getBKey = new Instruction(prototype, Opcode.PushBKey)
                            {
                                A = getBKey_,
                                Untouched = true
                            };

                            var setTableA = new Instruction(prototype, Opcode.SetTableNC)
                            {
                                A = RedirectInsAKey,
                                B = getAKey_,
                                C = 0, /* Post-processed in the serializer */
                                Untouched = true
                            };

                            var setTableB = new Instruction(prototype, Opcode.SetTableNC)
                            {
                                A = RedirectInsAKey,
                                B = getBKey_,
                                C = 0, /* Post-processed in the serializer */
                                Untouched = true
                            };

                            var stackClear = new Instruction(prototype, Opcode.LoadNil)
                            {
                                A = 190,
                                B = GetBiggest_2,
                                Untouched = true
                            };

                            /*
                            local instrPointer = Instructions[PC + 1];
                            instrPointer[OP_A] = A;
                            instrPointer[OP_B] = B;
                            */
                            var redirectionPattern = new List<Instruction>
                    {
                        redirectedInstruction,
                        getAKey,
                        getBKey,
                        setTableA,
                        setTableB,
                        stackClear
                    };

                            prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                            break;
                        }
                    case 3: /* Polymorphic */
                        {
                            if (RandomNumberGenerator.GetInt32(1, 6) == RandomNumberGenerator.GetInt32(1, 6))
                            {
                                instruction.HasObfuscation = true;
                                instruction.ObfuscationType = ObfuscationType.Polymorphic;
                                /* Post-processed by generator */
                            }
                            break;
                        }
                    case 4: /* Get Data from Virtualized table */
                        {
                            switch (RandomNumberGenerator.GetInt32(0, 4))
                            {
                                case 1:
                                    {
                                        var usedRegisters = new List<int>();
                                        var RedirectInsAKey = getRegister(false, usedRegisters);
                                        var getAKey_ = getRegister(false, usedRegisters);
                                        var GetBiggest_1 = RedirectInsAKey > getAKey_ ? RedirectInsAKey : getAKey_;
                                        var GetSmallest_1 = RedirectInsAKey < getAKey_ ? RedirectInsAKey : getAKey_;

                                        var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                                        {
                                            A = RedirectInsAKey,
                                            B = 0, /* Post-processed in the serializer */
                                            /* First instruction has custom handling */
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.ResetOpcodeData
                                        };

                                        instruction.ObfuscatedBy = redirectedInstruction;

                                        var getAKey = new Instruction(prototype, Opcode.PushAKey)
                                        {
                                            A = getAKey_,
                                            Untouched = true
                                        };
                                        var setTableA = new Instruction(prototype, Opcode.SetTableNC)
                                        {
                                            A = RedirectInsAKey,
                                            B = getAKey_,
                                            C = 0, /* Post-processed in the serializer */
                                            Untouched = true
                                        };

                                        var stackClear = new Instruction(prototype, Opcode.LoadNil)
                                        {
                                            A = GetSmallest_1,
                                            B = GetBiggest_1,
                                            Untouched = true
                                        };
                                        //instruction.A = RandomNumberGenerator.GetInt32(1, 255);

                                        var redirectionPattern = new List<Instruction>
                                        {
                                            redirectedInstruction,
                                            getAKey,
                                            setTableA,
                                            stackClear
                                        };

                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);

                                        break;
                                    }
                                case 2:
                                    {
                                        var usedRegisters = new List<int>();
                                        var RedirectInsAKey = getRegister(false, usedRegisters);
                                        var getBKey_ = getRegister(false, usedRegisters);
                                        var GetBiggest_1 = RedirectInsAKey > getBKey_ ? RedirectInsAKey : getBKey_;
                                        var GetSmallest_1 = RedirectInsAKey < getBKey_ ? RedirectInsAKey : getBKey_;

                                        var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                                        {
                                            A = RedirectInsAKey,
                                            B = 0, /* Post-processed in the serializer */
                                            /* First instruction has custom handling */
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.ResetOpcodeData
                                        };

                                        instruction.ObfuscatedBy = redirectedInstruction;

                                        var getBKey = new Instruction(prototype, Opcode.PushBKey)
                                        {
                                            A = getBKey_,
                                            Untouched = true
                                        };
                                        var setTableB = new Instruction(prototype, Opcode.SetTableNC)
                                        {
                                            A = RedirectInsAKey,
                                            B = getBKey_,
                                            C = 0, /* Post-processed in the serializer */
                                            Untouched = true
                                        };

                                        var stackClear = new Instruction(prototype, Opcode.LoadNil)
                                        {
                                            A = GetSmallest_1,
                                            B = GetBiggest_1,
                                            Untouched = true
                                        };
                                        //instruction.A = RandomNumberGenerator.GetInt32(1, 255);

                                        var redirectionPattern = new List<Instruction>
                                        {
                                            redirectedInstruction,
                                            getBKey,
                                            setTableB,
                                            stackClear
                                        };

                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);

                                        break;
                                    }
                                case 3:
                                    {
                                        var usedRegisters = new List<int>();
                                        var RedirectInsAKey = getRegister(false, usedRegisters);
                                        var getCKey_ = getRegister(false, usedRegisters);
                                        var getEnumKey_ = getRegister(false, usedRegisters);
                                        var GetBiggest_1 = RedirectInsAKey > getCKey_ ? RedirectInsAKey : getCKey_;
                                        var GetBiggest_2 = GetBiggest_1 > getEnumKey_ ? GetBiggest_1 : getEnumKey_;

                                        var GetSmallest_1 = RedirectInsAKey < getCKey_ ? RedirectInsAKey : getCKey_;
                                        var GetSmallest_2 = GetSmallest_1 < getEnumKey_ ? GetSmallest_1 : getEnumKey_;

                                        var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                                        {
                                            A = RedirectInsAKey,
                                            B = 0, /* Post-processed in the serializer */
                                            /* First instruction has custom handling */
                                            HasObfuscation = true,
                                            ObfuscationType = ObfuscationType.ResetOpcodeData
                                        };

                                        instruction.ObfuscatedBy = redirectedInstruction;

                                        var getCKey = new Instruction(prototype, Opcode.PushCKey)
                                        {
                                            A = getCKey_,
                                            Untouched = true
                                        };
                                        var setTableC = new Instruction(prototype, Opcode.SetTableNC)
                                        {
                                            A = RedirectInsAKey,
                                            B = getCKey_,
                                            C = 0, /* Post-processed in the serializer */
                                            Untouched = true
                                        };

                                        var getEnumKey = new Instruction(prototype, Opcode.PushEnumKey)
                                        {
                                            A = getEnumKey_,
                                            Untouched = true
                                        };
                                        var setTableEnum = new Instruction(prototype, Opcode.SetTableNC)
                                        {
                                            A = RedirectInsAKey,
                                            B = getEnumKey_,
                                            C = 0, /* Post-processed in the serializer */
                                            Untouched = true
                                        };

                                        var stackClear = new Instruction(prototype, Opcode.LoadNil)
                                        {
                                            A = GetSmallest_2,
                                            B = GetBiggest_2,
                                            Untouched = true
                                        };
                                        //instruction.A = RandomNumberGenerator.GetInt32(1, 255);

                                        var redirectionPattern = new List<Instruction>
                                        {
                                            redirectedInstruction,
                                            getCKey,
                                            setTableC,
                                            getEnumKey,
                                            setTableEnum,
                                            stackClear
                                        };

                                        prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);

                                        break;
                                    }
                            }
                            break;
                        }
                    case 5: /* Dynamic Handler */
                        {
                            var usedRegisters = new List<int>();
                            var RedirectInsAKey = getRegister(false, usedRegisters);
                            var getEnumAKey = getRegister(false, usedRegisters);

                            var redirectedInstruction = new Instruction(prototype, Opcode.GetInstruction)
                            {
                                A = RedirectInsAKey,
                                B = 0, /* Post-processed in the serializer */
                                /* First instruction has custom handling */
                                HasObfuscation = true,
                                ObfuscationType = ObfuscationType.DynamicHandler
                            };

                            instruction.ObfuscatedBy = redirectedInstruction;

                            var getEnumKey = new Instruction(prototype, Opcode.PushEnumKey)
                            {
                                A = getEnumAKey,
                                Untouched = true
                            };

                            var setTable = new Instruction(prototype, Opcode.SetTableNC)
                            {
                                A = RedirectInsAKey,
                                B = getEnumAKey,
                                C = 0, /* Post-processed in the serializer */
                                Untouched = true
                            };

                            var jumpBack = new Instruction(prototype, Opcode.Jmp)
                            {
                                B = 0, /* Post-processed in the serializer */
                                Untouched = true
                            };

                            var stackClear = new Instruction(prototype, Opcode.LoadNil)
                            {
                                A = 190,
                                B = RedirectInsAKey > getEnumAKey ? RedirectInsAKey : getEnumAKey,
                                Untouched = true
                            };

                            /*
                            local instrPointer = Instructions[PC + 1];
                            instrPointer[OP_ENUM] = A;
                            */
                            var redirectionPattern = new List<Instruction>
                    {
                        redirectedInstruction,
                        getEnumKey,
                        setTable,
                        jumpBack,
                        stackClear
                    };

                            prototype.Instructions.InsertRange(prototype.Instructions.IndexOf(instruction), redirectionPattern);
                            break;
                        }
                }
            }
        }

        public static void ObfuscateControlFlow(HashSet<Chunk> allPrototypes)
        {
            foreach (var prototype in allPrototypes)
            {
                /* Avoid obfuscating string encryption for speed */
                if (prototype.Instructions.Any(instruction => instruction.OpCode is Opcode.PushXor))
                    continue;

                var controlFlow = BlockGenerator.Generate(prototype);
                EdgeGenerator.Generate(controlFlow);

                controlFlow.UpdateMappings();

                /* Block shuffling */

                controlFlow.Blocks.Remove(controlFlow.RootBlock);
                controlFlow.Blocks.Shuffle();
                controlFlow.Blocks.Insert(0, controlFlow.RootBlock);

                for (var i = 0; i < controlFlow.Blocks.Count; i += 2)
                {
                    var block = controlFlow.Blocks[i];

                    controlFlow.Blocks.Insert(i + 1, new Block
                    {
                        BlockType = BlockType.Jump,
                        NextBlock = block.NextBlock
                    });
                }

                foreach (var block in controlFlow.Blocks)
                {
                    if (block.BlockType is BlockType.Jump && block.Instructions.Count == 0)
                    {
                        var jumpInstruction = new Instruction(prototype, Opcode.Jmp)
                        {
                            EncompassingBlock = block,
                            JumpBlock = block.NextBlock
                        };

                        block.Instructions.Add(jumpInstruction);
                    }
                }

                controlFlow.UpdateMappings();

                /* Straight line shuffling */

                foreach (var block in controlFlow.Blocks)
                {
                    if (block.BlockType is not BlockType.Straight || block.Edges.Count != 0)
                        continue;

                    var firstInstruction = block.Instructions[0];

                    block.Instructions.Shuffle();

                    for (var i = 0; i < block.Instructions.Count; i += 2)
                    {
                        var instruction = block.Instructions[i];

                        /* We reached the last instruction */
                        block.Instructions.Insert(i + 1, new Instruction(prototype, Opcode.Jmp)
                        {
                            InstructionReference = instruction.NextInstruction
                        });
                    }

                    block.Instructions.Insert(0, new Instruction(prototype, Opcode.Jmp)
                    {
                        InstructionReference = firstInstruction
                    });
                }

                controlFlow.UpdateMappings();

                prototype.Instructions = controlFlow.ReconstructInstructions();

                prototype.UpdateMappings();

                foreach (var instruction in prototype.Instructions)
                {
                    instruction.UpdateReferences();
                }
            }
        }

        public void EnableSecurity(Chunk lPrototype, ObfuscationSettings settings, bool IsMain = true)
        {
            if (!EmbeddedChunks.Add(lPrototype))
                return;

            lPrototype.HasAntiTamper = true;
            lPrototype.UpdateMappings();

            var Str = (@"
local Env = SUPER_SECRET_GLOBAL_FOR_ENVIRONMENT

local PersistentStacks = SUPER_SECRET_GLOBAL_FOR_PERSISTENT_STACKS
local PROTOTYPE_ID = SUPER_SECRET_GLOBAL_FOR_PROTOTYPE_ID

local PersistentStack = PersistentStacks[PROTOTYPE_ID]
local stack = SUPER_SECRET_GLOBAL_FOR_THE_STACK
local burner_move = stack

ENV_REPLACE

local NewEnv = Env;
if PersistentStack then
	--for i,v in pairs(PersistentStack) do
		--stack[i] = v
	--end
    stack = PersistentStack;
	while true do end --/* Postprocessed later, a little ugly. */
else
	PersistentStack = { [252] = Env }
	PersistentStacks[PROTOTYPE_ID] = PersistentStack
end

local SharedCache = SUPER_SECRET_GLOBAL_FOR_THE_CACHE;
setmetatable(SharedCache, {
	__tostring = function()
		SUPER_SECRET_GLOBAL_FOR_NEWSTACK = nil;
		while true do
		end;
	end
});

setmetatable(
    stack,
    {
        __tostring = function()
            SUPER_SECRET_GLOBAL_FOR_NEWSTACK = nil
            while true do
            end
        end
    }
)

local trap =
    setmetatable(
    {},
    {
        __tostring = function()
            SUPER_SECRET_GLOBAL_FOR_NEWSTACK = nil
            while true do
            end
        end,
        __call = function()
        end,
        __index = function(t)
            return t
        end,
        __newindex = function()
        end
    }
)

local instructions = SUPER_SECRET_GLOBAL_FOR_INSTRUCTIONS
local burner_move = instructions -- dont remove this

for i, v in pairs(instructions) do
    if type(v) == ""table"" then
        setmetatable(
            v,
            {
                __tostring = function()
                    SUPER_SECRET_GLOBAL_FOR_NEWSTACK = nil
                    while true do
                    end
                end,
            }
        )
    end
end

local constants_table = SUPER_SECRET_GLOBAL_FOR_CONSTANTS

setmetatable(
    constants_table,
    {
        __tostring = function()
            SUPER_SECRET_GLOBAL_FOR_NEWSTACK = nil
            while true do
            end
        end,
    }
)

--PersistentStack[251] = string_decryption_forms
PersistentStack[253] = SharedCache
PersistentStack[255] = trap
PersistentStack[250] = constants_table

local constants_table_move = constants_table;
local trap_move = trap;
--local string_decryption_forms_move = string_decryption_forms;
local string_cache_move = SharedCache;
 ".Replace("ENV_REPLACE",
            settings.EnhancedSecurity
                ? @"local ObfuscatedEnv = setmetatable({
    [SUPER_SECRET_GLOBAL_FIELD_1] = SUPER_SECRET_UNKNOWN_GLOBAL_1,
    [SUPER_SECRET_GLOBAL_FIELD_2] = SUPER_SECRET_UNKNOWN_GLOBAL_2,
    [SUPER_SECRET_GLOBAL_FIELD_3] = SUPER_SECRET_UNKNOWN_GLOBAL_3,
    [SUPER_SECRET_GLOBAL_FIELD_4] = SUPER_SECRET_UNKNOWN_GLOBAL_4,
    [SUPER_SECRET_GLOBAL_FIELD_5] = SUPER_SECRET_UNKNOWN_GLOBAL_5,
    [SUPER_SECRET_GLOBAL_FIELD_6] = SUPER_SECRET_UNKNOWN_GLOBAL_6,
    [SUPER_SECRET_GLOBAL_FIELD_7] = SUPER_SECRET_UNKNOWN_GLOBAL_7,
    [SUPER_SECRET_GLOBAL_FIELD_8] = SUPER_SECRET_UNKNOWN_GLOBAL_8,
    [SUPER_SECRET_GLOBAL_FIELD_9] = SUPER_SECRET_UNKNOWN_GLOBAL_9,
    [SUPER_SECRET_GLOBAL_FIELD_10] = SUPER_SECRET_UNKNOWN_GLOBAL_10
}, {
   __index = function(t,k,v)
       local MarkerConstant = ""SUPER_SECRET_CONSTANT_INDEX""
       local PersistentStacks = SUPER_SECRET_GLOBAL_FOR_PERSISTENT_STACKS
       local PROTOTYPE_ID = SUPER_SECRET_GLOBAL_FOR_PROTOTYPE_ID
       local PersistentStack = PersistentStacks[PROTOTYPE_ID]
       local Env = PersistentStack[252]

       return Env[k]
   end
})

SUPER_SECRET_GLOBAL_FOR_ENVIRONMENT = ObfuscatedEnv"
                : "SUPER_SECRET_GLOBAL_FOR_ENVIRONMENT = Env"));
            if (IsMain)
            {
                Str += @"
local ChecksRan = 0;
local DetectedTimes = 0;

local i = 0
local function interate()
	i = i + 1

	interate()
end

local function UniversalLuaChecks()
    local LocalEnv = getfenv(0)
    local Functions = {
        debug.getinfo,
        string.char,
        table.concat,
        debug.getmetatable,
        rawget,
        debug.traceback
    }
    local LibraryTables = {
        table,
        debug,
        string
    }

    local ReturnTraceback = (function()
        return string.match(debug.traceback(),'%d+');
    end)()

    for i = 1,#LibraryTables do
        local tbl = LibraryTables[i];

        if not debug then
            DetectedTimes = DetectedTimes + 1;
            interate()
        end

        local mt = debug.getmetatable(tbl)

        if mt then
            if rawget(mt, '__index') or rawget(mt, '__newindex') then
                DetectedTimes = DetectedTimes + 1;
                interate()
            else
                ChecksRan = ChecksRan + 1;
            end
        end
    end

    for i = 1,#Functions do
        local func = Functions[i];

        local Info = debug.getinfo(func);
        if Info['what'] == 'Lua' or Info['lastlinedefined'] > 0 or Info['linedefined'] > 0 or Info['short_src'] ~= '[C]' or Info['currentline'] > 0 then
            DetectedTimes = DetectedTimes + 1;
            interate()
        else
            ChecksRan = ChecksRan + 1;
        end
        if LocalEnv[func] ~= NewEnv[func] then
            DetectedTimes = DetectedTimes + 1;
            interate()
            --detected
        else
            ChecksRan = ChecksRan + 1;
		end

        local success, err = pcall(function() string.dump(func[i]) end)
		if success then
            DetectedTimes = DetectedTimes + 1;
            interate()
        else
            ChecksRan = ChecksRan + 1;
        end

    end

    local _, str = pcall(function() local a = 1 - ""bruh"" ^ 2 return ""china"" + 1; end)
    local Matched = string.gmatch(tostring(str), ':(%d*):')()
    local thisLine = tonumber(Matched)

    if string.match(debug.traceback(),':(%d*):') ~= ReturnTraceback or thisLine ~= tonumber(ReturnTraceback) then
        --print(thisLine,ReturnTraceback, string.match(debug.traceback(),':(%d*):'));
        DetectedTimes = DetectedTimes + 1;
        interate()
    else
        ChecksRan = ChecksRan + 1;
    end
end

if string.lower(_VERSION) ~= 'luau' then
    UniversalLuaChecks()
    if ChecksRan ~= 19 or DetectedTimes ~= 0 then
        interate();
        print('\7'):rep(9e9);
        while true do end;
    end
end

";
            };
            var ATList = GetChunkRepresentation(Str);

            var getEnvironment = ATList.Instructions.First(x =>
            x.OpCode is Opcode.GetGlobal &&
            ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_ENVIRONMENT");
            getEnvironment.OpCode = Opcode.GetFEnv;

            var getCache = ATList.Instructions.First(x =>
           x.OpCode is Opcode.GetGlobal &&
           ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_THE_CACHE");
            getCache.OpCode = Opcode.PushCache;

            var setEnvironment = ATList.Instructions.First(x =>
                x.OpCode is Opcode.SetGlobal &&
                ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_ENVIRONMENT");
            setEnvironment.OpCode = Opcode.SetFenv;

            ATList.Constants[getEnvironment.B].TamperConstant = true;
            ATList.Constants[getEnvironment.B].Data = "what the hell";
            ATList.Constants[setEnvironment.B].TamperConstant = true;
            ATList.Constants[setEnvironment.B].Data = "this is not a real cosntant.";
            ATList.Constants[getCache.B].TamperConstant = true;
            ATList.Constants[getCache.B].Data = "fake.";

            var getPersistentStacks = ATList.Instructions.First(x =>
               x.OpCode is Opcode.GetGlobal &&
               ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_PERSISTENT_STACKS");
            getPersistentStacks.OpCode = Opcode.PushStackPersistent;

            ATList.Constants[getPersistentStacks.B].TamperConstant = true;
            ATList.Constants[getPersistentStacks.B].Data = "im gonna explode";

            var loadPrototypeId = ATList.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_PROTOTYPE_ID");
            loadPrototypeId.OpCode = Opcode.LoadN;
            loadPrototypeId.InstructionType = InstructionType.ABx;

            ATList.Constants[loadPrototypeId.B].TamperConstant = true;
            ATList.Constants[loadPrototypeId.B].Data = "hello!";

            var getStack = ATList.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_THE_STACK");
            getStack.OpCode = Opcode.PushStack;

            ATList.Constants[getStack.B].TamperConstant = true;
            ATList.Constants[getStack.B].Data = "77fuscator be like:";

            var getInstructions = ATList.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_INSTRUCTIONS");
            getInstructions.OpCode = Opcode.PushInsts;

            ATList.Constants[getInstructions.B].TamperConstant = true;
            ATList.Constants[getInstructions.B].Data = "im gonna go homeless";

            var getConstants = ATList.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                ATList.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_CONSTANTS");
            getConstants.OpCode = Opcode.PushConsts;

            ATList.Constants[getConstants.B].TamperConstant = true;
            ATList.Constants[getConstants.B].Data = "sup yall";

            //ME SETTINGS CONSTANTS BREAKS IT
            //FIX

            loadPrototypeId.B = lPrototype.PrototypeId;

            //var stringCacheMove = ATList.Instructions[^1];
            var trapMove = ATList.Instructions[^1];
            //var stringDecryptionMove = ATList.Instructions[^3];
            var constantsTableMove = ATList.Instructions[^2];
            //var stringDecryptionMove = ATList.Instructions[^3];
            var stringCacheMove = ATList.Instructions[^3];

            stringCacheMove.A = 253;
            trapMove.A = 255;
            //stringDecryptionMove.A = 251;
            constantsTableMove.A = 250;

            foreach (var proto in ATList.Functions)
            {
                if (proto.Instructions.Count != 3)
                    continue;

                if (proto.Instructions[0] is { OpCode: Opcode.SetGlobal } setGlobal
                    && proto.Instructions[1] is { OpCode: Opcode.Jmp }
                    && proto.Instructions[2] is { OpCode: Opcode.Return }
                    && setGlobal.ConstantReference is { Data: "SUPER_SECRET_GLOBAL_FOR_NEWSTACK" } constant)
                {
                    setGlobal.OpCode = Opcode.NewStack;
                    setGlobal.A = 0;
                    setGlobal.B = 0;

                    constant.TamperConstant = true;
                    constant.Data = "no.";
                }
            }

            var stringEncryptionKeys = new List<int>
        {
            RandomNumberGenerator.GetInt32(8, 128),
            RandomNumberGenerator.GetInt32(8, 128),
            RandomNumberGenerator.GetInt32(8, 128)
        };
            if (settings.EnhancedSecurity)
            {
                var obfuscatedGlobals = lPrototype.ObfuscatedEnvironmentNames.Keys.ToList();
                obfuscatedGlobals.Shuffle();

                for (var i = 0; i < obfuscatedGlobals.Count; i++)
                {
                    var global = obfuscatedGlobals[i];

                    var globalField = ATList.Instructions.First(x =>
                        x.OpCode is Opcode.GetGlobal &&
                        (string)ATList.Constants[x.B].Data! == $"SUPER_SECRET_GLOBAL_FIELD_{i + 1}");
                    globalField.OpCode = Opcode.LoadConst;
                    ATList.Constants[globalField.B].Data = lPrototype.ObfuscatedEnvironmentNames[global];

                    var globalValue = ATList.Instructions.First(x =>
                        x.OpCode is Opcode.GetGlobal &&
                        (string)ATList.Constants[x.B].Data! == $"SUPER_SECRET_UNKNOWN_GLOBAL_{i + 1}");
                    ATList.Constants[globalValue.B].Data = global;
                }

                var indexPrototype = ATList.Functions.First(proto =>
                    proto.Constants.Count > 0 && proto.Constants[0].Data is "SUPER_SECRET_CONSTANT_INDEX");

                var getPersistentStacks2 = indexPrototype.Instructions.First(x =>
                    x.OpCode is Opcode.GetGlobal &&
                    indexPrototype.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_PERSISTENT_STACKS");
                getPersistentStacks2.OpCode = Opcode.PushStackPersistent;

                var loadPrototypeId2 = indexPrototype.Instructions.First(x =>
                    x.OpCode is Opcode.GetGlobal &&
                    indexPrototype.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_PROTOTYPE_ID");
                loadPrototypeId2.OpCode = Opcode.LoadN;
                loadPrototypeId2.InstructionType = InstructionType.ABx;

                indexPrototype.Constants[0].TamperConstant = true;
                indexPrototype.Constants[0].Data = "hmm";

                indexPrototype.Constants[getPersistentStacks2.B].TamperConstant = true;
                indexPrototype.Constants[getPersistentStacks2.B].Data = "hmm part 2";

                indexPrototype.Constants[loadPrototypeId2.B].TamperConstant = true;
                indexPrototype.Constants[loadPrototypeId2.B].Data = "hmm part 3";

                loadPrototypeId2.B = lPrototype.PrototypeId;
            }

            /*var stringDecryptForm1 = ATList.Functions[^3];

            var getKeyForm1 = stringDecryptForm1.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                stringDecryptForm1.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_FORM_1_KEY");
            getKeyForm1.OpCode = Opcode.LoadN;

            stringDecryptForm1.Constants[getKeyForm1.B].TamperConstant = true;
            stringDecryptForm1.Constants[getKeyForm1.B].Data = "aasd";
            getKeyForm1.B = stringEncryptionKeys[0];

            var getXor = stringDecryptForm1.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                stringDecryptForm1.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_XOR");
            getXor.OpCode = Opcode.PushXor;

            stringDecryptForm1.Constants[getXor.B].TamperConstant = true;
            stringDecryptForm1.Constants[getXor.B].Data = "sadasd";

            var getCacheDec = stringDecryptForm1.Instructions.First(x =>
                x.OpCode is Opcode.GetGlobal &&
                stringDecryptForm1.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_THE_CACHE");
            getCacheDec.OpCode = Opcode.PushCache;

            stringDecryptForm1.Constants[getCacheDec.B].TamperConstant = true;
            stringDecryptForm1.Constants[getCacheDec.B].Data = "sdsafsdf";
            */
            ////////////////////////

            foreach (var chunk in ATList.Functions)
            {
                EmbeddedChunks.Add(chunk);

                foreach (var inst in chunk.Instructions) inst.Untouched = true;
            }

            for (var i = ATList.Instructions.Count - 1; i >= 0; i--)
            {
                var instruction = ATList.Instructions[i];

                Console.WriteLine(instruction.OpCode);

                instruction.Untouched = true;
                instruction.SetupReferences();

                switch (instruction.OpCode)
                {
                    case Opcode.LoadConst or Opcode.GetGlobal or Opcode.SetGlobal:
                        if (lPrototype.Constants.Contains(instruction.ConstantReference!))
                            break;

                        lPrototype.Constants.Add(instruction.ConstantReference!);
                        break;

                    case Opcode.Closure:
                        if (lPrototype.Functions.Contains(instruction.PrototypeReference!))
                            break;

                        lPrototype.Functions.Add(instruction.PrototypeReference!);
                        break;

                    case Opcode.GetTable:
                    case Opcode.SetTable:
                    case Opcode.Add:
                    case Opcode.Sub:
                    case Opcode.Mul:
                    case Opcode.Div:
                    case Opcode.Mod:
                    case Opcode.Pow:
                    case Opcode.Eq:
                    case Opcode.Lt:
                    case Opcode.Le:
                    case Opcode.Self:
                        if (instruction.B > 255)
                        {
                            if (!lPrototype.Constants.Contains(instruction.KBReference!))
                            {
                                lPrototype.Constants.Add(instruction.KBReference!);
                            }
                        }

                        if (instruction.C > 255)
                        {
                            if (lPrototype.Constants.Contains(instruction.KCReference!))
                                break;

                            lPrototype.Constants.Add(instruction.KCReference!);
                        }

                        break;
                }

                instruction.Chunk = lPrototype;

                lPrototype.Instructions.Insert(0, instruction);
            }
            var antiTamperStackClear = new Instruction(lPrototype, Opcode.LoadNil)
            {
                A = 0,
                B = ATList.StackSize,
                Untouched = true
            };

            lPrototype.Instructions.Insert(ATList.Instructions.Count, antiTamperStackClear);
            lPrototype.FinalAntiTamperInstruction = antiTamperStackClear;

            var persistentStackJump = ATList.Instructions.First(x =>
           x.OpCode is Opcode.Jmp && x.B == -1);
            persistentStackJump.InstructionReference = lPrototype.Instructions[ATList.Instructions.Count];

            /*if (lPrototype is { HasAntiTamper: true, FinalAntiTamperInstruction: { } finalInstruction })
            {
                lPrototype.Instructions.Insert(lPrototype.InstructionMap[finalInstruction] + 1, new Instruction(lPrototype, Opcode.PrepStackArgs));
            }
            else
            {
                lPrototype.Instructions.Insert(0, new Instruction(lPrototype, Opcode.PrepStackArgs));
            }
            lPrototype.Instructions.Insert(0, new Instruction(lPrototype, Opcode.PrepVarargSize));

            lPrototype.UpdateMappings();*/

            foreach (var instruction in lPrototype.Instructions)
            {
                instruction.Chunk = lPrototype;
            }
            lPrototype.UpdateMappings();

            foreach (var x in lPrototype.Instructions) x.UpdateReferences();

            //START DECRYPTION FIXES

            var getCacheOp = lPrototype.Instructions.FirstOrDefault(x =>
            x.OpCode is Opcode.GetGlobal &&
            lPrototype.Constants[x.B].Data is "SUPER_SECRET_GLOBAL_FOR_THE_CACHE");

            if (getCacheOp is not null)
            {
                getCacheOp.OpCode = Opcode.PushCache;
                lPrototype.Constants[getCacheOp.B].TamperConstant = true;
                lPrototype.Constants[getCacheOp.B].Data = "THIS FUCKING SUCKS";
            }

            //END DECRYPTION FIXES

            lPrototype.UpdateMappings();

            lPrototype.Constants.Shuffle();
            lPrototype.Functions.Shuffle();

            lPrototype.UpdateMappings();

            foreach (var x in lPrototype.Instructions) x.UpdateReferences();

            ProtectRegion(lPrototype, settings, ATList.Instructions.Count + 1, stringEncryptionKeys);
            lPrototype.UpdateMappings();

            foreach (var x in lPrototype.Instructions) x.UpdateReferences();

            SubstituteOpcode(lPrototype, ATList.Instructions.Count + 1);
            lPrototype.UpdateMappings();

            foreach (var x in lPrototype.Instructions) x.UpdateReferences();

            var countmain = 0;

            foreach (var proto in lPrototype.Functions)
            {
                if (countmain >= 1)
                    EnableSecurity(proto, settings, false);
                else
                    EnableSecurity(proto, settings, true);
                countmain++;
            }
        }
    }
}