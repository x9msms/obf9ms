using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpReplay : VOpcode
{
    public override string OverrideString { get; set; } = " if Inst[OP_A] ~= 0 then InstrPoint = InstrPoint + Inst[OP_B]; Inst[OP_B] = 0; end;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.Replay;
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