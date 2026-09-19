using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpUnpackTable : VOpcode
    {
        public override string OverrideString { get; set; } = "return Unpack(Stk[Inst[OP_A]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.UnpackTable;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}