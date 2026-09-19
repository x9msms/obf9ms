using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpLoadKList : VOpcode
    {
        public override string OverrideString { get; set; } = @"
        local off = Inst[OP_A];
        for i = off, Inst[OP_C] do
            Stk[i] = Const[i - off + 1];
        end;
        --InstrPoint = InstrPoint + inst.len - 1
        ";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadConstList; // && instruction.Chunk.Constants[instruction.B].Type != ConstantType.String;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B++;
    }
}