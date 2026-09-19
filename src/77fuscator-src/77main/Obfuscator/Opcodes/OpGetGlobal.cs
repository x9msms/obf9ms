using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpGetGlobal : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Env[Const[Inst[OP_B]]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.GetGlobal;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }
}