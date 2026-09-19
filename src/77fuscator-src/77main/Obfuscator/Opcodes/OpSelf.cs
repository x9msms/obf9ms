using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpSelf : VOpcode
    {
        public override string OverrideString { get; set; } = "local A=Inst[OP_A];local B=Stk[Inst[OP_B]];Stk[A+1]=B;Stk[A]=B[Stk[Inst[OP_C]]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Self && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpSelfC : VOpcode
    {
        public override string OverrideString { get; set; } = "local A=Inst[OP_A];local B=Stk[Inst[OP_B]];Stk[A+1]=B;Stk[A]=B[Const[Inst[OP_C]]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Self && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.C -= 255;
    }
}