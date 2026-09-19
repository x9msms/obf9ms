using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpFuncDecl1 : VOpcode
{
    public override string OverrideString { get; set; } =
        "local CurrentInst = Inst Stk[Inst[OP_A]]=function() return CurrentInst[OP_A] end;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.FuncDecl1;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}

public class OpFuncDecl2 : VOpcode
{
    public override string OverrideString { get; set; } =
        "local CurrentInst = Inst Stk[Inst[OP_A]]=function() return CurrentInst[OP_A] end;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.FuncDecl2;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}

public class OpFuncDecl3 : VOpcode
{
    public override string OverrideString { get; set; } =
        "local CurrentInst = Inst Stk[Inst[OP_A]]=function() return CurrentInst[OP_A] end;";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.FuncDecl3;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}