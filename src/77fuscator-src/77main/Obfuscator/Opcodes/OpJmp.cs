using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpJmp : VOpcode
    {
        public override string OverrideString { get; set; } = "InstrPoint=Inst[OP_B];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Jmp;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B += instruction.PC + 1;
        }
    }
}