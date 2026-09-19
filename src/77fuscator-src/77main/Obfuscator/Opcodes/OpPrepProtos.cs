using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepProtos : VOpcode
    {
        public override string OverrideString { get; set; } = "Proto = Chunk[PROTO_CHUNK];";

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepProtos;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}