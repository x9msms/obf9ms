using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPow : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Stk[Inst[OP_B]]^Stk[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Pow && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpPowB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Const[Inst[OP_B]]^Stk[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Pow && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.B -= 255;
    }

    public class OpPowC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Stk[Inst[OP_B]]^Const[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Pow && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction) =>
            instruction.C -= 255;
    }

    public class OpPowBC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=Const[Inst[OP_B]]^Const[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Pow && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
            instruction.C -= 255;
        }
    }
}