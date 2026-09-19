using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpNOP : VOpcode
{
    public override string OverrideString { get; set; } = "";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.NOP;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }

    public override void Mutate(Instruction instruction)
    {
        //instruction.B += instruction.PC + 1;
    }
}