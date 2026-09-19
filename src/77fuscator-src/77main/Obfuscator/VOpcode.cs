using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator
{
    public abstract class VOpcode
    {
        public int VIndex;

        public Instruction Previous;
        public abstract string OverrideString { get; set; }

        public abstract bool IsInstruction(Instruction instruction);

        public abstract string GetObfuscated(ObfuscationContext context);

        public virtual void Mutate(Instruction instruction)
        { }
    }
}