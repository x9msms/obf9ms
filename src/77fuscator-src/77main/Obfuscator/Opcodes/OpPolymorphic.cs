using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes;

public class OpPolymorphic : VOpcode
{
    public string ObfuscatedOverride = "";
    public Instruction Owner;

    public override string OverrideString { get; set; }

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.Polymorphic;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return ObfuscatedOverride;
    }
}

public class OpDynamic : VOpcode
{
    public string ObfuscatedOverride = "";
    public Instruction Owner;

    public override string OverrideString { get; set; }

    public override bool IsInstruction(Instruction instruction)
    {
        return instruction.OpCode == Opcode.Dynamic;
    }

    public override string GetObfuscated(ObfuscationContext context)
    {
        return ObfuscatedOverride;
    }
}