using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpBAND : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitBAND(Stk[Inst[OP_B]],Stk[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BAND && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpBANDB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitBAND(Const[Inst[OP_B]],Stk[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BAND && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
        }
    }

    public class OpBANDC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitBAND(Stk[Inst[OP_B]],Const[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BAND && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;
        }
    }

    public class OpBANDD : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitBAND(Const[Inst[OP_B]],Const[Inst[OP_C]]);\n";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BAND && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
            instruction.C -= 255;
        }
    }
}