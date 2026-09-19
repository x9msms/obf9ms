using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpSetUpval : VOpcode
    {
        public override string OverrideString { get; set; } = "local UV = Upvalues[Inst[OP_B]];UV[1][UV[2]]=Stk[Inst[OP_A]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.SetUpval;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}