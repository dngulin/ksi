using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KsiGenericAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"KSIGENERIC{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01GenericMethodArgumentTraits = Rule(01, DiagnosticSeverity.Error,
        "Argument type traits are not compatible with generic parameter type traits",
        "Argument type `{0}` traits are not compatible with generic parameter type {1} traits"
    );

    private static readonly DiagnosticDescriptor Rule02JaggedRefList = Rule(02, DiagnosticSeverity.Error,
        "Jagged TRefList<T> types are not supported",
        "Jagged TRefList<T> types are not supported. " +
        "Consider to wrap inner collection with a structure"
    );

    private static readonly DiagnosticDescriptor Rule03GenericTypeArgumentTraits = Rule(03, DiagnosticSeverity.Error,
        "Generic type argument traits are not compatible with generic type parameter traits",
        "Generic type argument `{0}` traits are not compatible with generic type parameter `{1}` traits"
    );


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01GenericMethodArgumentTraits,
        Rule02JaggedRefList,
        Rule03GenericTypeArgumentTraits
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeGenericTypeSyntax, SyntaxKind.GenericName);
        context.RegisterSyntaxNodeAction(AnalyzeArrayTypeSyntax, SyntaxKind.ArrayType);
        context.RegisterSyntaxNodeAction(AnalyzeTupleTypeSyntax, SyntaxKind.TupleType);
        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
        context.RegisterOperationAction(AnalyzeTuple, OperationKind.Tuple);

    }

    private static void AnalyzeGenericTypeSyntax(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s.GetTypeExpr(), ctx.CancellationToken);
        if (IsJaggedRefList(i.Type))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, s.GetLocation()));

        if (i.Type is INamedTypeSymbol { IsGenericType: true } t && !t.IsSpanOrReadonlySpan())
            AnalyzeGenericTypeArgTraits(ctx, t, s.TypeArgumentList.Arguments.Select(x => x.GetLocation()).ToImmutableArray());
    }

    private static void AnalyzeArrayTypeSyntax(SyntaxNodeAnalysisContext ctx)
    {
        var s = (ArrayTypeSyntax)ctx.Node;

        var i = ctx.SemanticModel.GetTypeInfo(s.ElementType, ctx.CancellationToken);
        if (i.Type is not {} t)
            return;

        if (t.IsExplicitCopy())
            ctx.Report(s.GetLocation(), Rule03GenericTypeArgumentTraits, t.Name, "TItem");
    }

    private static void AnalyzeTupleTypeSyntax(SyntaxNodeAnalysisContext ctx)
    {
        var s = (TupleTypeSyntax)ctx.Node;

        var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
        if (i.Type is INamedTypeSymbol { IsGenericType: true } t)
            AnalyzeGenericTypeArgTraits(ctx, t, s.Elements.Select(e => e.Type.GetLocation()).ToImmutableArray());
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (!d.IsVar())
            return;

        var loc = d.GetDeclaredTypeLocation();

        if (IsJaggedRefList(d.Symbol.Type))
            ctx.Report(loc, Rule02JaggedRefList);

        switch (d.Symbol.Type)
        {
            case IArrayTypeSymbol a when a.ElementType.IsExplicitCopy():
                ctx.Report(loc, Rule03GenericTypeArgumentTraits, a.ElementType.Name, "TItem");
                break;
            case INamedTypeSymbol { IsGenericType: true } t when !t.IsSpanOrReadonlySpan():
                AnalyzeGenericTypeArgTraits(ctx, t, loc);
                break;
        }
    }

    private static bool IsJaggedRefList(ITypeSymbol? s)
    {
        if (s is not INamedTypeSymbol { IsGenericType: true, TypeKind: TypeKind.Struct } t || !t.IsRefList())
            return false;

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
            return false;

        return gt.IsRefList();
    }

    private static void AnalyzeGenericTypeArgTraits(OperationAnalysisContext ctx, INamedTypeSymbol t, Location loc)
    {
        for (var i = 0; i < t.TypeArguments.Length; i++)
        {
            var at = t.TypeArguments[i];
            var pt = t.TypeParameters[i];
            if (!CheckTraits(at, pt))
                ctx.Report(loc, Rule03GenericTypeArgumentTraits, at.Name, pt.Name);
        }
    }

    private static void AnalyzeGenericTypeArgTraits(SyntaxNodeAnalysisContext ctx, INamedTypeSymbol t, ImmutableArray<Location> locs)
    {
        for (var i = 0; i < t.TypeArguments.Length; i++)
        {
            var at = t.TypeArguments[i];
            var pt = t.TypeParameters[i];
            if (!CheckTraits(at, pt))
                ctx.Report(locs[i], Rule03GenericTypeArgumentTraits, at.Name, pt.Name);
        }
    }

    private static void AnalyzeArgument(OperationAnalysisContext ctx)
    {
        var a = (IArgumentOperation)ctx.Operation;
        foreach (var (t, p) in a.GetGenericTypeSubstitutions())
        {
            if (CheckTraits(t, p))
                continue;

            ctx.Report(a.Syntax.GetLocation(), Rule01GenericMethodArgumentTraits, t.Name, p.Name);
        }
    }

    private static bool CheckExpCopy(ITypeSymbol t, ITypeParameterSymbol p) => !t.IsExplicitCopy() || p.IsExplicitCopy();
    private static bool CheckDealloc(ITypeSymbol t, ITypeParameterSymbol p) => !t.IsDealloc() || p.IsDealloc();
    private static bool CheckTempAlloc(ITypeSymbol t, ITypeParameterSymbol p) => !t.IsTempAlloc() || p.IsTempAlloc();

    private static bool CheckTraits(ITypeSymbol t, ITypeParameterSymbol p)
    {
        return CheckExpCopy(t, p) && CheckDealloc(t, p) && CheckTempAlloc(t, p);
    }

    private static void AnalyzeTuple(OperationAnalysisContext ctx)
    {
        var tuple = (ITupleOperation)ctx.Operation;
        for (var i = 0; i < tuple.Elements.Length; i++)
        {
            var e = tuple.Elements[i];
            if (e.Type == null)
                continue;

            if (e.Type.IsExplicitCopy())
                ctx.Report(e.Syntax.GetLocation(), Rule03GenericTypeArgumentTraits, e.Type.Name, $"T{i+1}");
        }
    }
}