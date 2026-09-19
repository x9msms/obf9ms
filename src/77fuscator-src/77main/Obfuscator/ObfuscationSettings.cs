namespace geniussolution.Obfuscator
{
    public class ObfuscationSettings
    {
        public bool EncryptStrings;
        public int DecryptTableLen;
        public bool ExtraCompression;

        public bool EnhancedSecurity;
        public bool DynamicOpcodeStructure;

        public string Watermark;

        public ObfuscationSettings()
        {
            Watermark = "77fuscator 0.6.1 EARLY BUILD";
            EncryptStrings = true;
            ExtraCompression = true;
            EnhancedSecurity = true;
            DecryptTableLen = 500;
            DynamicOpcodeStructure = false;
        }
    }
}