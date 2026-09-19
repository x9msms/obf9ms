using geniussolution.Bytecode_Library.Bytecode;
using geniussolution.Bytecode_Library.IR;
using System;

namespace geniussolution.ControlFlowAns;

public static class BlockGenerator
{
    public static ControlFlow Generate(Chunk prototype)
    {
        var currentLine = new Block
        {
            BlockType = BlockType.Root
        };

        var controlFlow = new ControlFlow { RootBlock = currentLine };

        controlFlow.Blocks.Add(currentLine);

        Instruction? previousInstruction = null;

        for (var i = 0; i < prototype.Instructions.Count; i++)
        {
            var instruction = prototype.Instructions[i];

            if (previousInstruction is not null)
                previousInstruction.NextInstruction = instruction;

            switch (instruction.OpCode)
            {
                case Opcode.Eq:
                case Opcode.Lt:
                case Opcode.Le:
                case Opcode.Test:
                case Opcode.TestSet:
                case Opcode.TForLoop:
                    {
                        var newLine = new Block
                        {
                            BlockType = instruction.OpCode is Opcode.TForLoop ? BlockType.GenericLoop : BlockType.Comparison
                        };

                        newLine.Instructions.Add(instruction);
                        newLine.Instructions.Add(prototype.Instructions[i + 1]);

                        instruction.NextInstruction = prototype.Instructions[i + 1];

                        currentLine.NextBlock = newLine;
                        currentLine.HasNextBlock = true;

                        currentLine = newLine;

                        controlFlow.Blocks.Add(newLine);

                        ++i; /* Skip next instruction */

                        break;
                    }

                case Opcode.Jmp:
                case Opcode.ForPrep:
                case Opcode.ForLoop:
                case Opcode.Return:
                case Opcode.UnpackTable:
                    {
                        var newLine = new Block
                        {
                            BlockType = instruction.OpCode switch
                            {
                                Opcode.Jmp => BlockType.Jump,
                                Opcode.ForPrep => BlockType.NumericLoop,
                                Opcode.ForLoop => BlockType.NumericLoop,
                                _ => BlockType.Return
                            }
                        };

                        newLine.Instructions.Add(instruction);

                        currentLine.NextBlock = newLine;
                        currentLine.HasNextBlock = true;

                        currentLine = newLine;

                        controlFlow.Blocks.Add(newLine);

                        break;
                    }

                case Opcode.Closure:
                    {
                        var newLine = new Block
                        {
                            BlockType = BlockType.Closure
                        };

                        if (previousInstruction is not null)
                            previousInstruction.NextInstruction = instruction;

                        previousInstruction = instruction;
                        newLine.Instructions.Add(instruction);

                        for (var j = 0; j < prototype.Functions[instruction.B].UpvalueCount; j++)
                        {
                            ++i;
                            previousInstruction.NextInstruction = instruction;
                            previousInstruction = instruction;
                            newLine.Instructions.Add(prototype.Instructions[i]);
                        }

                        currentLine.NextBlock = newLine;
                        currentLine.HasNextBlock = true;

                        currentLine = newLine;

                        controlFlow.Blocks.Add(newLine);

                        break;
                    }

                case Opcode.GetInstruction:
                case Opcode.GetTableN when instruction.ObfuscationType is ObfuscationType.StringEncryption:
                    {
                        var instructions = instruction.ObfuscationType switch
                        {
                            ObfuscationType.Redirect => 4,
                            ObfuscationType.RegisterReload => 6,
                            ObfuscationType.StringEncryption => 6,
                            _ => throw new Exception("Unsupported obfuscation type passed to control flow analysis")
                        };

                        var newLine = new Block
                        {
                            BlockType = BlockType.Obfuscated
                        };

                        for (var j = 0; j < instructions; j++)
                        {
                            if (previousInstruction is not null)
                                previousInstruction.NextInstruction = prototype.Instructions[i];

                            newLine.Instructions.Add(prototype.Instructions[i]);
                            previousInstruction = prototype.Instructions[i];

                            ++i;
                        }

                        if (previousInstruction is not null)
                            previousInstruction.NextInstruction = prototype.Instructions[i];
                        newLine.Instructions.Add(prototype.Instructions[i]);

                        currentLine.NextBlock = newLine;
                        currentLine.HasNextBlock = true;

                        currentLine = newLine;

                        controlFlow.Blocks.Add(newLine);

                        break;
                    }

                default:
                    {
                        if (currentLine.BlockType
                            is BlockType.Root
                            or BlockType.Jump
                            or BlockType.GenericLoop
                            or BlockType.NumericLoop
                            or BlockType.Comparison
                            or BlockType.Return
                            or BlockType.Closure
                            or BlockType.Obfuscated)
                        {
                            var newLine = new Block();

                            currentLine.NextBlock = newLine;
                            currentLine.HasNextBlock = true;

                            currentLine = newLine;

                            controlFlow.Blocks.Add(newLine);
                        }

                        currentLine.Instructions.Add(instruction);

                        break;
                    }
            }

            previousInstruction = prototype.Instructions[i];
        }

        controlFlow.UpdateMappings();

        return controlFlow;
    }
}