using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpBLSHFT : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitLSHIFT(Stk[Inst[OP_B]],Stk[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BLSHFT && instruction.B <= 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpBLSHFTB : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitLSHIFT(Const[Inst[OP_B]],Stk[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BLSHFT && instruction.B > 255 && instruction.C <= 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
        }
    }

    public class OpBLSHFTC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitLSHIFT(Stk[Inst[OP_B]],Const[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BLSHFT && instruction.B <= 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.C -= 255;
        }
    }

    public class OpBLSHFTD : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=BitLSHIFT(Const[Inst[OP_B]],Const[Inst[OP_C]]);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.BLSHFT && instruction.B > 255 && instruction.C > 255;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 255;
            instruction.C -= 255;
        }
    }
}