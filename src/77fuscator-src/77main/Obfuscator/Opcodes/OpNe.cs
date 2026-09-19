using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpNe : VOpcode
    {
        public override string OverrideString { get; set; } = @" if (Stk[Inst[OP_B]] ~= Stk[Inst[OP_C]]) then
            InstrPoint = InstrPoint + 1;
        end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Eq && instruction.A != 0 && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        /*public override void Mutate(Instruction instruction)
        {
            instruction.A = instruction.B;

            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;
        }*/
    }

    public class OpNeB : VOpcode
    {
        public override string OverrideString { get; set; } = @"if (Const[Inst[OP_B]] ~= Stk[Inst[OP_C]]) then
            InstrPoint = InstrPoint + 1;
        end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Eq && instruction.A != 0 && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;

            /*instruction.A = instruction.B - 255;

            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;*/
        }
    }

    public class OpNeC : VOpcode
    {
        public override string OverrideString { get; set; } = @"if (Stk[Inst[OP_B]] ~= Const[Inst[OP_C]]) then
            InstrPoint = InstrPoint + 1;
        end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Eq && instruction.A != 0 && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;

            /*instruction.A = instruction.B;
			instruction.C -= 255;

            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;*/
        }
    }

    public class OpNeBC : VOpcode
    {
        public override string OverrideString { get; set; } = @"  if (Const[Inst[OP_B]] ~= Const[Inst[OP_C]]) then
            InstrPoint = InstrPoint + 1;
        end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Eq && instruction.A != 0 && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;
            instruction.B -= 255;

            /*instruction.A = instruction.B - 255;
			instruction.C -= 255;

            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;*/
        }
    }
}