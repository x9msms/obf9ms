namespace geniussolution.Bytecode_Library.IR
{
    public class Constant
    {
        public dynamic? Data;
        public int EncryptionForm;

        public bool IsEncrypted = false;

        public bool TamperConstant = false;

        public ConstantType Type;
    }
}