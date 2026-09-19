using geniussolution.Obfuscator.Opcodes;

namespace geniussolution.Obfuscator;

public class PolymorphicData
{
    public bool DontOffsetA = false;
    public bool DontOffsetB = false;
    public bool DontOffsetC = false;
    public bool DontOffsetE = false;
    public bool DontOffsetF = false;

    public bool IsASubtract = false;
    public bool IsBSubtract = false;
    public bool IsCSubtract = false;
    public bool IsESubtract = false;
    public bool IsFSubtract = false;

    public int OffsetA = 0;
    public int OffsetB = 0;
    public int OffsetC = 0;
    public int OffsetE = 0;
    public int OffsetF = 0;

    public OpPolymorphic Virtual;
}