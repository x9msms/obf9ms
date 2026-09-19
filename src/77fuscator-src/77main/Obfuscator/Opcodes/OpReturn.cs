using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpReturn : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
do return Unpack(Stk, A, A + Inst[OP_B]) end;
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Return && instruction.B > 3;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;

        public override void Mutate(Instruction instruction)
        {
            instruction.B -= 2;
        }
    }

    public class OpReturnB2 : VOpcode
    {
        public override string OverrideString { get; set; } = @"
do return Stk[Inst[OP_A]] end
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Return && instruction.B == 2;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpReturnB3 : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
do return Stk[A], Stk[A + 1] end
";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Return && instruction.B == 3;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpReturnB0 : VOpcode
    {
        public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
do return Unpack(Stk, A, Top) end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Return && instruction.B == 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpReturnB1 : VOpcode
    {
        public override string OverrideString { get; set; } = "do return end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Return && instruction.B == 1;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}