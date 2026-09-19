using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpCopyTable : VOpcode
    {
        public override string OverrideString { get; set; } = "local t = Stk[Inst[OP_B]] for i,v in Next,Stk[Inst[OP_A]] do t[#t+1] = v end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.CopyTable;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}