using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpForLoop : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
local step = Stk[A + 2];
local index = Stk[A] + step;
local limit = Stk[A + 1];
local loops;
if step == Abs(step) then
	loops = index <= limit;
else
	loops = index >= limit;
end;
if loops then
	Stk[A] = index;
	Stk[A + 3] = index;
	InstrPoint = Inst[OP_B];
end;
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ForLoop;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B += instruction.PC + 1;
        }
    }
}