using System.Collections.Immutable;
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

    private static readonly DiagnosticDescriptor Rule01IncompatibleItemTypeTraits = Rule(01, DiagnosticSeverity.Error,
        "Passing TRefList<T> argument with incompatible item type traits",
        "Passing TRefList<T> argument with incompatible item type traits"
    );

    private static readonly DiagnosticDescriptor Rule02JaggedRefList = Rule(02, DiagnosticSeverity.Error,
        "Jagged TRefList<T> types are not supported",
        "Jagged TRefList<T> types are not supported. " +
        "Consider to wrap inner collection with a structure"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01IncompatibleItemTypeTraits,
        Rule02JaggedRefList
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
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

    private static void AnalyzeArgument(OperationAnalysisContext ctx)
    {
        var a = (IArgumentOperation)ctx.Operation;
        var p = a.Parameter;

        if (a.Value.Type is not INamedTypeSymbol at || !at.IsSupportedGenericType())
            return;

        if (p?.OriginalDefinition.Type is not INamedTypeSymbol pt || !pt.IsSupportedGenericType())
            return;

        var gat = at.TypeArguments[0];

        if (pt.TypeArguments[0] is not ITypeParameterSymbol gpt)
            return;

        if (!CheckExpCopy(gat, gpt) || !CheckDealloc(gat, gpt) || !CheckTempAlloc(gat, gpt))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01IncompatibleItemTypeTraits, a.Syntax.GetLocation()));
    }

    private static bool CheckExpCopy(ITypeSymbol a, ITypeParameterSymbol p) => !a.IsExplicitCopy() || p.IsExplicitCopy();
    private static bool CheckDealloc(ITypeSymbol a, ITypeParameterSymbol p) => !a.IsDealloc() || p.IsDealloc();
    private static bool CheckTempAlloc(ITypeSymbol a, ITypeParameterSymbol p) => !a.IsTempAlloc() || p.IsTempAlloc();
}