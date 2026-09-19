using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushBKey : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = OP_B;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushBKey;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}