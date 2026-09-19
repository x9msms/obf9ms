using System.Collections.Generic;
using System.Security.Cryptography;

namespace geniussolution.Obfuscator.New_Generation.Dynamic_Structure_Generation
{
    public class Utils
    {
        public static string getName(List<string> ListName, List<string> usedRegisters)
        {
            string register;

            do
            {
                register = ListName[RandomNumberGenerator.GetInt32(0, ListName.Count)];
            } while (usedRegisters.Contains(register));

            usedRegisters.Add(register);

            return register;
        }
    }
}