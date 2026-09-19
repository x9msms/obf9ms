using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushConsts : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = Const;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushConsts;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}