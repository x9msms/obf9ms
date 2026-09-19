using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpReplayJmp : VOpcode
{
    public override string OverrideString { get; set; } = @"
if Inst[OP_A] == 0 then
    InstrPoint = InstrPoint + Inst[OP_B];
    local instrPointer = Inst[InstrPoint + Inst[OP_C]];
    instrPointer[OP_A] = 1;
    Inst[OP_A] = 1;
 --print('PREPARE REPLAY JUMP');
end
";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.ReplayJmp;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }

    public override void Mutate(Instruction instruction)
    {
        // instruction.B += instruction.PC + 1;
    }
}