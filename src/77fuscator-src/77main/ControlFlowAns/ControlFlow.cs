using geniussolution.Bytecode_Library.IR;
using System.Collections.Generic;

namespace geniussolution.ControlFlowAns;

public class ControlFlow
{
    public readonly List<Block> Blocks = new();

    public Block RootBlock;

    public void UpdateMappings()
    {
        for (var index = 0; index < Blocks.Count; index++)
        {
            var block = Blocks[index];
            block.BlockId = index;

            foreach (var instruction in block.Instructions)
            {
                if (instruction.JumpBlock is { } jumpBlock)
                {
                    instruction.InstructionReference = jumpBlock.Instructions[0];
                }

                instruction.EncompassingBlock = block;
            }
        }
    }

    public List<Instruction> ReconstructInstructions()
    {
        var instructions = new List<Instruction>();

        foreach (var block in Blocks) instructions.AddRange(block.Instructions);

        return instructions;
    }
}