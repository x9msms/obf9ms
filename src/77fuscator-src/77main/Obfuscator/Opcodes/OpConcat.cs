using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpConcatMulti : VOpcode
    {
        public override string OverrideString { get; set; } = "local B=Inst[OP_B];local K=Stk[B] for Idx=B+1,Inst[OP_C] do K=K..Stk[Idx];end;Stk[Inst[OP_A]]=K;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Concat && instruction.B != instruction.C - 1;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpConcatSingle : VOpcode
    {
        public override string OverrideString { get; set; } = "local B=Inst[OP_B];Stk[Inst[OP_A]] = Stk[B] .. Stk[B+1];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Concat && instruction.B == instruction.C - 1;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}