using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpClosure : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local NewProto=Proto[Inst[OP_B]];
local uvlist = {};
local C = Inst[OP_C];
for i = 1, C, 1 do
	InstrPoint = InstrPoint + 1;
	local pseudo = Instr[InstrPoint];
	if pseudo[OP_ENUM] == OP_MOVE then
		uvlist[i - 1] = {
			Stk,
			pseudo[OP_B]
		};
	else
        uvlist[i - 1] = Upvalues[pseudo[OP_B]]
	end;
	Lupvals[(#Lupvals) + 1] = uvlist;
end;
Stk[Inst[OP_A]] = Wrap(NewProto, uvlist);
";

        public override bool IsInstruction(Instruction instruction)
        {
            if (instruction.Junk)
                return false;

            return instruction.OpCode == Opcode.Closure && instruction.Chunk.Functions[instruction.B].UpvalueCount > 0;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            context.InstructionMapping.TryGetValue(Opcode.Move, out var i1);
            return
           OverrideString.Replace("OP_MOVE", i1?.VIndex.ToString() ?? "-1");
        }

        public override void Mutate(Instruction instruction)
        {
            instruction.InstructionType = InstructionType.AsBxC;
            instruction.C = instruction.Chunk.Functions[instruction.B].UpvalueCount;
        }
    }

    public class OpClosureNU : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Wrap(Proto[Inst[OP_B]],nil);";

        public override bool IsInstruction(Instruction instruction)
        {
            if (instruction.Junk)
                return false;

            return instruction.OpCode == Opcode.Closure && instruction.Chunk.Functions[instruction.B].UpvalueCount == 0;
        }

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}