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
public class TempAllocAnalyzer : DiagnosticAnalyzer
{
    private static int _ruleId;

    private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"TEMPALLOC{++_ruleId:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor DynSizedRule = Rule(
        DiagnosticSeverity.Error,
        "DynSized Attribute Required",
        "Missing `DynSized` attribute for a struct `{0}` marked with `Temp` attribute"
    );

    private static readonly DiagnosticDescriptor FieldRule = Rule(
        DiagnosticSeverity.Error,
        "Field of Non-Temp Structure",
        "Structure `{0}` can be a field only of a structure marked with the `Temp` attribute"
    );

    private static readonly DiagnosticDescriptor RedundantRule = Rule(
        DiagnosticSeverity.Warning,
        "Redundant Temp Attribute",
        "Structure `{0}` is marked with the `DynSized` attribute but doesn't have any `Temp` fields"
    );

    private static readonly DiagnosticDescriptor GenericTypeArgumentRule = Rule(
        DiagnosticSeverity.Error,
        "Generic Type Argument",
        "Passing the `Temp` type `{0}` as a type argument"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DynSizedRule,
        FieldRule,
        RedundantRule,
        GenericTypeArgumentRule
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var sym = (IFieldSymbol)ctx.Symbol;
        if (sym.Type.TypeKind != TypeKind.Struct || sym.ContainingType.TypeKind != TypeKind.Struct)
            return;

        if (sym.Type.IsTempAlloc() && !sym.ContainingType.IsTempAlloc())
            ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
        if (sym == null || !sym.IsTempAlloc())
            return;

        var hasTempFields = sym
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Type.IsTempAlloc());

        if (!hasTempFields)
            ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));

        if (!sym.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(DynSizedRule, sym.Locations.First(), sym.Name));
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
        if (i.Type is not INamedTypeSymbol { IsGenericType: true } n)
            return;

        if (n.IsExclusiveAccess() || (n.IsRefList() && !n.IsTempAlloc()))
        {
            var gt = n.TypeArguments.First();
            if (gt.IsTempAlloc())
                ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeArgumentRule, s.GetLocation(), gt.Name));
        }
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;

        if (d.Symbol.Type is not INamedTypeSymbol { IsGenericType: true } n)
            return;

        if (n.IsExclusiveAccess() || (n.IsRefList() && !n.IsTempAlloc()))
        {
            var gt = n.TypeArguments.First();
            var loc = d.GetDeclaredTypeLocation();
            if (gt.IsTempAlloc())
                ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeArgumentRule, loc, gt.Name));
        }
    }
}