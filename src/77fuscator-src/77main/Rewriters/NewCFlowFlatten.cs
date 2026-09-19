using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

namespace LorettaTest;

public static class GenericFlattener
{
    private static readonly Random Rand = new();
    private static int _created;
    public static readonly SyntaxAnnotation FlattenerSkipAnnotation = new();

    private static StatementSyntax FixStatement(StatementSyntax statement)
    {
        if (statement.IsKind(SyntaxKind.LocalVariableDeclarationStatement))
        {
            var variableInfo = statement.FirstAncestorOrSelf<LocalVariableDeclarationStatementSyntax>();

            if (variableInfo!.EqualsValues is null)
                return SyntaxUtilities.ParseStatement($"{variableInfo.Names} = nil");

            statement = SyntaxUtilities.ParseStatement($"{variableInfo.Names} {variableInfo.EqualsValues}");
        }
        else if (statement.IsKind(SyntaxKind.LocalFunctionDeclarationStatement))
        {
            var functionInfo = statement.FirstAncestorOrSelf<LocalFunctionDeclarationStatementSyntax>();

            var identifierName = IdentifierName(functionInfo!.Name.Identifier.Text);

            var newAssignment = AssignmentStatement(
                SeparatedList<PrefixExpressionSyntax>(new[] { identifierName }),
                SeparatedList<ExpressionSyntax>(new[]
                {
                    AnonymousFunctionExpression(functionInfo.TypeParameterList, functionInfo.Parameters,
                        functionInfo.TypeBinding, functionInfo.Body)
                })
            );
            statement = newAssignment;
        }

        return statement!.NormalizeWhitespace();
    }

    private static StatementListSyntax CreateBinarySearchTree(List<(int Enum, StatementListSyntax @virtual)> cut,
        string name)
    {
        switch (cut.Count)
        {
            case 1: // Its just an operation
                return cut[0].@virtual.NormalizeWhitespace();

            case 2: // Its a branch
                return Rand.Next(8) switch
                {
                    0 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{name} > {cut[0].Enum}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[1].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[0].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    1 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{name} == {cut[0].Enum}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[0].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[1].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    2 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{name} < {cut[1].Enum}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[0].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[1].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    3 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{name} ~= {cut[1].Enum}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[0].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[1].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    4 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{cut[1].Enum} ~= {name}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[0].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[1].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    5 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{cut[0].Enum} == {name}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[0].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[1].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    6 => StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{cut[0].Enum} < {name}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        cut[1].@virtual,
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(cut[0].@virtual.NormalizeWhitespace())
                    )).NormalizeWhitespace(),
                    7 => StatementList(IfStatement(
                       (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{cut[1].Enum} > {name}",
                           SyntaxUtilities.CacheKind.ExpressionKind),
                       cut[0].@virtual,
                       new SyntaxList<ElseIfClauseSyntax>(),
                       ElseClause(cut[1].@virtual.NormalizeWhitespace())
                   )).NormalizeWhitespace(),
                };

            default:
                var orderedList = cut.OrderBy(o => o).ToList();
                var (split1, split2) = (orderedList.Take(orderedList.Count / 2).ToList(),
                    orderedList.Skip(orderedList.Count / 2).ToList());

                return StatementList(IfStatement(
                        (ExpressionSyntax)SyntaxUtilities.GetCachedNodeFor($"{name} <= {split1.Last().Enum}",
                            SyntaxUtilities.CacheKind.ExpressionKind),
                        CreateBinarySearchTree(split1, name).NormalizeWhitespace(),
                        new SyntaxList<ElseIfClauseSyntax>(),
                        ElseClause(CreateBinarySearchTree(split2, name).NormalizeWhitespace())
                    )).NormalizeWhitespace();
        }
    }

    private static List<string> GetVariables(SyntaxList<StatementSyntax> statements)
    {
        var usedVariables = new List<string>();

        void AddVariable(string name)
        {
            if (!usedVariables.Contains(name))
                usedVariables.Add(name);
        }

        foreach (var statement in statements)
        {
            switch (statement)
            {
                case LocalVariableDeclarationStatementSyntax loc:
                    {
                        foreach (var var in loc.Names)
                            AddVariable(var.Name);
                        break;
                    }
                case LocalFunctionDeclarationStatementSyntax func:
                    AddVariable(func.Name.Name);
                    break;
            }
        }

        return usedVariables;
    }

    private static StatementListSyntax CreateStepCFlow(SyntaxList<StatementSyntax> statementsOrder)
    {
        var stepName = "step_" + _created;
        var startingValue = 0;
        var stepValue = 1;

        #region CFlow generation

        var variables = GetVariables(statementsOrder);

        var informationList = new List<(int, StatementListSyntax)>();

        statementsOrder = statementsOrder.Add(BreakStatement());

        foreach (var statement in statementsOrder)
        {
            var innerStats = statement.ChildNodes().Where(stat => stat is StatementListSyntax); // has inner statement list
            if (innerStats.Any())
            {
                foreach (var syntaxNode in innerStats)
                {
                    var istat = (StatementListSyntax)syntaxNode;
                    statementsOrder.Add(statement);
                }
            }

            var index = statementsOrder.IndexOf(statement);
            //var next = statementsOrder.Count >= index + 1 ? statementsOrder[0] : statementsOrder[index + 1];
            informationList.Add((startingValue + index * stepValue,
                    StatementList(new List<StatementSyntax>
                        { FixStatement(statement) }) // change this later to basically make super operators
                ));
        }

        var cflow = CreateBinarySearchTree(informationList, stepName).NormalizeWhitespace();

        #endregion CFlow generation

        variables.Insert(0, stepName);

        var localNames = variables.Select(LocalDeclarationName).ToList();

        var localDecl = LocalVariableDeclarationStatement(
            SeparatedList(localNames),
            SingletonSeparatedList<ExpressionSyntax>(LiteralExpression(SyntaxKind.NumericalLiteralExpression,
                Literal(startingValue)))
        );

        var binOp = BinaryExpression(SyntaxKind.AddExpression, IdentifierName(stepName),
            Token(SyntaxFacts.GetOperatorTokenKind(SyntaxKind.AddExpression).Value),
            LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(stepValue)));
        var body = List<StatementSyntax>();
        body = body.AddRange(cflow.Statements);
        body = body.Add(AssignmentStatement(
            SingletonSeparatedList<PrefixExpressionSyntax>(IdentifierName(stepName)),
            SingletonSeparatedList<ExpressionSyntax>(binOp)
        ));

        _created++;
        return StatementList(localDecl, WhileStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression), StatementList(body))).NormalizeWhitespace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining & MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<IEnumerable<T>> SplitByChunks<T>(IEnumerable<T> source, int chunkSize)
    {
        return source
            .Select((x, i) => new { Value = x, Index = i })
            .GroupBy(x => x.Index / chunkSize)
            .Select(g => g.Select(x => x.Value));
    }

    private static StatementListSyntax CreateIncrementalCFlow(SyntaxList<StatementSyntax> statementChunks)
    {
        return CreateStepCFlow(statementChunks).NormalizeWhitespace();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static LocalFunctionDeclarationStatementSyntax CreateFunctionFromBody(LocalFunctionDeclarationStatementSyntax node, SyntaxList<StatementSyntax> body) =>
        node.Update(node.LocalKeyword, node.FunctionKeyword, node.Name, node.TypeParameterList, node.Parameters, node.TypeBinding, StatementList(body), node.EndKeyword, node.SemicolonToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static FunctionDeclarationStatementSyntax CreateFunctionFromBody(FunctionDeclarationStatementSyntax node, SyntaxList<StatementSyntax> body) =>
        node.Update(node.FunctionKeyword, node.Name, node.TypeParameterList, node.Parameters, node.TypeBinding, StatementList(body), node.EndKeyword, node.SemicolonToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AnonymousFunctionExpressionSyntax CreateFunctionFromBody(AnonymousFunctionExpressionSyntax node, SyntaxList<StatementSyntax> body) =>
        node.Update(node.FunctionKeyword, node.TypeParameterList, node.Parameters, node.TypeBinding, StatementList(body), node.EndKeyword);

    private class CFlowRewriter : LuaSyntaxRewriter
    {
        private int created;

        public CFlowRewriter() => created = 0;

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = (ExpressionSyntax)Visit(node.Left);
            var right = (ExpressionSyntax)Visit(node.Right);

            var operatorToken = node.OperatorToken;

            var gr = Rand.Next(2) == 0;

            ExpressionSyntax start;
            ExpressionSyntax end;

            if (Rand.Next(2) == 1)
                goto no_change;

            switch (operatorToken.Kind())
            {
                case SyntaxKind.EqualsEqualsToken:
                    {
                        node = node.Update(left, Token(SyntaxKind.TildeEqualsToken), right);

                        return UnaryExpression(SyntaxKind.UnaryMinusExpression, Token(SyntaxKind.NotKeyword), ParenthesizedExpression(node)).NormalizeWhitespace();
                    }

                case SyntaxKind.TildeEqualsToken:
                    {
                        node = node.Update(left, Token(SyntaxKind.EqualsEqualsToken), right);

                        return UnaryExpression(SyntaxKind.UnaryMinusExpression, Token(SyntaxKind.NotKeyword), ParenthesizedExpression(node)).NormalizeWhitespace();
                    }

                case SyntaxKind.GreaterThanEqualsToken:
                    {
                        start = BinaryExpression(SyntaxKind.GreaterThanExpression, left, Token(SyntaxKind.GreaterThanToken), right);
                        end = BinaryExpression(SyntaxKind.EqualsExpression, left, Token(SyntaxKind.EqualsEqualsToken), right);

                        node = BinaryExpression(
                            SyntaxKind.ExclusiveOrExpression,
                            gr ? start : end,
                            Token(SyntaxKind.OrKeyword),
                            gr ? end : start
                        );

                        return ParenthesizedExpression(node).NormalizeWhitespace();
                    }

                case SyntaxKind.LessThanEqualsToken:
                    {
                        start = BinaryExpression(SyntaxKind.LessThanExpression, left, Token(SyntaxKind.LessThanToken), right);
                        end = BinaryExpression(SyntaxKind.EqualsExpression, left, Token(SyntaxKind.EqualsEqualsToken), right);

                        node = BinaryExpression(
                            SyntaxKind.ExclusiveOrExpression,
                            gr ? start : end,
                            Token(SyntaxKind.OrKeyword),
                            gr ? end : start
                        );

                        return ParenthesizedExpression(node).NormalizeWhitespace();
                    }
            }

            node = node.Update(left, operatorToken, right);

        no_change:
            return Rand.Next(2) == 0 ? node : ParenthesizedExpression(node).NormalizeWhitespace(); // avoid specific regex
        }

        public override SyntaxNode VisitTableConstructorExpression(TableConstructorExpressionSyntax node)
        {
            if (node.Fields.Count <= 0)
                return node.NormalizeWhitespace();

            var lastField = node.Fields.Last();

            if (node.Fields.Any(p => p is ExpressionKeyedTableFieldSyntax or IdentifierKeyedTableFieldSyntax)) // lua 5.1 compiles them before-hand
                return node.NormalizeWhitespace();

            if (lastField is UnkeyedTableFieldSyntax { Value: FunctionCallExpressionSyntax or MethodCallExpressionSyntax or VarArgExpressionSyntax }) // may return multiple values
                return node.NormalizeWhitespace();

            for (var i = 0; i < Rand.Next(1, 5); i++)
                node = node.AddFields(UnkeyedTableField(LiteralExpression(SyntaxKind.NilLiteralExpression)));

            return node.NormalizeWhitespace();
        }

        public override SyntaxNode VisitStatementList(StatementListSyntax node)
        {
            var updated = VisitList(node.Statements);

            // Don't touch it if its wrapped in a do statement
            //if (node.Parent is DoStatementSyntax)
            //    return node.Update(updated);

            //if (updated.Count < 7) // not worth to create a loop
            //    return node.Update(updated);

            if (updated.Count >= 7 || Rand.Next(0, 2) == 54)
            {
                return node.Update(CreateIncrementalCFlow(node.Statements).Statements).NormalizeWhitespace();
            }

            return node.Update(updated);
        }

        public override SyntaxNode VisitGenericForStatement(GenericForStatementSyntax node)
        {
            _created++;
            var updated = VisitList(node.Body.Statements);
            var body = StatementList(updated);
            // if (Random.Next(2) == 1) // modify statement

            var i = node.Identifiers[0].Name;
            var v = node.Identifiers[1].Name;
            /* Reminder to fix this */
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

        public override SyntaxNode VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatementSyntax node)
        {
            if (node.Name.Name is "Wrap" || node.HasAnnotation(FlattenerSkipAnnotation))
                return node;

            var updated = VisitList(node.Body.Statements); // visit updated body (this will also handle with inner functions)
            return CreateFunctionFromBody(node, Rand.Next(0, 3) == 2 ? CreateIncrementalCFlow(updated).Statements : CreateStepCFlow(updated).Statements).NormalizeWhitespace(); // return rewritten body
        }

        public override SyntaxNode VisitFunctionDeclarationStatement(FunctionDeclarationStatementSyntax node)
        {
            if (node.HasAnnotation(FlattenerSkipAnnotation))
                return node;

            var updated = VisitList(node.Body.Statements); // visit updated body (this will also handle with inner functions)
            return CreateFunctionFromBody(node, Rand.Next(0, 3) == 2 ? CreateIncrementalCFlow(updated).Statements : CreateStepCFlow(updated).Statements).NormalizeWhitespace(); // return rewritten body
        }

        public override SyntaxNode VisitAnonymousFunctionExpression(AnonymousFunctionExpressionSyntax node)
        {
            if (node.HasAnnotation(FlattenerSkipAnnotation))
                return node;

            var updated = VisitList(node.Body.Statements); // visit updated body (this will also handle with inner functions)
            return CreateFunctionFromBody(node, Rand.Next(0, 3) == 2 ? CreateIncrementalCFlow(updated).Statements : CreateStepCFlow(updated).Statements).NormalizeWhitespace(); // return rewritten body
        }
    }

    public static SyntaxNode CreateMainFlow(SyntaxNode node) => new CFlowRewriter().Visit(node);
}