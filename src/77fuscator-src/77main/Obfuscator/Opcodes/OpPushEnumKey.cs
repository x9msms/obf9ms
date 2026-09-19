using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushEnumKey : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = OP_ENUM;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushEnumKey;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}