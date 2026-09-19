using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpINTDIV : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=__INTDIV__(Stk[Inst[OP_B]],Stk[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.INTDIV && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpINTDIVB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=__INTDIV__(Const[Inst[OP_B]],Stk[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.INTDIV && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
        }
    }

    public class OpINTDIVC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=__INTDIV__(Stk[Inst[OP_B]],Const[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.INTDIV && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;
        }
    }

    public class OpINTDIVD : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=__INTDIV__(Const[Inst[OP_B]],Const[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.INTDIV && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
            instruction.C -= 255;
        }
    }
}