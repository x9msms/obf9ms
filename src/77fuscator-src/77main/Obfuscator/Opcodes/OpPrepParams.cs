using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepParams : VOpcode
    {
        public override string OverrideString { get; set; } = "Params = Chunk[PARAMS_CHUNK];";

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepParams;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}