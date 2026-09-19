using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes.Custom;

internal class OpSetTableNC : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]][Stk[Inst[OP_B]]]=Inst[OP_C];";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.SetTableNC;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}