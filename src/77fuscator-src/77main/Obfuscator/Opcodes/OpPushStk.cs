using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPushStk : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = Stk";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.PushStack;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}