using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    //custom VM opcode for inlining
    public class OpSetTop : VOpcode
    {
        public override string OverrideString { get; set; } = "Top=Inst[OP_A];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.SetTop;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}