using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepVarargSize : VOpcode
    {
        public override string OverrideString { get; set; } = "Varargsz = PCount - Params + 1";

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepVarargSize;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}