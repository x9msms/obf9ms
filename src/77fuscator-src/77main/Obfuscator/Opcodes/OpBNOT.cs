using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpBNOT : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitNOT(Stk[Inst[OP_B]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BNOT;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}