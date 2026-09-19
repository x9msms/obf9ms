using geniussolution.Bytecode_Library.Bytecode;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace geniussolution.Bytecode_Library.IR
{
    public class Chunk
    {
        public string Name;
        public int Line;
        public int LastLine;
        public byte UpvalueCount;
        public byte ParameterCount;
        public byte VarargFlag;
        public byte StackSize;
        public int CurrentOffset = 0;
        public int CurrentParamOffset = 0;
        public List<Instruction> Instructions;
        public Dictionary<Instruction, int> InstructionMap = new Dictionary<Instruction, int>();
        public List<Constant> Constants;
        public Dictionary<Constant, int> ConstantMap = new Dictionary<Constant, int>();
        public List<Chunk> Functions;
        public Dictionary<Chunk, int> FunctionMap = new Dictionary<Chunk, int>();
        public List<string> Upvalues;
        public Instruction? FinalAntiTamperInstruction;
        public int PrototypeId;
        public bool HasAntiTamper = false;

        public static string String(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        }

        public readonly Dictionary<string, string> ObfuscatedEnvironmentNames = new()
        {
            {"math", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"string", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"table", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"coroutine", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"debug", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"os", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"task", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"buffer", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"bit", String(RandomNumberGenerator.GetInt32(2, 12))},
            {"bit32", String(RandomNumberGenerator.GetInt32(2, 12))},
    };

        public void UpdateMappings()
        {
            InstructionMap.Clear();
            ConstantMap.Clear();
            FunctionMap.Clear();

            for (int i = 0; i < Instructions.Count; i++)
                InstructionMap.Add(Instructions[i], i);

            for (int i = 0; i < Constants.Count; i++)
                ConstantMap.Add(Constants[i], i);

            for (int i = 0; i < Functions.Count; i++)
                FunctionMap.Add(Functions[i], i);
        }
    }
}