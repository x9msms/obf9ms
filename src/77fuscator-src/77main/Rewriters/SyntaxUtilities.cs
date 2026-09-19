using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Syntax;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LorettaTest;

public static class SyntaxUtilities
{
    public static SyntaxTree ToSyntaxTree(this string source)
    {
        var syntaxTree = LuaSyntaxTree.ParseText(source, LuaParseOptions.Default);

        var diagnostics = syntaxTree.GetDiagnostics();

        if (diagnostics.FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) is
            { } syntaxError)
            throw new Exception($"[{syntaxError.Location}] {syntaxError.GetMessage()}");

        return syntaxTree;
    }

    private static readonly ConditionalWeakTable<string, SyntaxNode> SyntaxNodeCache = new();
    private static int Size = 0;

    public enum CacheKind
    {
        ExpressionKind,
        StatementKind,
        StatementListKind
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static SyntaxNode GetCachedNodeFor(string toParse, CacheKind cacheKind)
    {
        if (SyntaxNodeCache.TryGetValue(toParse, out var cachedParsed))
            return cachedParsed;

        SyntaxNode parsed = cacheKind switch
        {
            CacheKind.ExpressionKind => SyntaxFactory.ParseExpression(toParse),
            CacheKind.StatementKind => SyntaxFactory.ParseStatement(toParse),
            CacheKind.StatementListKind => SyntaxFactory.ParseCompilationUnit(toParse).Statements,
        };

        if (parsed.GetDiagnostics().FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) is { })
            throw new Exception($"Syntax Error On: {toParse} ");

        SyntaxNodeCache.Add(toParse, parsed);

        Size++;

        return parsed;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static StatementSyntax ParseStatement(string toParse)
    {
        if (SyntaxNodeCache.TryGetValue(toParse, out var cachedParsed))
            return (StatementSyntax)cachedParsed;

        var parsed = SyntaxFactory.ParseStatement(toParse);

        if (parsed.GetDiagnostics().FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) is { })
            throw new Exception($"Syntax Error On: {toParse} ");

        SyntaxNodeCache.Add(toParse, parsed);

        Size++;

        return parsed;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static ExpressionSyntax ParseExpression(string toParse)
    {
        if (SyntaxNodeCache.TryGetValue(toParse, out var cachedParsed))
            return (ExpressionSyntax)cachedParsed;

        var parsed = SyntaxFactory.ParseExpression(toParse);

        if (parsed.GetDiagnostics().FirstOrDefault(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) is { })
            // throw exception

            throw new Exception($"Syntax Error On: {toParse} ");

        SyntaxNodeCache.Add(toParse, parsed);

        Size++;

        return parsed;
    }
}