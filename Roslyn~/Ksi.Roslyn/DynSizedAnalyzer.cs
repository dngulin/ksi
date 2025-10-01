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
public class DynSizedAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"DYNSIZED{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01MissingAttribute = Rule(01, DiagnosticSeverity.Error,
        "Field of Non-DynSized Structure",
        "Structure `{0}` can be a field only of a structure marked with the `DynSized` attribute"
    );

    private static readonly DiagnosticDescriptor Rule02ExplicitCopyRequired = Rule(02, DiagnosticSeverity.Error,
        "ExplicitCopy Attribute Required",
        "Missing `ExplicitCopy` attribute for a struct `{0}` marked with `DynSized` attribute"
    );

    private static readonly DiagnosticDescriptor Rule03RedundantAttribute = Rule(03, DiagnosticSeverity.Warning,
        "Redundant DynSized Attribute",
        "Structure `{0}` is marked with the `DynSized` attribute but doesn't have any `DynSized` fields"
    );

    private static readonly DiagnosticDescriptor Rule03NoResize = Rule(04, DiagnosticSeverity.Error,
        "DynNoResize Violation",
        "Passing as an argument a mutable reference to `{0}` that is derived from the `DynNoResize` parameter. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor Rule04RedundantNoResize = Rule(05, DiagnosticSeverity.Warning,
        "Redundant DynNoResize Annotation",
        "DynNoResize attribute is added to non-compatible parameter and has no effect."
    );

    private static readonly DiagnosticDescriptor Rule05FieldOfReferenceType = Rule(06, DiagnosticSeverity.Error,
        "Field Of Reference Type",
        "Type `{0}` cannot be a field of a reference type. Consider to wrap it with `ExclusiveAccess<{0}>`"
    );

    private static readonly DiagnosticDescriptor Rule06RedundantExclusiveAccess = Rule(07, DiagnosticSeverity.Warning,
        "Redundant ExclusiveAccess<T> Usage",
        "Usage of the `ExclusiveAccess<{0}>` is redundant because the generic argument `{0}` is not `[DynSized]`"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01MissingAttribute,
        Rule02ExplicitCopyRequired,
        Rule03RedundantAttribute,
        Rule03NoResize,
        Rule04RedundantNoResize,
        Rule05FieldOfReferenceType,
        Rule06RedundantExclusiveAccess
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeDynNoResizeArgs, OperationKind.Invocation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var sym = (IFieldSymbol)ctx.Symbol;
        if (!sym.Type.IsDynSized())
            return;

        if (sym.ContainingType.TypeKind == TypeKind.Struct && !sym.ContainingType.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01MissingAttribute, sym.Locations.First(), sym.Type.Name));

        if (sym.ContainingType.TypeKind == TypeKind.Class)
        {
            var loc = sym.GetDeclaredTypeLocation(ctx.CancellationToken);
            ctx.ReportDiagnostic(Diagnostic.Create(Rule05FieldOfReferenceType, loc, sym.Type.Name));
        }
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
        if (sym == null || !sym.IsDynSized())
            return;

        var hasDynSizedFields = sym
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Type.IsDynSized());

        if (!hasDynSizedFields)
            ctx.ReportDiagnostic(Diagnostic.Create(Rule03RedundantAttribute, sym.Locations.First(), sym.Name));

        if (!sym.IsExplicitCopy())
            ctx.ReportDiagnostic(Diagnostic.Create(Rule02ExplicitCopyRequired, sym.Locations.First(), sym.Name));
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (IsRedundantAccessScope(d.Symbol.Type, out var gtName))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule06RedundantExclusiveAccess, d.GetDeclaredTypeLocation(), gtName));
    }

    private static bool IsRedundantAccessScope(ITypeSymbol t, out string genericTypeName)
    {
        genericTypeName = "";
        if (t is not INamedTypeSymbol nt)
            return false;

        if (nt.IsExclusiveAccess() && nt.TypeArguments.First() is INamedTypeSymbol gt && !gt.IsDynSized())
        {
            genericTypeName = gt.Name;
            return true;
        }

        return false;
    }

    private static void AnalyzeDynNoResizeArgs(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;

        var resizableDynArgs = i.Arguments
            .Where(a =>
            {
                var p = a.Parameter;
                if (p == null || !p.IsMut())
                    return false;

                return p.Type.IsDynSizedOrWrapsDynSized() && !p.IsDynNoResize();
            })
            .Select(a => (RefPath: a.Value.ToRefPath(), Arg: a.Syntax.GetLocation()))
            .Where(x => !x.RefPath.IsEmpty)
            .ToImmutableArray();

        if (resizableDynArgs.IsEmpty)
            return;

        var m = i.GetEnclosingMethod(ctx.CancellationToken);
        if (m == null)
            return;

        var noResizeParams = m.Parameters
            .Where(p => p.IsMut() && p.Type.IsDynSizedOrWrapsDynSized() && p.IsDynNoResize())
            .Select(p => p.Name)
            .ToImmutableArray();

        if (noResizeParams.IsEmpty)
            return;

        foreach (var (refPath, location) in resizableDynArgs)
        {
            var root = refPath.Segments[0];
            if (noResizeParams.Contains(root))
                ctx.ReportDiagnostic(Diagnostic.Create(Rule03NoResize, location, refPath));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        var m = (IMethodSymbol)ctx.Symbol;

        foreach (var p in m.Parameters)
        {
            if (!p.IsDynNoResize())
                continue;

            if (!p.IsMut() || !p.Type.IsDynSizedOrWrapsDynSized())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule04RedundantNoResize, p.Locations.First()));
        }
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
        if (i.Type != null && IsRedundantAccessScope(i.Type, out var gtName))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule06RedundantExclusiveAccess, s.GetLocation(), gtName));
    }
}