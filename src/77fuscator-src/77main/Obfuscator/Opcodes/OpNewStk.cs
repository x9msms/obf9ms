using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpNewStk : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk = {};for Idx = 0, PCount do if Idx < Params then Stk[Idx] = Args[Idx + 1]; else break end; end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.NewStack;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}