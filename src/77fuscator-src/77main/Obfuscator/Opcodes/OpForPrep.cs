using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpForPrep : VOpcode
    {
        public override string OverrideString { get; set; } = "local A=Inst[OP_A];Stk[A]=(Stk[A] or 0)-(Stk[A+2] or 0);InstrPoint=Inst[OP_B];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ForPrep;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B += instruction.PC + 1;
        }
    }
}