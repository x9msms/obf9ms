using geniussolution.Bytecode_Library.IR;
using geniussolution.Obfuscator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace geniussolution.Bytecode_Library.Bytecode
{
    public class Serializer
    {
        private ObfuscationContext _context;
        private ObfuscationSettings _settings;
        private Random _r = new Random();
        private Encoding _fuckingLua = Encoding.GetEncoding(28591);
        private List<VOpcode> _virtuals;

        public Serializer(ObfuscationContext context, ObfuscationSettings settings, List<VOpcode> virtuals)
        {
            _context = context;
            _settings = settings;
            _virtuals = virtuals;
        }

        public byte[] SerializeLChunk(Chunk chunk, bool factorXor = true)
        {
            List<byte> bytes = new List<byte>();

            void WriteByte(byte b)
            {
                if (factorXor)
                    b ^= (byte)(_context.PrimaryXorKey);

                bytes.Add(b);
            }

            void Write(byte[] b, bool checkEndian = true)
            {
                if (!BitConverter.IsLittleEndian && checkEndian)
                    b = b.Reverse().ToArray();

                bytes.AddRange(b.Select(i =>
                                        {
                                            if (factorXor)
                                                i ^= (byte)(_context.PrimaryXorKey);

                                            return i;
                                        }));
            }

            void WriteInt32(int i) =>
                Write(BitConverter.GetBytes(i));

            void WriteNumber(double d) =>
                Write(BitConverter.GetBytes(d));

            void WriteU24(uint u24)
            {
                for (int i = 0; i < 3; i++)
                {
                    WriteByte((byte)(u24 & 0xFF));
                    u24 >>= 8;
                }
            }

            void WriteULEB128(uint value)
            {
                do
                {
                    var byteValue = (byte)(value & 0x7F);
                    value >>= 7;
                    if (value != 0)
                    {
                        byteValue |= 0x80;
                    }

                    WriteByte(byteValue);
                } while (value != 0);
            }

            void WriteString(string s)
            {
                byte[] sBytes = _fuckingLua.GetBytes(s);

                WriteInt32(sBytes.Length);
                Write(sBytes, false);
            }

            void WriteInt16(short i) =>
                 Write(BitConverter.GetBytes(i));

            void WriteBool(bool b) =>
                Write(BitConverter.GetBytes(b));

            void SerializeInstruction(List<Instruction> instructions, Instruction inst)
            {
                if (inst.InstructionType == InstructionType.Data)
                {
                    WriteByte(1);
                    return;
                }
                inst.UpdateReferences();

                var cData = inst.CustomData;
                var opCode = (int)inst.OpCode;

                if (cData != null)
                {
                    var virtualOpcode = cData.Opcode;

                    //opCode = cData.WrittenOpcode?.VIndex ?? virtualOpcode.VIndex;
                    opCode = virtualOpcode.VIndex;

                    if (!inst.Mutated && !cData.Preprocessed)
                    {
                        inst.CustomData.Mutated = true;
                        inst.Mutated = true;
                        virtualOpcode.Mutate(inst);
                    }

                    if (cData.NopAfterMutate)
                    {
                        opCode = 1;
                    }
                }

                if (inst.HasObfuscation)
                    switch (inst.ObfuscationType)
                    {
                        case ObfuscationType.DynamicHandler:
                            {
                                var currentPC = instructions.IndexOf(inst);

                                var targetInstruction = instructions[currentPC + 5];
                                var jumpInstruction = instructions[currentPC + 4];
                                var updateInstruction = instructions[currentPC + 2];
                                var getEnumInst = instructions[currentPC + 1];
                                targetInstruction.Untouched = true;

                                inst.B = 5;

                                var updateCData = targetInstruction.CustomData;
                                var updateOpcode = (int)targetInstruction.OpCode;

                                if (updateCData != null)
                                {
                                    var virtualOpcode = updateCData.Opcode;
                                    virtualOpcode.Mutate(targetInstruction);
                                    updateOpcode = virtualOpcode.VIndex;
                                    updateCData.Preprocessed = true;
                                    targetInstruction.Mutated = true;
                                    targetInstruction.CustomData.Mutated = true;
                                }
                                jumpInstruction.B = getEnumInst.CustomData!.Opcode.VIndex;

                                updateInstruction.C = updateOpcode;
                                targetInstruction.CustomData!.NopAfterMutate = true;

                                break;
                            }

                        case ObfuscationType.Polymorphic:
                            {
                                if (!inst.HasPolymorphicData)
                                    break; // somethin broke prob

                                inst.Untouched = true;

                                var data = inst.PolymorphicData;

                                opCode = data.Virtual.VIndex;

                                if (data.IsASubtract)
                                {
                                    inst.A += data.OffsetA;
                                }
                                else if (data.DontOffsetA)
                                {
                                }
                                else
                                {
                                    inst.A -= data.OffsetA;
                                }

                                if (data.IsBSubtract)
                                {
                                    inst.B += data.OffsetB;
                                }
                                else if (data.DontOffsetB)
                                {
                                }
                                else
                                {
                                    inst.B -= data.OffsetB;
                                }

                                if (data.IsCSubtract)
                                {
                                    inst.C += data.OffsetC;
                                }
                                else if (data.DontOffsetC)
                                {
                                }
                                else
                                {
                                    inst.C -= data.OffsetC;
                                }

                                if (data.IsESubtract)
                                {
                                    inst.E += data.OffsetE;
                                }
                                else if (data.DontOffsetE)
                                {
                                }
                                else
                                {
                                    inst.E -= data.OffsetE;
                                }

                                if (data.IsFSubtract)
                                {
                                    inst.F += data.OffsetF;
                                }
                                else if (data.DontOffsetF)
                                {
                                }
                                else
                                {
                                    inst.F -= data.OffsetF;
                                }

                                inst.OpCode = Opcode.Polymorphic;

                                break;
                            }
                        case ObfuscationType.Redirect:
                            {
                                var currentPC = instructions.IndexOf(inst);

                                var targetInstruction = instructions[currentPC + 4];
                                var updateInstruction = instructions[currentPC + 2];
                                targetInstruction.Untouched = true;

                                inst.B = 4;

                                var updateCData = targetInstruction.CustomData;
                                var updateOpcode = (int)targetInstruction.OpCode;

                                if (updateCData != null)
                                {
                                    var virtualOpcode = updateCData.Opcode;
                                    virtualOpcode.Mutate(targetInstruction);
                                    updateOpcode = virtualOpcode.VIndex;
                                    updateCData.Preprocessed = true;
                                    targetInstruction.Mutated = true;
                                    targetInstruction.CustomData.Mutated = true;
                                }

                                updateInstruction.C = updateOpcode;
                                targetInstruction.CustomData!.NopAfterMutate = true;

                                break;
                            }
                        case ObfuscationType.ResetOpcodeData:
                            {
                                var currentPC = instructions.IndexOf(inst);
                                var InstructionTest = instructions[currentPC + 1];

                                if (InstructionTest.OpCode == Opcode.PushAKey)
                                {
                                    var targetInstruction = instructions[currentPC + 4];
                                    var setTable = instructions[currentPC + 2];
                                    targetInstruction.Untouched = true;
                                    inst.B = 4;

                                    var updateCData = targetInstruction.CustomData;
                                    var updateOpcode = (int)targetInstruction.OpCode;

                                    if (updateCData != null)
                                    {
                                        var virtualOpcode = updateCData.Opcode;
                                        virtualOpcode.Mutate(targetInstruction);
                                        updateCData.Preprocessed = true;
                                        targetInstruction.Mutated = true;
                                        targetInstruction.CustomData.Mutated = true;
                                    }

                                    setTable.C = targetInstruction.A;
                                    targetInstruction.A = RandomNumberGenerator.GetInt32(1, 255);
                                }
                                else if (InstructionTest.OpCode == Opcode.PushBKey)
                                {
                                    var targetInstruction = instructions[currentPC + 4];
                                    var setTable = instructions[currentPC + 2];
                                    targetInstruction.Untouched = true;
                                    inst.B = 4;

                                    var updateCData = targetInstruction.CustomData;
                                    var updateOpcode = (int)targetInstruction.OpCode;

                                    if (updateCData != null)
                                    {
                                        var virtualOpcode = updateCData.Opcode;
                                        virtualOpcode.Mutate(targetInstruction);
                                        updateCData.Preprocessed = true;
                                        targetInstruction.Mutated = true;
                                        targetInstruction.CustomData.Mutated = true;
                                    }

                                    setTable.C = targetInstruction.B;
                                    targetInstruction.B = RandomNumberGenerator.GetInt32(1, 255);
                                }
                                else if (InstructionTest.OpCode == Opcode.PushCKey && instructions[currentPC + 3].OpCode == Opcode.PushEnumKey)
                                {
                                    var targetInstruction = instructions[currentPC + 6];
                                    var GetCInstruction = instructions[currentPC + 2];
                                    var GetEnumInstruction = instructions[currentPC + 4];

                                    targetInstruction.Untouched = true;
                                    inst.B = 6;

                                    var updateCData = targetInstruction.CustomData;
                                    var updateOpcode = (int)targetInstruction.OpCode;

                                    if (updateCData != null)
                                    {
                                        var virtualOpcode = updateCData.Opcode;
                                        virtualOpcode.Mutate(targetInstruction);
                                        updateOpcode = virtualOpcode.VIndex;
                                        updateCData.Preprocessed = true;
                                        targetInstruction.Mutated = true;
                                        targetInstruction.CustomData.Mutated = true;
                                    }
                                    GetCInstruction.C = targetInstruction.C;

                                    targetInstruction.C = RandomNumberGenerator.GetInt32(1, 255);

                                    GetEnumInstruction.C = updateOpcode;

                                    targetInstruction.CustomData!.NopAfterMutate = true;
                                }
                                break;
                            }

                        case ObfuscationType.RegisterReload:
                            {
                                var currentPC = instructions.IndexOf(inst);
                                var targetInstruction = instructions[currentPC + 6];

                                var updateCData = targetInstruction.CustomData;
                                var updateOpcode = (int)targetInstruction.OpCode;

                                if (updateCData != null)
                                {
                                    var virtualOpcode = updateCData.Opcode;
                                    virtualOpcode.Mutate(targetInstruction);
                                    updateCData.Preprocessed = true;
                                    targetInstruction.Mutated = true;
                                    targetInstruction.CustomData.Mutated = true;
                                }
                                var setTableA = instructions[currentPC + 3];
                                setTableA.C = targetInstruction.A;
                                var setTableB = instructions[currentPC + 4];
                                setTableB.C = targetInstruction.B;

                                targetInstruction.Untouched = true;
                                targetInstruction.A = 0;
                                targetInstruction.B = 0;

                                inst.B = 6;

                                break;
                            }

                        case ObfuscationType.ShuffleAB:
                            {
                                (inst.B, inst.A) = (inst.A, inst.B);
                                break;
                            }
                        case ObfuscationType.ShuffleAC:
                            {
                                (inst.C, inst.A) = (inst.A, inst.C);
                                break;
                            }
                        case ObfuscationType.ShuffleABC:
                            {
                                (inst.C, inst.A, inst.B) = (inst.A, inst.B, inst.C);
                                break;
                            }
                        case ObfuscationType.ShuffleBAC:
                            {
                                (inst.B, inst.A, inst.C) = (inst.A, inst.C, inst.B);
                                break;
                            }
                        case ObfuscationType.ShuffleBC:
                            {
                                (inst.C, inst.B) = (inst.B, inst.C);
                                break;
                            }
                    }

                int t = (int)inst.InstructionType;
                WriteByte((byte)(t << 1));
                WriteULEB128((uint)opCode);
                WriteInt16((short)inst.A);

                int b = inst.B;
                int c = inst.C;

                switch (inst.InstructionType)
                {
                    case InstructionType.AsBx:
                        b += 1 << 16;
                        WriteInt32(b);
                        break;

                    case InstructionType.AsBxC:
                        b += 1 << 16;
                        WriteInt32(b);
                        WriteU24((uint)c);
                        WriteInt16((short)inst.E);
                        WriteInt32((short)inst.F);
                        break;

                    case InstructionType.ABC:
                        WriteU24((uint)b);
                        WriteU24((uint)c);
                        WriteInt16((short)inst.E);
                        WriteInt32((short)inst.F);
                        break;

                    case InstructionType.ABx:
                        WriteInt32(b);
                        break;
                }
            }

            chunk.UpdateMappings();

            for (int i = 0; i < (int)ChunkStep.StepCount; i++)
            {
                switch (_context.ChunkSteps[i])
                {
                    case ChunkStep.ParameterCount:
                        WriteInt16((short)(chunk.ParameterCount ^ _context.PrimaryXorKey));
                        break;

                    case ChunkStep.Constants:
                        WriteInt32(chunk.Constants.Count);
                        foreach (Constant c in chunk.Constants)
                        {
                            WriteByte((byte)_context.ConstantMapping[(int)c.Type]);
                            switch (c.Type)
                            {
                                case ConstantType.Boolean:
                                    WriteBool(c.Data);
                                    break;

                                case ConstantType.Number:
                                    WriteNumber(c.Data);
                                    break;

                                case ConstantType.String:
                                    {
                                        if (c.TamperConstant)
                                        {
                                            WriteByte(1);
                                            break;
                                        }
                                        WriteByte(0);
                                        WriteString(c.Data);
                                        break;
                                    }
                            }
                        }
                        break;

                    case ChunkStep.Instructions:
                        WriteULEB128((uint)chunk.Instructions.Count);
                        //Console.WriteLine($"Instruction Count: {chunk.Instructions.Count}");
                        //WriteU24((uint)chunk.Instructions.Count);
                        foreach (Instruction ins in chunk.Instructions)
                        {
                            SerializeInstruction(chunk.Instructions, ins);
                        }
                        break;

                    case ChunkStep.Functions:
                        WriteULEB128((uint)chunk.Functions.Count);
                        foreach (Chunk c in chunk.Functions)
                            Write(SerializeLChunk(c, false));

                        break;
                }
            }

            return bytes.ToArray();
        }
    }
}