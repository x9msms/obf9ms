using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.ControlFlowAns;
using geniussolution.Obfuscator;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace geniussolution.Bytecode_Library.IR
{
    public enum ObfuscationType
    {
        StringEncryption,
        Polymorphic,
        Redirect,
        RegisterReload,
        ShuffledInstructions,
        ObfuscatedGlobal,
        ResetOpcodeData,
        DynamicHandler,

        ShuffleAB,
        ShuffleABC,
        ShuffleAC,
        ShuffleBC,
        ShuffleBAC
    }

    public class Instruction
    {
        private static readonly Dictionary<Opcode, InstructionType> InstructionMappings = new()
    {
        { Opcode.Move, InstructionType.ABC },
        { Opcode.LoadConst, InstructionType.ABx },
        { Opcode.LoadBool, InstructionType.ABC },
        { Opcode.LoadNil, InstructionType.ABC },
        { Opcode.GetUpval, InstructionType.ABC },
        { Opcode.GetGlobal, InstructionType.ABx },
        { Opcode.GetTable, InstructionType.ABC },
        { Opcode.SetGlobal, InstructionType.ABx },
        { Opcode.SetUpval, InstructionType.ABC },
        { Opcode.SetTable, InstructionType.ABC },
        { Opcode.NewTable, InstructionType.ABC },
        { Opcode.Self, InstructionType.ABC },
        { Opcode.Add, InstructionType.ABC },
        { Opcode.Sub, InstructionType.ABC },
        { Opcode.Mul, InstructionType.ABC },
        { Opcode.Div, InstructionType.ABC },
        { Opcode.Mod, InstructionType.ABC },
        { Opcode.Pow, InstructionType.ABC },

        { Opcode.BOR, InstructionType.ABC },
        { Opcode.BAND, InstructionType.ABC },
        { Opcode.BXOR, InstructionType.ABC },
        { Opcode.BLSHFT, InstructionType.ABC },
        { Opcode.BRSHFT, InstructionType.ABC },
        { Opcode.BNOT, InstructionType.ABC },
        { Opcode.INTDIV, InstructionType.ABC },

        { Opcode.Unm, InstructionType.ABC },
        { Opcode.Not, InstructionType.ABC },
        { Opcode.Len, InstructionType.ABC },
        { Opcode.Concat, InstructionType.ABC },
        { Opcode.Jmp, InstructionType.AsBx },
        { Opcode.Eq, InstructionType.ABC },
        { Opcode.Lt, InstructionType.ABC },
        { Opcode.Le, InstructionType.ABC },
        { Opcode.Test, InstructionType.ABC },
        { Opcode.TestSet, InstructionType.ABC },
        { Opcode.Call, InstructionType.ABC },
        { Opcode.TailCall, InstructionType.ABC },
        { Opcode.Return, InstructionType.ABC },
        { Opcode.ForLoop, InstructionType.AsBx },
        { Opcode.ForPrep, InstructionType.AsBx },
        { Opcode.TForLoop, InstructionType.ABC },
        { Opcode.SetList, InstructionType.ABC },
        { Opcode.Close, InstructionType.ABC },
        { Opcode.Closure, InstructionType.ABx },
        { Opcode.VarArg, InstructionType.ABC }
    };

        public int A;
        public int B;
        public List<Instruction> BackReferences = new();
        public int C;

        public int E; // Custom
        public int F; // Custom

        public CustomInstructionData CustomData;
        public int? WrittenVIndex = null;

        public int Data;

        public bool HasLinkedInstruction;
        public bool HasObfuscation;
        public bool HasPolymorphicData;
        public bool HasDynamicData;
        public InstructionType InstructionType = InstructionType.ABC;

        public bool IsObfuscated;
        public int Line;

        //public InstructionConstantMask ConstantMask;
        public bool Reloaded = false;

        public Instruction LinkedInstruction;
        public Instruction ObfuscatedBy;
        public ObfuscationType ObfuscationType;
        public bool Mutated = false;
        public Constant? KAReference;
        public Constant? KBReference;
        public Constant? KCReference;
        public bool NeedsSpecialBranch = false;

        public Constant? RedirectKAReference;
        public Constant? RedirectKBReference;
        public Constant? RedirectKCReference;
        public bool Junk;

        public Constant? ConstantReference;

        public Opcode OpCode;
        public int PC;
        public PolymorphicData PolymorphicData;
        public DynamicData DynamicData;
        public Chunk? PrototypeReference;

        public Instruction? InstructionReference;

        public Instruction? NextInstruction;
        public Block? EncompassingBlock;
        public Block? JumpBlock;

        public Chunk Chunk;
        public object[] RefOperands = { null, null, null };

        public bool Untouched = false;

        public Instruction(Instruction other)
        {
            Chunk = other.Chunk;
            OpCode = other.OpCode;
            InstructionType = InstructionMappings.GetValueOrDefault(other.OpCode, InstructionType.ABC);
            A = other.A;
            B = other.B;
            C = other.C;
            Data = other.Data;
            PC = other.PC;
            Line = other.Line;
        }

        public Instruction(Chunk chunk, Opcode code, params object[] refOperands)
        {
            A = RandomNumberGenerator.GetInt32(-200, -150);
            B = RandomNumberGenerator.GetInt32(-200, -150);
            C = RandomNumberGenerator.GetInt32(-200, -150);
            Data = 0;

            Chunk = chunk;
            OpCode = code;

            InstructionType = InstructionMappings.GetValueOrDefault(code, InstructionType.ABC);

            /* for (int i = 0; i < refOperands.Length; i++)
             {
                 var op = refOperands[i];
                 RefOperands[i] = op;

                 if (op is Instruction ins)
                     ins.BackReferences.Add(this);
             }*/
        }

        public void UpdateReferences()
        {
            if (InstructionType == InstructionType.Data)
                return;

            PC = Chunk.InstructionMap[this];

            switch (OpCode)
            {
                case Opcode.LoadConst:
                case Opcode.GetGlobal:
                case Opcode.SetGlobal:
                    if (ConstantReference is null)
                        break;

                    B = Chunk.ConstantMap[ConstantReference];
                    break;

                case Opcode.Jmp:
                case Opcode.ForLoop:
                case Opcode.ForPrep:
                    if (InstructionReference is null)
                        break;

                    try
                    {
                        if (InstructionReference.ObfuscatedBy is not null)
                            B = Chunk.InstructionMap[InstructionReference.ObfuscatedBy] - PC - 1;
                        else
                            B = Chunk.InstructionMap[InstructionReference] - PC - 1;
                    }
                    catch (Exception e) { }

                    break;

                case Opcode.Closure:
                    if (PrototypeReference is null)
                        break;
                    try
                    {
                        B = Chunk.FunctionMap[PrototypeReference];
                    }
                    catch (Exception e)
                    {
                    }
                    break;

                case Opcode.GetTable:
                case Opcode.SetTable:
                case Opcode.Add:
                case Opcode.Sub:
                case Opcode.Mul:
                case Opcode.Div:
                case Opcode.Mod:
                case Opcode.Pow:
                case Opcode.Self:
                case Opcode.Eq:
                case Opcode.Lt:
                case Opcode.Le:
                    {
                        if (KBReference is Constant cB)
                            B = Chunk.ConstantMap[cB] + 256;

                        if (KCReference is Constant cC)
                            C = Chunk.ConstantMap[cC] + 256;
                        break;
                    }
            }
        }

        public void SetupReferences()
        {
            switch (OpCode)
            {
                case Opcode.LoadConst:
                case Opcode.GetGlobal:
                case Opcode.SetGlobal:
                    ConstantReference = Chunk.Constants[B];
                    break;

                case Opcode.Jmp:
                case Opcode.ForLoop:
                case Opcode.ForPrep:
                    try
                    {
                        InstructionReference = Chunk.Instructions[Chunk.InstructionMap[this] + B + 1];
                    }
                    catch (Exception e) { }
                    break;

                case Opcode.Closure:
                    PrototypeReference = Chunk.Functions[B];
                    break;

                case Opcode.GetTable:
                case Opcode.SetTable:
                case Opcode.Add:
                case Opcode.Sub:
                case Opcode.Mul:
                case Opcode.Div:
                case Opcode.Mod:
                case Opcode.Pow:
                case Opcode.Self:
                case Opcode.Eq:
                case Opcode.Lt:
                case Opcode.Le:
                    try
                    {
                        if (!HasLinkedInstruction && B > 255)
                            KBReference = Chunk.Constants[B - 256];
                    }
                    catch (Exception e) { }
                    try
                    {
                        if (!HasLinkedInstruction && C > 255)
                            KCReference = Chunk.Constants[C - 256];
                    }
                    catch (Exception e) { }
                    break;
            }
        }
    }
}