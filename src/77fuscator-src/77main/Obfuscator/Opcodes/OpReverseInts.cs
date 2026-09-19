using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpReverseConstB : VOpcode
    {
        public override string OverrideString { get; set; } = "Const[Inst[OP_B]] = -Const[Inst[OP_B]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ReverseConstBInt;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            if (instruction.B > 255 && instruction.C <= 255)
            {
                instruction.B -= 255;
            }
            else if (instruction.B <= 255 && instruction.C > 255)
            {
                instruction.C -= 255;
            }
            else if (instruction.B > 255 && instruction.C > 255)
            {
                instruction.B -= 255;
                instruction.C -= 255;
            }
        }
    }

    public class OpReverseConstC : VOpcode
    {
        public override string OverrideString { get; set; } = "Const[Inst[OP_C]] = -Const[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ReverseConstCInt;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            if (instruction.B > 255 && instruction.C <= 255)
            {
                instruction.B -= 255;
            }
            else if (instruction.B <= 255 && instruction.C > 255)
            {
                instruction.C -= 255;
            }
            else if (instruction.B > 255 && instruction.C > 255)
            {
                instruction.B -= 255;
                instruction.C -= 255;
            }
        }
    }

    public class OpReverseStkC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_C]] = -Stk[Inst[OP_C]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ReverseStkCInt;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            if (instruction.B > 255 && instruction.C <= 255)
            {
                instruction.B -= 255;
            }
            else if (instruction.B <= 255 && instruction.C > 255)
            {
                instruction.C -= 255;
            }
            else if (instruction.B > 255 && instruction.C > 255)
            {
                instruction.B -= 255;
                instruction.C -= 255;
            }
        }
    }

    public class OpReverseStkB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_B]] = -Stk[Inst[OP_B]];";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.ReverseStkBInt;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            if (instruction.B > 255 && instruction.C <= 255)
            {
                instruction.B -= 255;
            }
            else if (instruction.B <= 255 && instruction.C > 255)
            {
                instruction.C -= 255;
            }
            else if (instruction.B > 255 && instruction.C > 255)
            {
                instruction.B -= 255;
                instruction.C -= 255;
            }
        }
    }
}