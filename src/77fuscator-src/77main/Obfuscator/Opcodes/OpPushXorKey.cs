using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPushXorKey : VOpcode
    {
        //public override string OverrideString { get; set; } = "Stk[Inst[OP_A]] = INSTRUCTION_XOR_KEY ";
        public override string OverrideString { get; set; } = "";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.PushXorKey;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}