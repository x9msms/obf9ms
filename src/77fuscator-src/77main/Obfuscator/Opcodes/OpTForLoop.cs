using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpTForLoop : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
local C = Inst[OP_C];
local Offset = A + 2;
local Result = {
	Stk[A](Stk[A + 1], Stk[Offset])
};
for Idx = 1, C do
	Stk[Offset + Idx] = Result[Idx];
end;
local R = Stk[A + 3];
if R then
	Stk[Offset] = R;
else
	InstrPoint = InstrPoint + 1;
end;
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TForLoop;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.InstructionType = InstructionType.AsBxC;
        }
    }
}