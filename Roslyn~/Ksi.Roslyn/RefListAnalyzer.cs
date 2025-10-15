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
        "Generic [RefList] type usage is unsafe",
        "Usage of the [RefList] collection in generic context is not supported"
    );

    private static readonly DiagnosticDescriptor Rule02JaggedRefList = Rule(02, DiagnosticSeverity.Error,
        "Jagged [RefList] types are not supported",
        "Jagged [RefList] types are not supported. " +
        "Consider to wrap inner collection with a structure"
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
        switch (AnalyzeGenericType(i.Type))
        {
            case RuleId.Rule01GenericItemType:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01GenericItemType, s.GetLocation()));
                break;

            case RuleId.Rule02JaggedRefList:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, s.GetLocation()));
                break;
        }
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (!d.IsVar())
            return;

        switch (AnalyzeGenericType(d.Symbol.Type))
        {
            case RuleId.Rule01GenericItemType:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01GenericItemType, d.GetDeclaredTypeLocation()));
                break;

            case RuleId.Rule02JaggedRefList:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02JaggedRefList, d.GetDeclaredTypeLocation()));
                break;
        }
    }

    private enum RuleId
    {
        None,
        Rule01GenericItemType,
        Rule02JaggedRefList,
    }

    private static RuleId AnalyzeGenericType(ITypeSymbol? s)
    {
        if (s is not INamedTypeSymbol { IsGenericType: true, TypeKind: TypeKind.Struct } t || !t.IsRefList())
            return RuleId.None;

        if (t.TypeArguments[0] is not INamedTypeSymbol gt)
            return RuleId.Rule01GenericItemType;

        if (gt.IsRefList())
            return RuleId.Rule02JaggedRefList;

        return RuleId.None;
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