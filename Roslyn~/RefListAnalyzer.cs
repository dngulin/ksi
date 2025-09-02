using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RefListAnalyzer  : DiagnosticAnalyzer
{
    private static int _ruleId;

    private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"REFLIST{++_ruleId:D2}",
            title: title,
            messageFormat: msg,
            category: "RefList",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor UnknownItemTypeRule = Rule(
        DiagnosticSeverity.Error,
        "Unknown Item Type",
        "RefList API is unsafe for unknown item types"
    );

    private static readonly DiagnosticDescriptor GenericItemTypeRule = Rule(
        DiagnosticSeverity.Error,
        "Generic Item",
        "RefList API is unsafe for generic item types"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        GenericItemTypeRule,
        UnknownItemTypeRule
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeOperation, OperationKind.VariableDeclaration);
    }

    private static void AnalyzeOperation(OperationAnalysisContext ctx)
    {
        var declaration = (IVariableDeclarationOperation)ctx.Operation;
        var declarator = declaration.Declarators.First();

        if (declarator.Symbol.Type is not INamedTypeSymbol t)
            return;

        var isRefList = t.IsGenericType && t.IsRefListType();
        if (!isRefList)
            return;

        var loc = declaration.Syntax.GetLocation();

        if (t.TypeArguments[0] is not INamedTypeSymbol gt || gt.IsGenericType)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(UnknownItemTypeRule, loc));
            return;
        }

        if (gt.IsGenericType)
            ctx.ReportDiagnostic(Diagnostic.Create(GenericItemTypeRule, loc));
    }
}