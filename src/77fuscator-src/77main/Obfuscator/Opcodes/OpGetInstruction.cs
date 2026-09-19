using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpGetInstruction : VOpcode
{
    public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Instr[InstrPoint + Inst[OP_B]];";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.GetInstruction;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }

    /*public override void Mutate(Instruction instruction)
    {
        instruction.PC += instruction.B;
    }*/
}