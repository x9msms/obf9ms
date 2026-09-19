using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using static Loretta.CodeAnalysis.Lua.SyntaxFactory;

public class CFRewriter : LuaSyntaxRewriter
{
    private static readonly Random Random = new();

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
                                var name = assignment.Variables[0];
                                newStatements.Add(AssignmentStatement(SingletonSeparatedList(name), SingletonSeparatedList<ExpressionSyntax>(innerBinOp)).NormalizeWhitespace());
                                var reassignment = AssignmentStatement(SingletonSeparatedList(name), SingletonSeparatedList(binOp.Right));
                                //newStatements.Add(IfStatement(
                                //   BinaryExpression(SyntaxKind.EqualsExpression, name, Token(SyntaxKind.EqualsEqualsToken),
                                //    LiteralExpression(SyntaxKind.NilLiteralExpression))).NormalizeWhitespace());

                                goto skip;
                            }
                        }

                        break;
                    }
                case LocalVariableDeclarationStatementSyntax or LocalFunctionDeclarationStatementSyntax:
                    newStatements.Add(statement);
                    continue;
            }

            newStatements.Add(statement);

        skip:;
        }

        return StatementList(newStatements).NormalizeWhitespace();
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

        for (var i = 0; i < Random.Next(1, 5); i++)
            node = node.AddFields(UnkeyedTableField(LiteralExpression(SyntaxKind.NilLiteralExpression)));

        return node.NormalizeWhitespace();
    }

    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        if (Random.Next(0, 5) == Random.Next(0, 5))
            return base.VisitIfStatement(node);

        if (node.ElseClause is null || node.Body.Statements.Count != 1 || node.Body.Statements[0] is not ReturnStatementSyntax ret1 || node.ElseClause.ElseBody.Statements[0] is not ReturnStatementSyntax ret2)
            return base.VisitIfStatement(node);

        var condition = ParenthesizedExpression(node.Condition);
        var return1 = (ExpressionSyntax)(ret1.Expressions.Count == 1 ? ParenthesizedExpression(ret1.Expressions[0]) : LiteralExpression(SyntaxKind.FalseLiteralExpression));
        var return2 = (ExpressionSyntax)(ret2.Expressions.Count == 1 ? ParenthesizedExpression(ret2.Expressions[0]) : LiteralExpression(SyntaxKind.FalseLiteralExpression));

        return ReturnStatement(SingletonSeparatedList<ExpressionSyntax>(
            BinaryExpression(SyntaxKind.LogicalOrExpression,
                BinaryExpression(SyntaxKind.LogicalAndExpression, condition,
                    Token(SyntaxKind.AndKeyword), return1), Token(SyntaxKind.OrKeyword),
                return2)
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

    public override SyntaxNode VisitAssignmentStatement(AssignmentStatementSyntax node)
    {
        if (node.Variables.Count == 1 && node.Variables[0] is ElementAccessExpressionSyntax && node.EqualsValues.Values[0] is FunctionCallExpressionSyntax { Argument: ExpressionListFunctionArgumentSyntax })
            return node.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        return base.VisitAssignmentStatement(node).NormalizeWhitespace();
    }

    private static int _created;

    public override SyntaxNode VisitGenericForStatement(GenericForStatementSyntax node)
    {
        _created++;
        var updated = VisitList(node.Body.Statements);
        var body = StatementList(updated);
        if (Random.Next(3) == 1) return base.VisitGenericForStatement(node).NormalizeWhitespace(); // modify statement

        var i = node.Identifiers[0].Name;
        var v = node.Identifiers[1].Name;
        var newLoop = ParseStatement(@$"
                    do
                        local iter_{_created}, tbltoiter_{_created}, {i}, {v} = {node.Expressions};
                        while true do
                            {i}, {v} = iter_{_created}(tbltoiter_{_created}, {i})
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
        _created++;
        var updated = VisitList(node.Body.Statements);
        var body = StatementList(updated);

        if (Random.Next(0, 3) == RandomNumberGenerator.GetInt32(0, 3))
            return base.VisitNumericForStatement(node).NormalizeWhitespace(); // modify statement

        var i_name = node.Identifier;
        var i = node.InitialValue;
        var v = node.FinalValue;

        var newLoop = ParseStatement(@$"
                    do
                        local {i_name},_v_{_created} = {i}, {v};
                        while true do
                            {body}
                            if {i_name} >= _v_{_created} then break end;
                            {i_name} = {i_name} + 1;
                        end;
                    end;
                    ");
        return newLoop.NormalizeWhitespace();

        //return base.VisitGenericForStatement(node).NormalizeWhitespace();
    }

    public static SyntaxNode RewriteControlFlow(SyntaxNode node) => new CFRewriter().Visit(node);
}