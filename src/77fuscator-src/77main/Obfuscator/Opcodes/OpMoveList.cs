using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpMoveList : VOpcode
    {
        public override string OverrideString { get; set; } = @"
        local A = Inst[OP_A];
        local B = Inst[OP_B];
        local diff = Inst[OP_C];
        for i = A, B do
            Stk[i] = Stk[i - diff];
        end;
        --InstrPoint = InstrPoint + (B - A) - 1;
        ";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.MoveList; // && instruction.Chunk.Constants[instruction.B].Type != ConstantType.String;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }
}