using geniussolution.Bytecode_Library.IR;
using System;

namespace geniussolution.ControlFlowAns;

public static class EdgeGenerator
{
    public static void Generate(ControlFlow controlFlow)
    {
        foreach (var block in controlFlow.Blocks)
        {
            Instruction? jumpInstruction = null;

            switch (block.BlockType)
            {
                case BlockType.NumericLoop:
                case BlockType.Jump:
                    {
                        jumpInstruction = block.Instructions[0];
                        break;
                    }
                case BlockType.GenericLoop:
                case BlockType.Comparison:
                    {
                        jumpInstruction = block.Instructions[1];
                        break;
                    }
            }

            if (jumpInstruction is null)
                continue;

            if (jumpInstruction.InstructionReference?.EncompassingBlock is null)
                throw new Exception("Uninitialized jump passed to control flow analysis!");

            var encompassingBlock = jumpInstruction.InstructionReference.EncompassingBlock!;

            block.JumpBlock = encompassingBlock;
            encompassingBlock.Edges.Add(block);
        }
    }
}