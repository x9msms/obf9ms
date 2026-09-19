using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;

namespace geniussolution.Obfuscator.Opcodes
{
    public class OpPrepStackArgs : VOpcode
    {
        public override string OverrideString { get; set; } = """

                                                          for Idx = 0, PCount do
                                                          	if (Idx >= Params) then
                                                          		Vararg[Idx - Params] = Args[Idx + 1];
                                                          	else
                                                          		Stk[Idx] = Args[Idx + 1];
                                                          	end;
                                                          end;

                                                          """;

        public override bool IsInstruction(Instruction instruction)
        {
            return instruction.OpCode == Opcode.PrepStackArgs;
        }

        public override string GetObfuscated(ObfuscationContext context)
        {
            return OverrideString;
        }
    }
}