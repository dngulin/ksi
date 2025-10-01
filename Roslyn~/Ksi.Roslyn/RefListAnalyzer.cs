using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RefListAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"REFLIST{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01GenericItemType = Rule(01, DiagnosticSeverity.Error,
        "Generic [RefList] type usage is unsafe",
        "Usage of the [RefList] collection in generic context is not supported"
    );

    private static readonly DiagnosticDescriptor Rule02JaggedRefList = Rule(02, DiagnosticSeverity.Error,
        "Jagged [RefList] types are not supported",
        "Jagged [RefList] types are not supported. " +
        "Consider to wrap inner collection with non-generic data structure"
    );

    private static readonly DiagnosticDescriptor Rule03NonSpecializedCall = Rule(03, DiagnosticSeverity.Error,
        "Non-specialized [RefList] API call",
        "Using non-specialized API for [RefList] of `{0}`. Consider to use the specialized method version"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01GenericItemType,
        Rule02JaggedRefList,
        Rule03NonSpecializedCall
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeVariableDeclaration, OperationKind.VariableDeclaration);
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
        context.RegisterOperationAction(AnalyzeExtensionInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeVariableDeclaration(OperationAnalysisContext ctx)
    {
        var declaration = (IVariableDeclarationOperation)ctx.Operation;
        var declarator = declaration.Declarators.First();

        if (declarator.Symbol.Type is not INamedTypeSymbol t)
            return;

        var isRefList = t.IsGenericType && t.IsRefList();
        if (!isRefList)
            return;

        var loc = declaration.Syntax.GetLocation();

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01GenericItemType, loc));
            return;
        }

        if (gt.IsGenericType && gt.IsRefList())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, loc));
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var f = (IFieldSymbol)ctx.Symbol;
        if (f.IsStatic || f.Type.TypeKind != TypeKind.Struct || f.Type is not INamedTypeSymbol t)
            return;

        var loc = f.DeclaringSyntaxReferences.First()
                      .GetSyntax(ctx.CancellationToken).Parent?.ChildNodes().First().GetLocation()
                  ?? f.Locations.First();

        AnalyzeSymbolTypeAppearance(ctx, t, loc);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext ctx)
    {
        var p = (IParameterSymbol)ctx.Symbol;
        if (p.Type.TypeKind != TypeKind.Struct || p.Type is not INamedTypeSymbol t)
            return;

        var loc = p.DeclaringSyntaxReferences
            .First().GetSyntax(ctx.CancellationToken).ChildNodes()
            .First().GetLocation();

        AnalyzeSymbolTypeAppearance(ctx, t, loc);
    }

    private static void AnalyzeSymbolTypeAppearance(SymbolAnalysisContext ctx, INamedTypeSymbol t, Location loc)
    {
        var isRefList = t.IsGenericType && t.IsRefList();
        if (!isRefList)
            return;

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01GenericItemType, loc));
            return;
        }

        if (gt.IsGenericType && gt.IsRefList())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, loc));
    }

    private static void AnalyzeExtensionInvocation(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;
        if (i.IsVirtual)
            return;

        var m = i.TargetMethod;
        if (m.IsAsync || !m.IsExtensionMethod || !m.IsGenericMethod || m.TypeArguments.Length != 1)
            return;

        var p = m.Parameters;
        if (p.Length == 0 || p[0].Type is not INamedTypeSymbol t)
            return;

        var isRefList = t.IsGenericType && t.IsRefList();
        if (!isRefList)
            return;

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
            return;

        var loc = i.Syntax.GetLocation();

        if (gt.IsDealloc())
        {
            ReportCalls(ctx, m, loc, ExplicitCopyTemplates.RefListExtensionNames);
            ReportCalls(ctx, m, loc, DeallocTemplates.RefListExtensionNames);
        }
        else if (gt.IsExplicitCopy())
        {
            ReportCalls(ctx, m, loc, ExplicitCopyTemplates.RefListExtensionNames);
        }
    }

    private static void ReportCalls(OperationAnalysisContext ctx, IMethodSymbol m, Location loc, string[] names)
    {
        if (names.Contains(m.Name))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule03NonSpecializedCall, loc));
    }
}