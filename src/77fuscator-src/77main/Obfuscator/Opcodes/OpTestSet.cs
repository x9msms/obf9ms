using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpTestSet : VOpcode
    {
        public override string OverrideString { get; set; } = "local B=Stk[Inst[OP_C]];if B then InstrPoint=InstrPoint+1;else Stk[Inst[OP_A]]=B;InstrPoint=Inst[OP_B];end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TestSet && instruction.C == 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C = instruction.B;
            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;
        }
    }

    public class OpTestSetC : VOpcode
    {
        public override string OverrideString { get; set; } = "local B=Stk[Inst[OP_C]];if not B then InstrPoint=InstrPoint+1;else Stk[Inst[OP_A]]=B;InstrPoint=Inst[OP_B];end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TestSet && instruction.C != 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C = instruction.B;
            instruction.B = instruction.PC + instruction.Chunk.Instructions[instruction.PC + 1].B + 2;
            instruction.InstructionType = InstructionType.AsBxC;
        }
    }
}