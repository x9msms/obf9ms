using geniussolution.Bytecode_Library.IR;
using System.Collections.Generic;

namespace geniussolution.ControlFlowAns;

public class Block
{
    public int BlockId;

    public readonly List<Instruction> Instructions = new();
    public readonly List<Block> Edges = new();

    public BlockType BlockType = BlockType.Straight;

    public bool HasNextBlock = false;
    public Block? NextBlock;

    public Block? JumpBlock;
}