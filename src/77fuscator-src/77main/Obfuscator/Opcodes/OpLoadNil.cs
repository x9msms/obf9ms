using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpLoadNilMulti : VOpcode
    {
        public override string OverrideString { get; set; } = "for Idx=Inst[OP_A],Inst[OP_B] do Stk[Idx]=nil;end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadNil && instruction.A != instruction.B;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpLoadNilSingle : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=nil;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadNil && instruction.A == instruction.B;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}