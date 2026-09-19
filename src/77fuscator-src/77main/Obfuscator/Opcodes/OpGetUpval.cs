using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpGetUpval : VOpcode
    {
        public override string OverrideString { get; set; } = "local UV = Upvalues[Inst[OP_B]];Stk[Inst[OP_A]]=UV[1][UV[2]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.GetUpval;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}