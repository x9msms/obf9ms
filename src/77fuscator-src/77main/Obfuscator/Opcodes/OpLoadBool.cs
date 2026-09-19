using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpLoadBool : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=(Inst[OP_B]~=0);";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadBool && instruction.C == 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpLoadBoolC : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]=(Inst[OP_B]~=0);InstrPoint=InstrPoint+1;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.LoadBool && instruction.C != 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction ins) =>
            ins.C = 0;
    }
}