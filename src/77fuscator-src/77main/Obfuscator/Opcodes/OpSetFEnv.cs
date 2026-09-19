using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpSetFEnv : VOpcode
    {
        public override string OverrideString { get; set; } = "Env = Stk[Inst[OP_A]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.SetFenv;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpGetFEnv : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = Env;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.GetFEnv;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}