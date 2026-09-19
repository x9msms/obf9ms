using geniussolution.Obfuscator;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;

namespace geniussolution.Rewriters;

internal class XorKeyRewriter : LuaSyntaxRewriter
{
    private readonly ObfuscationContext _context;
    private readonly ObfuscationSettings _settings;

    public XorKeyRewriter(ObfuscationContext context, ObfuscationSettings settings)
    {
        _context = context;
        _settings = settings;
    }

    public override SyntaxNode? VisitLocalVariableDeclarationStatement(LocalVariableDeclarationStatementSyntax node)
    {
        if (node.Names[0].Name is "XOR_KEY")
        {
            var key = NumberObfuscation.ObfuscateNumber(_context.PrimaryXorKey).NormalizeWhitespace();

            return SyntaxFactory.LocalVariableDeclarationStatement(
                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.LocalDeclarationName("XOR_KEY") }),
                SyntaxFactory.SeparatedList(new[] { (ExpressionSyntax)key })
            );
        }

        return base.VisitLocalVariableDeclarationStatement(node);
    }
}