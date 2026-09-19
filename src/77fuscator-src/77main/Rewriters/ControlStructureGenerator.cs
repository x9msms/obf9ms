using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

public class ControlStructureGenerator : LuaSyntaxRewriter
{
    private static readonly Random Random = new();
    private int _created;

    public override SyntaxNode VisitStatementList(StatementListSyntax node)
    {
        var statements = VisitList(node.Statements);
        var newStatements = new List<StatementSyntax>();

        for (var index = 0; index < statements.Count; index++)
        {
            var statement = statements[index];

            if (index == statements.Count - 1)
            {
                newStatements.Add(statement);
                continue;
            }

            switch (statement)
            {
                case AssignmentStatementSyntax { Variables.Count: 1 } assignment when assignment.EqualsValues.Values.Count is 1:
                    {
                        if (assignment.EqualsValues.Values[0] is BinaryExpressionSyntax { Left: BinaryExpressionSyntax innerBinOp } binOp)
                        {
                            if (innerBinOp.Kind() == SyntaxKind.LogicalAndExpression && binOp.Kind() == SyntaxKind.LogicalOrExpression)
                            {
                                if (Random.Next(1, 4) != 3)
                                {
                                    newStatements.Add(statement.NormalizeWhitespace());
                                    goto skip;
                                }

                                var name = assignment.Variables[0];
                                newStatements.Add(AssignmentStatement(SingletonSeparatedList(name), SingletonSeparatedList<ExpressionSyntax>(innerBinOp)).NormalizeWhitespace());
                                var reassignment = AssignmentStatement(SingletonSeparatedList(name), SingletonSeparatedList(binOp.Right));
                                newStatements.Add(IfStatement(
                                    BinaryExpression(SyntaxKind.EqualsExpression, name, Token(SyntaxFacts.GetOperatorTokenKind(SyntaxKind.EqualsEqualsToken).Value),
                                        LiteralExpression(SyntaxKind.NilLiteralExpression)), StatementList(reassignment), new SyntaxList<ElseIfClauseSyntax>(), null
                                ).NormalizeWhitespace());

                                goto skip;
                            }
                        }

                        break;
                    }
                case LocalVariableDeclarationStatementSyntax or LocalFunctionDeclarationStatementSyntax:
                    newStatements.Add(statement.NormalizeWhitespace());
                    continue;
            }

            if (Random.Next(0, 8) < 6 || statement is BreakStatementSyntax or ContinueStatementSyntax)
            {
                newStatements.Add(statement.NormalizeWhitespace());
            }
            else
            {
                if (statements[index + 1] is LocalVariableDeclarationStatementSyntax
                    or LocalFunctionDeclarationStatementSyntax)
                {
                    switch (Random.Next(0, 2))
                    {
                        case 0:
                            {
                                newStatements.Add(DoStatement(StatementList(statement)).NormalizeWhitespace());
                                break;
                            }
                        case 1:
                            {
                                newStatements.Add(RepeatUntilStatement(StatementList(statement), LiteralExpression(SyntaxKind.TrueLiteralExpression)).NormalizeWhitespace());
                                break;
                            }
                    }
                }
                else
                {
                    if (Random.Next(0, 4) == 3)
                    {
                        switch (Random.Next(0, 2))
                        {
                            case 0:
                                {
                                    newStatements.Add(DoStatement(StatementList(statement, statements[index + 1])).NormalizeWhitespace());
                                    break;
                                }
                            case 1:
                                {
                                    newStatements.Add(RepeatUntilStatement(StatementList(statement, statements[index + 1]), LiteralExpression(SyntaxKind.TrueLiteralExpression)).NormalizeWhitespace());
                                    break;
                                }
                        }

                        index++;
                    }
                    else
                    {
                        switch (Random.Next(0, 2))
                        {
                            case 0:
                                {
                                    newStatements.Add(DoStatement(StatementList(statement)).NormalizeWhitespace());
                                    break;
                                }
                            case 1:
                                {
                                    newStatements.Add(RepeatUntilStatement(StatementList(statement), LiteralExpression(SyntaxKind.TrueLiteralExpression)).NormalizeWhitespace());
                                    break;
                                }
                        }
                    }
                }
            }

        skip:;
        }

        return StatementList(newStatements);
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

        return node.NormalizeWhitespace();
    }

    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {
        if (node.Parameters.Count == 1 && node.Parameters[0] is VarArgParameterSyntax)
            return base.VisitParameterList(node);

        for (var i = 0; i < Random.Next(1, 5); i++)
            node = node.AddParameters(NamedParameter("fake_" + i));

        return node.NormalizeWhitespace();
    }

    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        if (node.ElseClause is null || node.Body.Statements.Count != 1 || node.Body.Statements[0] is not ReturnStatementSyntax ret1 || node.ElseClause.ElseBody.Statements[0] is not ReturnStatementSyntax ret2)
            return base.VisitIfStatement(node).NormalizeWhitespace();

        var condition = ParenthesizedExpression(node.Condition).NormalizeWhitespace();
        var return1 = (ExpressionSyntax)(ret1.Expressions.Count == 1 ? ParenthesizedExpression(ret1.Expressions[0]).NormalizeWhitespace() : LiteralExpression(SyntaxKind.FalseLiteralExpression));
        var return2 = (ExpressionSyntax)(ret2.Expressions.Count == 1 ? ParenthesizedExpression(ret2.Expressions[0]).NormalizeWhitespace() : LiteralExpression(SyntaxKind.FalseLiteralExpression));

        return ReturnStatement(SingletonSeparatedList<ExpressionSyntax>(
            BinaryExpression(SyntaxKind.LogicalOrExpression,
                BinaryExpression(SyntaxKind.LogicalAndExpression, condition.NormalizeWhitespace(),
                    Token(SyntaxKind.AndKeyword), return1.NormalizeWhitespace()), Token(SyntaxKind.OrKeyword),
                return2.NormalizeWhitespace())
        )).NormalizeWhitespace();
    }

    public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var left = (ExpressionSyntax)Visit(node.Left);
        var right = (ExpressionSyntax)Visit(node.Right);

        var operatorToken = node.OperatorToken;

        var gr = Random.Next(2) == 0;

        ExpressionSyntax start;
        ExpressionSyntax end;

        if (Random.Next(2) == 1)
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
        return Random.Next(2) == 0 ? node : ParenthesizedExpression(node).NormalizeWhitespace(); // avoid specific regex
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

    public override SyntaxNode VisitNumericForStatement(NumericForStatementSyntax node)
    {
        var updated = VisitList(node.Body.Statements);
        var body = StatementList(updated);

        if (Random.Next(5) != 3)
            return base.VisitNumericForStatement(node);

        _created++;
        var i = node.Identifier.Name;

        var statements = new List<StatementSyntax>();

        var step = LocalVariableDeclarationStatement(
            SingletonSeparatedList(LocalDeclarationName($"numeric_for_step_{_created}")),
            node.StepValue is not null
                ? SingletonSeparatedList(node.StepValue)
                : SingletonSeparatedList<ExpressionSyntax>(LiteralExpression(SyntaxKind.NumericalLiteralExpression, Literal(1))));

        statements.Add(step);

        var index = LocalVariableDeclarationStatement(
            SingletonSeparatedList(LocalDeclarationName(i)),
            SingletonSeparatedList(node.InitialValue)
        );

        statements.Add(index);

        var finalValue = LocalVariableDeclarationStatement(
            SingletonSeparatedList(LocalDeclarationName($"numeric_for_finalValue_{_created}")),
            SingletonSeparatedList(node.FinalValue)
        );

        statements.Add(finalValue);

        switch (Random.Next(1))
        {
            case 0:
                {
                    var whileStatements = new List<StatementSyntax>();
                    var condition = LiteralExpression(SyntaxKind.TrueLiteralExpression);

                    whileStatements.AddRange(body.Statements);

                    whileStatements.Add(IfStatement(
                        BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(index.Names.First().Name), Token(SyntaxKind.EqualsEqualsToken), IdentifierName(finalValue.Names.First().Name)),
                        StatementList(new List<StatementSyntax> { BreakStatement() }), new SyntaxList<ElseIfClauseSyntax>(), null
                    ).NormalizeWhitespace());

                    whileStatements.Add(AssignmentStatement(
                        SingletonSeparatedList<PrefixExpressionSyntax>(IdentifierName(i)),
                        SingletonSeparatedList<ExpressionSyntax>(BinaryExpression(SyntaxKind.AddExpression, IdentifierName(index.Names.First().Name), Token(SyntaxKind.PlusToken), IdentifierName($"numeric_for_step_{_created}")))
                    ).NormalizeWhitespace());

                    statements.Add(WhileStatement(condition, StatementList(whileStatements)).NormalizeWhitespace());

                    break;
                }
            case 1:
                {
                    break;
                }
        }

        return DoStatement(StatementList(statements)).NormalizeWhitespace();
    }

    public override SyntaxNode VisitAssignmentStatement(AssignmentStatementSyntax node)
    {
        //if (node.Variables.Count == 1 && node.Variables[0] is ElementAccessExpressionSyntax && node.EqualsValues.Values[0] is FunctionCallExpressionSyntax { Argument: ExpressionListFunctionArgumentSyntax })
        //    return node.WithSemicolonToken(Token(SyntaxKind.SemicolonToken)).NormalizeWhitespace();

        if (Random.Next(0, 5) != Random.Next(0, 5))
            return base.VisitAssignmentStatement(node).NormalizeWhitespace();

        if (node.EqualsValues.Values.Any(statement => statement.IsKind(SyntaxKind.NilLiteralExpression) || statement is IdentifierNameSyntax { Name: "_nil" }))
        {
            return base.VisitAssignmentStatement(node).NormalizeWhitespace();
        }

        var expressions = node.EqualsValues.Values;

        var tableFields = SyntaxFactory.SeparatedList<TableFieldSyntax>();
        tableFields = expressions.Aggregate(tableFields, (current, expression) => current.Add(SyntaxFactory.UnkeyedTableField(expression)));

        var functionCall = SyntaxFactory.FunctionCallExpression(SyntaxFactory.IdentifierName("Unpack"), SyntaxFactory.TableConstructorFunctionArgument(SyntaxFactory.TableConstructorExpression(tableFields))).NormalizeWhitespace();

        var assignment = SyntaxFactory.AssignmentStatement(node.Variables, SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(functionCall)).NormalizeWhitespace(); ;

        return assignment.NormalizeWhitespace();
    }

    public static SyntaxNode RewriteControlFlow(SyntaxNode node) => new ControlStructureGenerator().Visit(node);
}