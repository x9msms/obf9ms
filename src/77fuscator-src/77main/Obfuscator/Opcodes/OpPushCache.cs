using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushCache : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = Cache;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushCache;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}

public class OpPushConstCache : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = ConstantsCache;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushConstCache;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}