using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpLoadK : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Const[Inst[OP_B]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadConst; // && instruction.Chunk.Constants[instruction.B].Type != ConstantType.String;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }

    public class OpLoadB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Inst[OP_B];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadN;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }
}