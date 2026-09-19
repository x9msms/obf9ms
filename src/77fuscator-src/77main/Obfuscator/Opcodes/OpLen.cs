using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpLen : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=#Stk[Inst[OP_B]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Len;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}