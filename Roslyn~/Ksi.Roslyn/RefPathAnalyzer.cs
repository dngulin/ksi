using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RefPathAnalyzer: DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"REFPATH{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01InvalidMethodSignature = Rule(01,  DiagnosticSeverity.Error,
        "Invalid [RefPath] method signature",
        "Method indicated with the [RefPath] attribute has incompatible signature"
    );

    private static readonly DiagnosticDescriptor Rule02InvalidDeclaration = Rule(02, DiagnosticSeverity.Error,
        "Invalid [RefPath] attribute arguments",
        "RefPath attribute constructed with invalid segments sequence"
    );

    private static readonly DiagnosticDescriptor Rule03InvalidDeclaredRoot = Rule(03, DiagnosticSeverity.Error,
        "Invalid declared [RefPath] root",
        "Declared [RefPath] is not derived from `this` parameter"
    );

    private static readonly DiagnosticDescriptor Rule04InvalidReturnExpr = Rule(04, DiagnosticSeverity.Error,
        "Invalid [RefPath] return expression",
        "Return operation is not a RefPath-compatible expression"
    );

    private static readonly DiagnosticDescriptor Rule05MismatchPaths = Rule(05, DiagnosticSeverity.Error,
        "Reference path mismatch",
        "Returning reference path `{0}` doesn't match the declared reference path `{1}`"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01InvalidMethodSignature,
        Rule02InvalidDeclaration,
        Rule03InvalidDeclaredRoot,
        Rule04InvalidReturnExpr,
        Rule05MismatchPaths
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        var m = (IMethodSymbol)ctx.Symbol;
        if (!m.IsRefPath(out var segments))
            return;

        var signatureIsValid = IsValidRefPathSignature(m);
        if (!signatureIsValid)
            ctx.ReportDiagnostic(Diagnostic.Create(Rule01InvalidMethodSignature, m.Locations.First()));

        if (segments.Length == 0)
            return;

        var pathIsValid = RefPath.TryCreateFromSegments(segments, out var path) && !path.IsEmpty;
        switch (pathIsValid)
        {
            case false:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02InvalidDeclaration, m.Locations.First()));
                break;

            case true when signatureIsValid && path.Segments[0] != m.Parameters[0].Name:
                ctx.ReportDiagnostic(Diagnostic.Create(Rule03InvalidDeclaredRoot, m.Locations.First()));
                break;
        }
    }

    private static bool IsValidRefPathSignature(IMethodSymbol m)
    {
        return m.IsExtensionMethod && m.ReturnsRef() && m.Parameters.First().IsRef();
    }

    private static void AnalyzeReturn(OperationAnalysisContext ctx)
    {
        var r = (IReturnOperation)ctx.Operation;
        var v = r.ReturnedValue?.Unwrapped();

        if (v?.Type == null || v.Type.IsReferenceType || v.Type.IsRefLikeType)
            return;

        var m = r.GetEnclosingMethod(ctx.CancellationToken);
        if (m == null || !m.IsRefPath(out var segments))
            return;

        var retPath = v.ToRefPath();
        if (retPath.IsEmpty)
            ctx.ReportDiagnostic(Diagnostic.Create(Rule04InvalidReturnExpr, v.Syntax.GetLocation()));

        if (segments.Length == 0 || retPath.IsEmpty)
            return;

        if (!RefPath.TryCreateFromSegments(segments, out var declPath) || declPath.IsEmpty)
            return;

        var decl = declPath.ToString();
        var actual = retPath.ToString();
        if (decl != actual)
            ctx.ReportDiagnostic(Diagnostic.Create(Rule05MismatchPaths, v.Syntax.GetLocation(), actual, decl));
    }
}