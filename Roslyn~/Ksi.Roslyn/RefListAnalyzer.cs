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
        "Generic TRefList<T> item type usage is unsafe",
        "Usage of the TRefList<T> with generic item types is not supported"
    );

    private static readonly DiagnosticDescriptor Rule02JaggedRefList = Rule(02, DiagnosticSeverity.Error,
        "Jagged TRefList<T> types are not supported",
        "Jagged TRefList<T> types are not supported. " +
        "Consider to wrap inner collection with a structure"
    );

    private static readonly DiagnosticDescriptor Rule03NonSpecializedCall = Rule(03, DiagnosticSeverity.Error,
        "Non-specialized TRefList API call",
        "Using non-specialized API for TRefList<{0}>. Consider to use the specialized method version"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule02JaggedRefList,
        Rule03NonSpecializedCall
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeExtensionInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s.GetTypeExpr(), ctx.CancellationToken);
        if (IsJaggedRefList(i.Type))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, s.GetLocation()));
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (!d.IsVar())
            return;

        if (IsJaggedRefList(d.Symbol.Type))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, d.GetDeclaredTypeLocation()));
    }

    private static bool IsJaggedRefList(ITypeSymbol? s)
    {
        if (s is not INamedTypeSymbol { IsGenericType: true, TypeKind: TypeKind.Struct } t || !t.IsRefList())
            return false;

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
            return false;

        return gt.IsRefList();
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

        if (!t.IsRefList())
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