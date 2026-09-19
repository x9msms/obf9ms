using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using geniussolution.Obfuscator.Opcodes;

namespace geniussolution.Obfuscator;

public class DynamicData
{
    public OpDynamic Virtual;

    public string OverrideString { get; set; } = "";

    //public OpDynamic Virtual;
    public Instruction PreviousData;

    public Opcode PreviousOpcode;
}