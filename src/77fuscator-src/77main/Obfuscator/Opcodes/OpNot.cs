using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpNot : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=(not Stk[Inst[OP_B]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Not;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}