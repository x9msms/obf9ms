using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpSetGlobal : VOpcode
    {
        public override string OverrideString { get; set; } = "Env[Const[Inst[OP_B]]]=Stk[Inst[OP_A]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.SetGlobal;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }
}