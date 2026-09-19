using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPushStkPersistent : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = PersistentStacks;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.PushStackPersistent;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpPushConstPersistent : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = ConstantsCache;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.PushConstCache;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}