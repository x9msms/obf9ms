using geniussolution.Extensions;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

namespace geniussolution.Rewriters;

internal class Flattener : LuaSyntaxRewriter
{
    public static readonly SyntaxAnnotation FlattenerSkipAnnotation = new();

    private readonly bool _intensive;
    private bool _bitXorDeclared;

    public Flattener(bool intensive = false)
    {
        _intensive = intensive;
    }

    private static List<string> CollectLocalVariables(IEnumerable<StatementSyntax> statements)
    {
        var usedVariables = new List<string>();

        foreach (var statement in statements)
        {
            switch (statement)
            {
                case LocalVariableDeclarationStatementSyntax localVariableDeclaration:
                    usedVariables.AddRange(localVariableDeclaration.Names
                        .Where(variable => !usedVariables.Contains(variable.Name))
                        .Select(variable => variable.Name));
                    break;

                case LocalFunctionDeclarationStatementSyntax localFunctionDeclaration:
                    if (!usedVariables.Contains(localFunctionDeclaration.Name.Name))
                    {
                        usedVariables.Add(localFunctionDeclaration.Name.Name);
                    }

                    break;
            }
        }

        return usedVariables;
    }

    private static StatementSyntax DelocalizeStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case LocalVariableDeclarationStatementSyntax localVariableDeclaration:
                {
                    var names = localVariableDeclaration.Names.Select(name => IdentifierName(name.Name)).ToList();

                    return AssignmentStatement(
                        SeparatedList<PrefixExpressionSyntax>(names),
                        localVariableDeclaration.EqualsValues ??
                        EqualsValuesClause(
                            SingletonSeparatedList<ExpressionSyntax>(LiteralExpression(SyntaxKind.NilLiteralExpression))));
                }
            case LocalFunctionDeclarationStatementSyntax localFunctionDeclaration:
                {
                    var identifierName = IdentifierName(localFunctionDeclaration.Name.Name);

                    var newAssignment = AssignmentStatement(
                        SeparatedList<PrefixExpressionSyntax>(new[] { identifierName }),
                        SeparatedList<ExpressionSyntax>(new[]
                        {
                        AnonymousFunctionExpression(localFunctionDeclaration.Parameters, localFunctionDeclaration.Body)
                        })
                    );

                    return newAssignment;
                }
        }

        return statement;
    }

    private enum NumberObfuscationType
    {
        Xor,
        Add
    }

    private static string ObfuscateNumber(int steps, double value)
    {
        var stepsList = new List<NumberObfuscationType>();
        var values = new List<double>();

        for (var I = 0; I < steps; I++)
        {
            var step = RandomNumberGenerator.GetInt32(2) == 0 ? NumberObfuscationType.Add : NumberObfuscationType.Xor;
            stepsList.Add(step);
            values.Add(RandomNumberGenerator.GetInt32(0, 1000000));
        }

        var stepIndex = 0;

        foreach (var step in stepsList)
        {
            switch (step)
            {
                case NumberObfuscationType.Xor:
                    {
                        value = (int)value ^ (int)values[stepIndex];
                        break;
                    }
                case NumberObfuscationType.Add:
                    {
                        value += values[stepIndex];
                        break;
                    }
            }

            stepIndex++;
        }

        var obfuscatedNumberExpression = value.ToString(CultureInfo.InvariantCulture);
        var index = stepsList.Count - 1;

        for (var stepIndexReverse = stepsList.Count - 1; stepIndexReverse >= 0; stepIndexReverse--)
        {
            var step = stepsList[stepIndexReverse];

            obfuscatedNumberExpression = step switch
            {
                NumberObfuscationType.Xor => $"BitXOR({obfuscatedNumberExpression}, {values[index]})",
                NumberObfuscationType.Add => $"({obfuscatedNumberExpression}) - {values[index]}",
                _ => obfuscatedNumberExpression
            };

            index--;
        }

        return obfuscatedNumberExpression;
    }

    private StatementListSyntax FlattenControlFlow(SyntaxList<StatementSyntax> mainStatements)
    {
        if (_intensive && mainStatements.Count < 3)
            return StatementList(mainStatements);

        var newBody = new List<StatementSyntax>();
        var splitStatements = mainStatements.ToList().Chunk(4).ToList();
        var collectedLocalNames = new List<LocalDeclarationNameSyntax>();

        foreach (var statements in splitStatements)
        {
            if (statements.Length == 1)
            {
                newBody.Add(statements[0]);
                continue;
            }

            var enumName = $"IB_SUPER_SECRET_FLATTENER_ENUM_{RandomNumberGenerator.GetInt32(0, 10000)}";

            var localVariables = CollectLocalVariables(statements);

            collectedLocalNames.AddRange(localVariables.Select(LocalDeclarationName));

            var canUseNumberObfuscation = _bitXorDeclared;

            if (localVariables.Contains("BitXOR"))
            {
                _bitXorDeclared = true;
            }

            var keys = new List<int>();

            for (var index = 0; index < statements.Length + 1; index++)
            {
                int key;

                do
                {
                    key = RandomNumberGenerator.GetInt32(statements.Length + 1);
                } while (keys.Contains(key));

                keys.Add(key);
            }

            var binaryTreeEntries = new List<BinaryTreeEntry>();

            for (var index = 0; index < statements.Length; index++)
            {
                var modifiedStatements = new List<StatementSyntax> { DelocalizeStatement(statements[index]) };

                var nextEnum = _intensive && canUseNumberObfuscation
                    ? ParseExpression(RandomNumberGenerator.GetInt32(0, 8) == RandomNumberGenerator.GetInt32(0, 8) ? ObfuscateNumber(RandomNumberGenerator.GetInt32(3, 7), keys[index + 1]) : NumberObfuscation.ObfuscateNumber(keys[index + 1]).ToString()).WithAdditionalAnnotations(FlattenerSkipAnnotation)
                    : LiteralExpression(
                        SyntaxKind.NumericalLiteralExpression,
                        Literal(keys[index + 1]).WithAdditionalAnnotations(FlattenerSkipAnnotation)
                    );

                modifiedStatements.Insert(RandomNumberGenerator.GetInt32(modifiedStatements.Count), AssignmentStatement(
                    SingletonSeparatedList<PrefixExpressionSyntax>(IdentifierName(enumName)),
                    EqualsValuesClause(
                        SingletonSeparatedList(nextEnum))
                ).WithAdditionalAnnotations(FlattenerSkipAnnotation));

                binaryTreeEntries.Add(new BinaryTreeEntry
                {
                    Body = StatementList(modifiedStatements),
                    Enum = keys[index].ToString(),
                    SortEnum = keys[index]
                });
            }

            binaryTreeEntries.Add(new BinaryTreeEntry
            {
                Body = StatementList(BreakStatement()),
                Enum = keys[statements.Length].ToString(),
                SortEnum = keys[statements.Length]
            });

            var binaryTree = BinaryTreeGenerator.CreateBinarySearchTree(binaryTreeEntries, enumName)
                .NormalizeWhitespace();

            var localNames = new List<LocalDeclarationNameSyntax> { LocalDeclarationName(enumName) };

            var newLocalDeclaration = LocalVariableDeclarationStatement(
                SeparatedList(localNames),
                SeparatedList(new List<ExpressionSyntax>
                    {
                        LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(keys[0]))
                    }
                )).NormalizeWhitespace();

            var body = List<StatementSyntax>();
            body = body.AddRange(binaryTree.Statements);

            var flattenedBody = new List<StatementSyntax>
            {
                newLocalDeclaration,
                WhileStatement(
                    LiteralExpression(SyntaxKind.TrueLiteralExpression),
                    StatementList(body)
                )
            };

            newBody.AddRange(flattenedBody);
        }

        if (collectedLocalNames.Count > 0)
        {
            collectedLocalNames.Shuffle();

            newBody.Insert(0, LocalVariableDeclarationStatement(
                SeparatedList(collectedLocalNames)).WithAdditionalAnnotations(FlattenerSkipAnnotation));
        }

        return StatementList(newBody).WithAdditionalAnnotations(FlattenerSkipAnnotation);
    }

    private static int _created;

    public override SyntaxNode VisitGenericForStatement(GenericForStatementSyntax node)
    {
        _created++;
        var updated = VisitList(node.Body.Statements);
        var body = StatementList(updated);
        // if (Random.Next(2) == 1) // modify statement

        var i = node.Identifiers[0].Name;
        var v = node.Identifiers[1].Name;
        var newLoop = ParseStatement(@$"
                    do
                        local iterator_{_created}, tableToIterate_{_created}, {i}, {v} = {node.Expressions};
                        while true do
                            {i}, {v} = iterator_{_created}(tableToIterate_{_created}, {i})
                            if not {i} then break end;
                            {body}
                        end;
                    end;
                    ");
        return newLoop.NormalizeWhitespace();

        //return base.VisitGenericForStatement(node).NormalizeWhitespace();
    }

    public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
    {
        var updated = VisitList(node.Statements.Statements);
        return node.Update(StatementList(FlattenControlFlow(updated).Statements), node.EndOfFileToken)
            .NormalizeWhitespace();
    }

    public override SyntaxNode VisitStatementList(StatementListSyntax node)
    {
        var updated = VisitList(node.Statements);

        // It is completely useless to flatten do statements because they're always a single statement
        if (node.Parent is DoStatementSyntax)
            return node.Update(updated);

        return node.Update(FlattenControlFlow(node.Statements).Statements).NormalizeWhitespace();
    }

    public override SyntaxNode VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
    {
        // Prevent the interpreter from being flattened
        if (node.Name.Name is "Wrap" || node.HasAnnotation(FlattenerSkipAnnotation))
            return node;

        return LocalFunctionDeclarationStatement(node.Name, node.Parameters,
            FlattenControlFlow(VisitList(node.Body.Statements)));
    }

    public override SyntaxNode VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
    {
        if (node.HasAnnotation(FlattenerSkipAnnotation))
            return node;

        var updated = VisitList(node.Body.Statements);
        return FunctionDeclarationStatement(node.Name, node.Parameters, FlattenControlFlow(updated))
            .NormalizeWhitespace();
    }

    public override SyntaxNode VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
    {
        if (node.HasAnnotation(FlattenerSkipAnnotation))
            return node;

        return AnonymousFunctionExpression(node.Parameters, FlattenControlFlow(VisitList(node.Body.Statements)))
            .NormalizeWhitespace();
    }

    public override SyntaxNode VisitNumericForStatement(NumericForStatementSyntax node)
    {
        return NumericForStatement(node.Identifier, node.InitialValue, node.FinalValue, node.StepValue,
                FlattenControlFlow(VisitList(node.Body.Statements)))
            .NormalizeWhitespace();
    }
}