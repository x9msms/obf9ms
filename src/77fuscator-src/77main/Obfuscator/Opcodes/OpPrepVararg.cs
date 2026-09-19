using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepVararg : VOpcode
    {
        public override string OverrideString { get; set; } = "Vararg = {}";

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepVararg;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}