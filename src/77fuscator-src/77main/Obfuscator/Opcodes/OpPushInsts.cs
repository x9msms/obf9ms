using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPushInsts : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = Instr;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.PushInsts;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}