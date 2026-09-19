using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushCKey : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = OP_C;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushCKey;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}