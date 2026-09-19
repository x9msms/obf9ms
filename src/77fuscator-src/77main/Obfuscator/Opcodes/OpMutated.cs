using geniussolution.Bytecode_Library.IR;
using System;
using System.Security.Cryptography;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpMutated : VOpcode
    {
        public static Random rand = new Random();

        public VOpcode Mutated;
        public int[] Registers;

        public override string OverrideString { get; set; } = "";

        public static string[] RegisterReplacements = { "OP__A", "OP__B", "OP__C" };

        public override bool IsInstruction(Instruction instruction) =>
            false;

        public bool CheckInstruction() =>
            RandomNumberGenerator.GetInt32(1, 15) == 1;

        public override string GetObfuscated(ObfuscationContext context)
        {
            Console.WriteLine(Mutated is null);
            if (Mutated is not null)
                return Mutated.GetObfuscated(context);

            return "";
        }

        public override void Mutate(Instruction instruction)
        {
            if (Mutated is not null)
                Mutated.Mutate(instruction);
        }
    }
}