using geniussolution.Extensions;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace geniussolution.Rewriters;

internal class NumberObfuscation : LuaSyntaxRewriter
{
    private readonly bool Force;

    public NumberObfuscation(bool force = false)
    {
        Force = force;
    }

    public static SyntaxNode ObfuscateNumber(double d)
    {
        if (d != Math.Floor(d) || d <= 0)
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericalLiteralExpression, SyntaxFactory.Literal(d));

        return new NumberObfuscation(true).Visit(
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericalLiteralExpression, SyntaxFactory.Literal(d))).WithAdditionalAnnotations(Flattener.FlattenerSkipAnnotation);
    }

    private enum NumberType
    {
        Add,
        Sub,
    }

    private static readonly List<NumberType> NumberTypes = new() { NumberType.Add, NumberType.Sub };

    private class NumberVirtual
    {
        public NumberType Type;
        public double Enum;
        public double Offset;
    }

    private static (double, StatementListSyntax) Generate(double original)
    {
        var numberVirtuals = new List<NumberVirtual>();
        var obfuscatedNumber = original;

        var squattedNumbers = new HashSet<double>();

        var iterations = RandomNumberGenerator.GetInt32(4, 12);

        for (var i = 0; i < iterations; i++)
        {
            var type = NumberTypes.Random();
            var offset = RandomNumberGenerator.GetInt32(1, 99999);

            switch (type)
            {
                case NumberType.Add:
                    {
                        obfuscatedNumber -= offset;
                        break;
                    }
                case NumberType.Sub:
                    {
                        obfuscatedNumber += offset;
                        break;
                    }
            }

            if (!squattedNumbers.Add(obfuscatedNumber))
                break;

            var @virtual = new NumberVirtual
            {
                Type = type,
                Enum = obfuscatedNumber,
                Offset = offset
            };

            numberVirtuals.Add(@virtual);
        }

        var statements = new List<StatementSyntax>();
        var treeEntries = new List<BinaryTreeEntry>();

        numberVirtuals.Reverse();

        statements.Add(SyntaxFactory.ParseStatement("local iterations = 0"));

        foreach (var @virtual in numberVirtuals)
        {
            var virtualString = @virtual.Type switch
            {
                NumberType.Add => $"Enum = Enum + {@virtual.Offset}",
                NumberType.Sub => $"Enum = Enum - {@virtual.Offset}",
                _ => throw new Exception("Incorrect number type passed to switch expression!")
            };

            var virtualStatements = SyntaxFactory.ParseCompilationUnit(virtualString).Statements.Statements.ToList();
            virtualStatements.Add(SyntaxFactory.ParseStatement("iterations = iterations + 1"));

            treeEntries.Add(new BinaryTreeEntry
            {
                Body = SyntaxFactory.StatementList(virtualStatements),
                Enum = @virtual.Enum.ToString(CultureInfo.InvariantCulture),
                SortEnum = (int)@virtual.Enum
            });
        }

        var whileStatements = new List<StatementSyntax>();
        whileStatements.AddRange(BinaryTreeGenerator.CreateBinarySearchTree(treeEntries).Statements);
        whileStatements.Add(SyntaxFactory.ParseStatement($"if iterations == {iterations} then break end"));

        statements.Add(SyntaxFactory.WhileStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
            SyntaxFactory.StatementList(whileStatements)));
        statements.Add(SyntaxFactory.ReturnStatement(SyntaxFactory.SeparatedList(new List<ExpressionSyntax>
            { SyntaxFactory.IdentifierName("Enum") })));

        return (obfuscatedNumber, (StatementListSyntax)SyntaxFactory.StatementList(statements).NormalizeWhitespace());
    }

    public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
    {
        if (node.Names[0].Name is "tab")
            return node;

        return base.VisitLocalVariableDeclarationStatement(node);
    }

    public override SyntaxNode? VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
    {
        if (node.Name.Name is "Wrap")
            return node;

        return base.VisitLocalFunctionDeclarationStatement(node);
    }

    public override SyntaxNode? VisitAssignmentStatement(AssignmentStatementSyntax node)
    {
        if (node.Variables[0] is IdentifierNameSyntax { Name: "Wrap" })
            return node;

        return base.VisitAssignmentStatement(node);
    }

    public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        if (!node.IsKind(SyntaxKind.NumericalLiteralExpression) || node.Token.Value is not double d || d != Math.Floor(d) || d <= 0)
            return base.VisitLiteralExpression(node)!;

        if (node.AncestorsAndSelf().Any(ancestor => ancestor.HasAnnotation(Flattener.FlattenerSkipAnnotation)))
            return base.VisitLiteralExpression(node)!;

        if (!Force && RandomNumberGenerator.GetInt32(6) != 2)
            return base.VisitLiteralExpression(node)!;

        var (obfuscated, statementList) = Generate(d);

        var functionWrapper = SyntaxFactory.AnonymousFunctionExpression(SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(new List<ParameterSyntax> { SyntaxFactory.NamedParameter("Enum") })),
            statementList).WithAdditionalAnnotations(Flattener.FlattenerSkipAnnotation);

        var initialNumber = SyntaxFactory.LiteralExpression(SyntaxKind.NumericalLiteralExpression,
            SyntaxFactory.Literal(obfuscated));

        return SyntaxFactory.FunctionCallExpression(SyntaxFactory.ParenthesizedExpression(functionWrapper),
            SyntaxFactory.ExpressionListFunctionArgument(SyntaxFactory.SeparatedList(new List<ExpressionSyntax>
                { initialNumber }))).WithAdditionalAnnotations(Flattener.FlattenerSkipAnnotation).NormalizeWhitespace();
    }
}