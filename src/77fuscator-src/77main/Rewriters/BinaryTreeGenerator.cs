using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace geniussolution.Rewriters;

using static SyntaxFactory;

public class BinaryTreeEntry
{
    public StatementListSyntax Body;
    public string Enum;
    public int SortEnum;
}

public static class BinaryTreeGenerator
{
    public static StatementListSyntax CreateBinarySearchTree(List<BinaryTreeEntry> nodes, string name = "Enum")
    {
        switch (nodes.Count)
        {
            case 1: // Its just an operation
                return nodes[0].Body;

            case 2: // Its a branch
                return RandomNumberGenerator.GetInt32(8) switch
                {
                    0 => StatementList(IfStatement(
                        ParseExpression($"  {name} > {nodes[0].Enum} "),
                        nodes[1].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[0].Body)
                    )),
                    7 => StatementList(IfStatement(
                        ParseExpression($"  {name} <= {nodes[0].Enum} "),
                        nodes[0].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[1].Body)
                    )),

                    1 => StatementList(IfStatement(
                        ParseExpression($"  {name} == {nodes[0].Enum} "),
                        nodes[0].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[1].Body)
                    )),
                    2 => StatementList(IfStatement(
                        ParseExpression($"  {name} == {nodes[1].Enum} "),
                        nodes[1].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[0].Body)
                    )),

                    3 => StatementList(IfStatement(
                        ParseExpression($"  {name} < {nodes[1].Enum} "),
                        nodes[0].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[1].Body)
                    )),
                    6 => StatementList(IfStatement(
                        ParseExpression($"  {name} >= {nodes[1].Enum} "),
                        nodes[1].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[0].Body)
                    )),

                    4 => StatementList(IfStatement(
                        ParseExpression($"  {name} ~= {nodes[1].Enum} "),
                        nodes[0].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[1].Body)
                    )),
                    5 => StatementList(IfStatement(
                        ParseExpression($"  {name} ~= {nodes[0].Enum} "),
                        nodes[1].Body,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(nodes[0].Body)
                    )),

                    _ => throw new ArgumentOutOfRangeException()
                };

            default:
                var orderedList = nodes.OrderBy(o => o.SortEnum).ToList();
                var (split1, split2) = (orderedList.Take(orderedList.Count / 2).ToList(),
                    orderedList.Skip(orderedList.Count / 2).ToList());

                return RandomNumberGenerator.GetInt32(2) switch
                {
                    0 => StatementList(IfStatement(
                        ParseExpression($"  {name} <= {split1.Last().Enum} "),
                        (StatementListSyntax)CreateBinarySearchTree(split1, name).NormalizeWhitespace(),
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause((StatementListSyntax)CreateBinarySearchTree(split2, name).NormalizeWhitespace())
                    )),
                    1 => StatementList(IfStatement(
                        ParseExpression($"  {name} >= {split2.First().Enum} "),
                        (StatementListSyntax)CreateBinarySearchTree(split2, name).NormalizeWhitespace(),
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause((StatementListSyntax)CreateBinarySearchTree(split1, name).NormalizeWhitespace())
                    )),
                    _ => throw new ArgumentOutOfRangeException()
                };
        }
    }
}