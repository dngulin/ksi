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
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"TEMPALLOC{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01MissingAttribute = Rule(01, DiagnosticSeverity.Error,
        "Missing [TempAlloc] attribute",
        "Structure should be annotated with the [TempAlloc] attribute " +
        "because it contains a [TempAlloc] field of type `{0}`"
    );

    private static readonly DiagnosticDescriptor Rule02MissingDynSized = Rule(02, DiagnosticSeverity.Error,
        "Missing [DynSized] attribute",
        "Structure marked with the [TempAlloc] attribute should be also marked with the [DynSized] attribute"
    );

    private static readonly DiagnosticDescriptor Rule03RedundantAttribute = Rule(03, DiagnosticSeverity.Warning,
        "Redundant [TempAlloc] attribute",
        "Structure is marked with the [TempAlloc] attribute but doesn't have any [TempAlloc] fields"
    );

    private static readonly DiagnosticDescriptor Rule04IncompatibleAllocator = Rule(04, DiagnosticSeverity.Error,
        "Incompatible allocator with the [TempAlloc] type",
        "[TempAlloc] type `{0}` can be owned only by a [TempAlloc] collection"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01MissingAttribute,
        Rule02MissingDynSized,
        Rule03RedundantAttribute,
        Rule04IncompatibleAllocator
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
        var t = sym.Type;
        var ct = sym.ContainingType;

        if (t.TypeKind != TypeKind.Struct || ct.TypeKind != TypeKind.Struct)
            return;

        if (t.IsTempAlloc() && !ct.IsTempAlloc())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01MissingAttribute, ct.Locations.First(), sym.Type.Name));
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
        if (sym == null || !sym.IsTempAlloc())
            return;

        if (!sym.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02MissingDynSized, sym.Locations.First()));

        var hasTempFields = sym
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Type.IsTempAlloc());

        if (!hasTempFields)
            ctx.ReportDiagnostic(Diagnostic.Create(Rule03RedundantAttribute, sym.Locations.First()));
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
        if (i.Type == null || !IsInvalidTempAllocContainer(i.Type, out var gt))
            return;

        ctx.ReportDiagnostic(Diagnostic.Create(Rule04IncompatibleAllocator, s.GetLocation(), gt!.Name));
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;

        if (!d.IsVar())
            return;

        if (!IsInvalidTempAllocContainer(d.Symbol.Type, out var gt))
            return;

        var loc = d.GetDeclaredTypeLocation();
        ctx.ReportDiagnostic(Diagnostic.Create(Rule04IncompatibleAllocator, loc, gt!.Name));
    }

    private static bool IsInvalidTempAllocContainer(ITypeSymbol t, out ITypeSymbol? gt)
    {
        gt = null;

        if (t is not INamedTypeSymbol { IsGenericType: true } nt)
            return false;

        if (!nt.IsSupportedGenericType())
            return false;

        gt = nt.TypeArguments.First();
        if (!gt.IsTempAlloc())
            return false;

        return nt.IsRefList() && !nt.IsTempAlloc();
    }
}