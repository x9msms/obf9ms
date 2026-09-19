using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpNewTableB0 : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]={};";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.NewTable && instruction.B == 0;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpNewTableB5000 : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]={};";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.NewTable && instruction.B > 5000;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }

    public class OpNewTableBElse : VOpcode
    {
        public override string OverrideString { get; set; } = "Stk[Inst[OP_A]]={Unpack({}, 1, Inst[OP_B])};";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.NewTable && instruction.B > 0 && instruction.B <= 5000;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}