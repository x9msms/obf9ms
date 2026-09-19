/*using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpDynamicCallB2C2 : VOpcode
{
    public override string OverrideString { get; set; } = @"
local A = Inst[OP_A];
local B = Inst[OP_B];
local Results = { Env[Const[B]](Unpack(Stk, A + 1, B)) };
local Edx = 0;
for Idx = A, Inst[OP_C] do
	Edx = Edx + 1;
	Stk[Idx] = Results[Edx];
end;
Stk[A]=Env[Const[B]];
print('sup homie');
";

    public override bool IsInstruction(Instruction instruction) =>
        instruction.OpCode == Opcode.Dynamic && instruction.B > 2 &&
        instruction.C > 2 && instruction.OpcodeBeforeDynamic == Opcode.Call;

    public override string GetObfuscated(ObfuscationContext context) =>
        OverrideString;

    public override void Mutate(Instruction instruction)
    {
        instruction.B += instruction.A - 1;
        instruction.C += instruction.A - 2;
    }
}

public class OpDynamic : VOpcode
{
    public Instruction Owner;

    public override string OverrideString { get; set; } = @"";
    public string ObfuscatedOverride = "";

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.Dynamic;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return OverrideString;
    }
}*/