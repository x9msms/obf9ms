using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpTailCall : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
do return Stk[A](Unpack(Stk, A + 1, Inst[OP_B])) end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TailCall && instruction.B > 1;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B += instruction.A - 1;
        }
    }

    public class OpTailCallB0 : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
do return Stk[A](Unpack(Stk, A + 1, Top)) end;
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TailCall && instruction.B == 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpTailCallB1 : VOpcode
    {
        public override string OverrideString { get; set; } = "do return Stk[Inst[OP_A]](); end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.TailCall && instruction.B == 1;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}