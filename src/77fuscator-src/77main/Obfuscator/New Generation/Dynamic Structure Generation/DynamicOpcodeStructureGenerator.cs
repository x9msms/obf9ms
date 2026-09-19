using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using geniussolution.Obfuscator.Opcodes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace geniussolution.Obfuscator.New_Generation.Dynamic_Structure_Generation
{
    public class DynamicOpcodeStructureGenerator
    {
        private int MutationsGenerated = 0;

        public List<VOpcode> GenerateDynamicOpcodeStructure(List<Instruction> instructions, Chunk chunk, ObfuscationSettings settings)
        {
            var virtuals = new List<VOpcode>();
            if (settings.DynamicOpcodeStructure)
                foreach (var instruction in instructions)
                {
                    if (MutationsGenerated >= (instructions.FindAll(x => x.OpCode is Opcode.SetTable).Count + instructions.FindAll(x => x.OpCode is Opcode.GetTable).Count) / 25)
                    {
                        Console.WriteLine("Generated Enough Mutations");
                        return virtuals;
                    }

                    switch (RandomNumberGenerator.GetInt32(0, 3))
                    {
                        case 1:
                            {
                                /*
                                    INSTRUCTION **GETTABLE** ALL INSTRUCTION C VARIANTS
                                */

                                if (instruction.OpCode == Opcode.GetTable && chunk.Instructions.IndexOf(instruction) > 2)
                                {
                                    var clonedVirtual = (VOpcode)Activator.CreateInstance(instruction.CustomData!.Opcode.GetType())!;
                                    var TimesDynamicGenerated = RandomNumberGenerator.GetInt32(1, 2);
                                    var WhatChanged = RandomNumberGenerator.GetInt32(0, 3);
                                    var newVirtual = new OpDynamic();

                                    var newVirutalSometimes = new OpDynamic();

                                    List<string> TableNames = new List<string>()
                                {
                                    "Stk",
                                    "Const",
                                    "Upvalues",
                                    "Cache",
                                    "Proto",
                                };

                                    List<string> OPNames = new List<string>()
                                {
                                    "OP_E",
                                    "OP_F",
                                };

                                    var UseEFirst = false;
                                    var UseFFirst = false;

                                    switch (instruction.C)
                                    {
                                        case <= 255:
                                            {
                                                if (TimesDynamicGenerated == 1)
                                                {
                                                    List<string> UsedNames = new List<string>();

                                                    if (WhatChanged == 1)
                                                    {
                                                        switch (RandomNumberGenerator.GetInt32(0, 7))
                                                        {
                                                            case 1:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]=Stk[Inst[OP_B]][{Name}[Inst[{NameOP}]][Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                            case 5:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][{Name}[Inst[{NameOP}]][Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                            case 6:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][{Name}[Inst[{NameOP}]][Inst[OP_C]]];";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    break;
                                                                }
                                                        }
                                                    }
                                                    else if (WhatChanged == 2)
                                                    {
                                                        switch (RandomNumberGenerator.GetInt32(0, 9))
                                                        {
                                                            case 1:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name2}[Inst[{NameOP2}]][Inst[OP_A]]=Stk[Inst[OP_B]][{Name}[Inst[{NameOP}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name2}[Inst[{NameOP2}]][Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][{Name2}[Inst[{NameOP2}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 5:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][{Name2}[Inst[{NameOP2}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 6:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]=Stk[Inst[OP_B]][{Name2}[Inst[{NameOP2}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 7:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]={Name2}[Inst[{NameOP2}]][Inst[OP_B]][{Name2}[Inst[{NameOP2}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                            case 8:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var Name2 = Utils.getName(TableNames, UsedNames);

                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name2}[Inst[{NameOP2}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][Inst[OP_B]][{Name2}[Inst[{NameOP2}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                    newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";
                                                                    //newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }
                                                                    Console.WriteLine("below 255 2");

                                                                    break;
                                                                }
                                                        }
                                                    }
                                                    Console.WriteLine("its 1");
                                                }
                                                else if (TimesDynamicGenerated == 2)
                                                {
                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]=Stk[Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                }

                                                break;
                                            }
                                        case > 255:
                                            {
                                                if (TimesDynamicGenerated == 1)
                                                {
                                                    List<string> UsedNames = new List<string>();

                                                    if (WhatChanged == 1)
                                                    {
                                                        switch (RandomNumberGenerator.GetInt32(0, 9))
                                                        {
                                                            case 1:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Const[{Name}[Inst[{NameOP}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Const[Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}]+1] = Inst[OP_C];";

                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }

                                                                    break;
                                                                }
                                                            case 2:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Const[{Name}[Inst[{NameOP}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][Const[Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}]+1] = Inst[OP_C];";

                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }

                                                                    break;
                                                                }
                                                            case 3:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Const[{Name}[Inst[{NameOP}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name}[Inst[{NameOP}]][Inst[OP_B]][Const[Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}]+1] = Inst[OP_C];";

                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }

                                                                    break;
                                                                }
                                                            case 4:
                                                                {
                                                                    var Name = Utils.getName(TableNames, UsedNames);
                                                                    var NameOP = Utils.getName(OPNames, UsedNames);
                                                                    string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";

                                                                    //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[Inst[OP_B]][Const[{Name}[Inst[{NameOP}] + {RandomPlus}]]];";
                                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]=Stk[Inst[OP_B]][{Name}[Inst[{NameOP}]][Inst[OP_C]]];";
                                                                    newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Const;";
                                                                    //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}]+1] = Inst[OP_C];";

                                                                    Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                    if (match.Success)
                                                                    {
                                                                        string capturedValue = match.Groups[1].Value;
                                                                        if (capturedValue == "OP_E")
                                                                            UseEFirst = true;
                                                                        //else if (capturedValue == "OP_F")
                                                                        //    UseFFirst = true;
                                                                    }

                                                                    break;
                                                                }
                                                        }
                                                    }
                                                    else if (WhatChanged == 2)
                                                    {
                                                        var RandomPlus = RandomNumberGenerator.GetInt32(255, 900);

                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                        var Name2 = Utils.getName(TableNames, UsedNames);

                                                        var NameOP = Utils.getName(OPNames, UsedNames);
                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]={Name2}[Inst[{NameOP2}]][Inst[OP_B]][Const[Inst[OP_C]]];";
                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];";

                                                        /*
                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][{Name}[Inst[{NameOP}] + {RandomPlus}]]={Name2}[Inst[{NameOP2}]][Inst[OP_B]][Const[{Name2}[Inst[{NameOP2}] + {RandomPlus}]]];";
                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_A];";
                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = {Name}[Inst[{NameOP}]];{Name2}[Inst[{NameOP2}] + {RandomPlus}] = Inst[OP_C];";
                                                        */

                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                        if (match.Success)
                                                        {
                                                            string capturedValue = match.Groups[1].Value;
                                                            if (capturedValue == "OP_E")
                                                                UseEFirst = true;
                                                        }
                                                        Console.WriteLine("above 255 2 " + UseEFirst.ToString());
                                                    }
                                                    Console.WriteLine("its 1_");
                                                }
                                                else if (TimesDynamicGenerated == 2)
                                                {
                                                    clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]]=Stk[Inst[OP_B]][Stk[Inst[OP_C]]];";
                                                }
                                                break;
                                            }
                                    }

                                    var IndexOfInstruction = chunk.Instructions.IndexOf(instruction);
                                    if (TimesDynamicGenerated == 1 && WhatChanged == 1)
                                    {
                                        var E1 = RandomNumberGenerator.GetInt32(255, 900);

                                        Instruction NewDynamic = new Instruction(chunk, Opcode.Dynamic)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            CustomData = new CustomInstructionData()
                                            {
                                                Opcode = newVirtual,
                                            }
                                        };
                                        if (UseEFirst)
                                        {
                                            NewDynamic.E = E1;
                                            instruction.E = E1;
                                        }
                                        else
                                        {
                                            NewDynamic.F = E1;
                                            instruction.F = E1;
                                        }

                                        instruction.CustomData!.Opcode = clonedVirtual;

                                        chunk.Instructions.RemoveAt(IndexOfInstruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, instruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, NewDynamic);

                                        chunk.UpdateMappings();

                                        foreach (var x in chunk.Instructions) x.UpdateReferences();

                                        virtuals.Add(newVirtual);
                                        virtuals.Add(clonedVirtual);
                                    }
                                    else if (TimesDynamicGenerated == 1 && WhatChanged == 2)
                                    {
                                        var E1 = RandomNumberGenerator.GetInt32(255, 900);
                                        var F1 = RandomNumberGenerator.GetInt32(255, 900);
                                        Instruction NewDynamic = new Instruction(chunk, Opcode.Dynamic)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            CustomData = new CustomInstructionData()
                                            {
                                                Opcode = newVirtual,
                                            }
                                        };

                                        Instruction NewDynamic2 = new Instruction(chunk, Opcode.Dynamic)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            CustomData = new CustomInstructionData()
                                            {
                                                Opcode = newVirutalSometimes,
                                            }
                                        };

                                        if (UseEFirst)
                                        {
                                            NewDynamic.E = E1;
                                            NewDynamic.F = F1;

                                            NewDynamic2.F = F1;
                                            NewDynamic2.E = E1;

                                            instruction.E = E1;
                                            instruction.F = F1;
                                        }
                                        else
                                        {
                                            NewDynamic2.F = E1;
                                            NewDynamic2.E = F1;

                                            NewDynamic.E = F1;
                                            NewDynamic.F = E1;

                                            instruction.E = F1;
                                            instruction.F = E1;
                                        }

                                        instruction.CustomData!.Opcode = clonedVirtual;

                                        chunk.Instructions.RemoveAt(IndexOfInstruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, instruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, NewDynamic2);
                                        chunk.Instructions.Insert(IndexOfInstruction, NewDynamic);

                                        chunk.UpdateMappings();

                                        foreach (var x in chunk.Instructions) x.UpdateReferences();

                                        virtuals.Add(newVirtual);
                                        virtuals.Add(newVirutalSometimes);
                                        virtuals.Add(clonedVirtual);
                                    }
                                    MutationsGenerated++;
                                    Console.WriteLine("Generated Dynamic Opcode Structure");
                                }

                                /*
                                    INSTRUCTION **SETTABLE* ALL INSTRUCTION B AND C
                                */

                                if (instruction.OpCode == Opcode.SetTable && chunk.Instructions.IndexOf(instruction) > 2)
                                {
                                    var clonedVirtual = (VOpcode)Activator.CreateInstance(instruction.CustomData!.Opcode.GetType())!;
                                    var TimesDynamicGenerated = RandomNumberGenerator.GetInt32(1, 2);
                                    var WhatChanged = RandomNumberGenerator.GetInt32(0, 3);
                                    var newVirtual = new OpDynamic();

                                    var newVirutalSometimes = new OpDynamic();

                                    List<string> TableNames = new List<string>()
                                {
                                    "Stk",
                                    "Const",
                                    "Upvalues",
                                    "Cache",
                                    "Proto",
                                };

                                    List<string> OPNames = new List<string>()
                                {
                                    "OP_E",
                                    "OP_F",
                                };

                                    var UseEFirst = false;
                                    var UseFFirst = false;

                                    switch (instruction.C)
                                    {
                                        case (<= 255):
                                            {
                                                if (instruction.B <= 255)
                                                {
                                                    if (TimesDynamicGenerated == 1)
                                                    {
                                                        List<string> UsedNames = new List<string>();

                                                        {
                                                            switch (RandomNumberGenerator.GetInt32(0, 8))
                                                            {
                                                                case 1:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][Stk[Inst[OP_B]]]=Stk[Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 2:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][{Name}[Inst[{NameOP}]][Inst[OP_B]]]=Stk[Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 3:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][{Name}[Inst[{NameOP}]][Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 4:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]][{Name}[Inst[{NameOP}]][Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 5:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]][Stk[Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 6:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]][{Name}[Inst[{NameOP}]][Inst[OP_B]]]=Stk[Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 7:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]][{Name}[Inst[{NameOP}]][Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (instruction.B > 255)
                                                {
                                                    List<string> UsedNames = new List<string>();
                                                    if (TimesDynamicGenerated == 1)
                                                    {
                                                        {
                                                            switch (RandomNumberGenerator.GetInt32(0, 6))
                                                            {
                                                                case 1:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        var Name2 = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][{Name2}[Inst[{NameOP2}]][Inst[OP_B]]]=Stk[Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = Const;";

                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 2:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        var Name2 = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"Stk[Inst[OP_A]][{Name2}[Inst[{NameOP2}]][Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = Const;";

                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 3:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        var Name2 = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][{Name2}[Inst[{NameOP2}]][Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = Const;";

                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 4:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        var Name2 = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][Const[Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = Const;";

                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                case 5:
                                                                    {
                                                                        var Name = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP = Utils.getName(OPNames, UsedNames);

                                                                        var Name2 = Utils.getName(TableNames, UsedNames);
                                                                        var NameOP2 = Utils.getName(OPNames, UsedNames);

                                                                        //clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]]=Stk[{Name}[Inst[{NameOP}] + {RandomPlus}]][Stk[Inst[OP_C]]];";
                                                                        clonedVirtual.OverrideString = $@"{Name}[Inst[{NameOP}]][Inst[OP_A]][Const[Inst[OP_B]]]={Name}[Inst[{NameOP}]][Inst[OP_C]];";
                                                                        //newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;{Name}[Inst[{NameOP}] + {RandomPlus}] = Inst[OP_B];";
                                                                        newVirtual.ObfuscatedOverride = $@"{Name}[Inst[{NameOP}]] = Stk;";
                                                                        newVirutalSometimes.ObfuscatedOverride = $@"{Name2}[Inst[{NameOP2}]] = Const;";

                                                                        string pattern = $@"{Name}\[Inst\[(?!OP_A|OP_B|OP_C)(.*?)\]\]";
                                                                        Match match = Regex.Match(clonedVirtual.OverrideString, pattern);
                                                                        if (match.Success)
                                                                        {
                                                                            string capturedValue = match.Groups[1].Value;
                                                                            if (capturedValue == "OP_E")
                                                                                UseEFirst = true;
                                                                            //else if (capturedValue == "OP_F")
                                                                            //    UseFFirst = true;
                                                                        }
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }
                                                }

                                                break;
                                            }
                                    }
                                    var IndexOfInstruction = chunk.Instructions.IndexOf(instruction);
                                    if (TimesDynamicGenerated == 1)
                                    {
                                        var E1 = RandomNumberGenerator.GetInt32(255, 900);
                                        var F1 = RandomNumberGenerator.GetInt32(255, 900);
                                        Instruction NewDynamic = new Instruction(chunk, Opcode.Dynamic)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            CustomData = new CustomInstructionData()
                                            {
                                                Opcode = newVirtual,
                                            }
                                        };

                                        Instruction NewDynamic2 = new Instruction(chunk, Opcode.Dynamic)
                                        {
                                            A = instruction.A,
                                            B = instruction.B,
                                            C = instruction.C,
                                            CustomData = new CustomInstructionData()
                                            {
                                                Opcode = newVirutalSometimes,
                                            }
                                        };

                                        if (UseEFirst)
                                        {
                                            NewDynamic.E = E1;
                                            NewDynamic.F = F1;

                                            NewDynamic2.F = F1;
                                            NewDynamic2.E = E1;

                                            instruction.E = E1;
                                            instruction.F = F1;
                                        }
                                        else
                                        {
                                            NewDynamic2.F = E1;
                                            NewDynamic2.E = F1;

                                            NewDynamic.E = F1;
                                            NewDynamic.F = E1;

                                            instruction.E = F1;
                                            instruction.F = E1;
                                        }

                                        instruction.CustomData!.Opcode = clonedVirtual;

                                        chunk.Instructions.RemoveAt(IndexOfInstruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, instruction);
                                        chunk.Instructions.Insert(IndexOfInstruction, NewDynamic2);
                                        chunk.Instructions.Insert(IndexOfInstruction, NewDynamic);

                                        chunk.UpdateMappings();

                                        foreach (var x in chunk.Instructions) x.UpdateReferences();

                                        virtuals.Add(newVirtual);
                                        virtuals.Add(newVirutalSometimes);
                                        virtuals.Add(clonedVirtual);
                                    }
                                    MutationsGenerated++;
                                    Console.WriteLine("Generated Dynamic Opcode Structure2");
                                }
                                break;
                            }
                    }
                }
            return virtuals;
        }
    }
}