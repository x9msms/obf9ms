namespace geniussolution.Obfuscator
{
    public class CustomInstructionData
    {
        public VOpcode Opcode;
        public VOpcode WrittenOpcode;
        public bool Preprocessed;
        public bool NopAfterMutate;
        public bool Mutated = false;
    }
}