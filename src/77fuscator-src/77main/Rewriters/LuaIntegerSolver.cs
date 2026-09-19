using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Experimental;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace geniussolution.Rewriters;

public class LuaIntegerSolver : LuaSyntaxRewriter
{
    public override SyntaxNode VisitTableConstructorExpression(TableConstructorExpressionSyntax node)
    {
        if (node.Kind() == SyntaxKind.HashToken)
        {
            var ctor = node.Fields;

            bool canOptimize = true;

            for (int i = 0; i < ctor.Count; i++)
            {
                var field = ctor[i];

                if (field != null)
                {
                    canOptimize = false;
                    break;
                }
            }
        }
        return base.VisitTableConstructorExpression(node).ConstantFold(ConstantFoldingOptions.All); ;
    }

    public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        if (node.Kind() == SyntaxKind.UnaryMinusExpression)
        {
            if (node.Kind() == SyntaxKind.NumericalLiteralExpression)
            {
                if (node.Token.Value is long num2)
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericalLiteralExpression, SyntaxFactory.Literal(-num2));
                }
            }
        }

        if (node.Kind() != SyntaxKind.NumericalLiteralExpression) return base.VisitLiteralExpression(node).ConstantFold(ConstantFoldingOptions.All);

        if (node.Token.Value is long num)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return SyntaxFactory.BinaryExpression(SyntaxKind.ExclusiveOrExpression, node, SyntaxFactory.Token(SyntaxKind.PipeToken), SyntaxFactory.LiteralExpression(SyntaxKind.NumericalLiteralExpression, SyntaxFactory.Literal(1)));
        }

        return base.VisitLiteralExpression(node);
    }
}