using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpClose : VOpcode
    {
        public override string OverrideString { get; set; } = "local A=Inst[OP_A];local Cls={};for Idx=1,#Lupvals do local List=Lupvals[Idx];for Idz=0,#List do local Upv=List[Idz];local NStk=Upv[1];local Pos=Upv[2]; if NStk==Stk and Pos>=A then Cls[Pos]=NStk[Pos];Upv[1]=Cls;end;end;end;";

        public override bool IsInstruction(Instruction instruction) =>
            instruction.OpCode == Opcode.Close;

        public override string GetObfuscated(ObfuscationContext context) =>
            OverrideString;
    }
}