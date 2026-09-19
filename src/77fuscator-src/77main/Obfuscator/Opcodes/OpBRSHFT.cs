using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpBRSHFT : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitRSHIFT(Stk[Inst[OP_B]],Stk[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BRSHFT && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpBRSHFTB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitRSHIFT(Const[Inst[OP_B]],Stk[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BRSHFT && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
        }
    }

    public class OpBRSHFTC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitRSHIFT(Stk[Inst[OP_B]],Const[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BRSHFT && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;
        }
    }

    public class OpBRSHFTD : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitRSHIFT(Const[Inst[OP_B]],Const[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BRSHFT && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
            instruction.C -= 255;
        }
    }
}