using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepConsts : VOpcode
    {
        public override string OverrideString { get; set; } = "Const = Chunk[CONST_CHUNK];";

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepConsts;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}