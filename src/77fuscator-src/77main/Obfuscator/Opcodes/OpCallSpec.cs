using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpCallSpec : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_C]] = Stk[Inst[OP_A]](Stk[Inst[OP_B]]);";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.CallSpec;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}