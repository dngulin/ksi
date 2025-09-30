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
    private static int _ruleId;

    private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"REFPATH{++_ruleId:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor SignatureRule = Rule(
        DiagnosticSeverity.Error,
        "Invalid [RefPath] method signature",
        "RefPath attribute should be applied to an extension method that receives `this` structure by reference " +
        "and returns a reference"
    );

    private static readonly DiagnosticDescriptor ReturnExprRule = Rule(
        DiagnosticSeverity.Error,
        "Invalid [RefPath] return operation",
        "RefPath method should return a RefPath-compatible reference"
    );

    private static readonly DiagnosticDescriptor RefPathSkipValueRule = Rule(
        DiagnosticSeverity.Error,
        "Invalid [RefPathSkip] return reference",
        "RefPathSkip extension method should return `this` parameter by reference"
    );

    private static readonly DiagnosticDescriptor RefPathItemValueRule = Rule(
        DiagnosticSeverity.Error,
        "Invalid [RefPathItem] Return Value",
        "RefPathItem extension method should return a reference derived from `this` parameter"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        SignatureRule,
        ReturnExprRule,
        RefPathSkipValueRule,
        RefPathItemValueRule
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
    }

    private static void AnalyzeReturn(OperationAnalysisContext ctx)
    {
        var r = (IReturnOperation)ctx.Operation;
        var v = r.ReturnedValue;

        if (v == null || v.Type == null || v.Type.IsReferenceType)
            return;

        var m = r.GetEnclosingMethod(ctx.CancellationToken);
        if (m == null)
            return;

        // TODO: one pass with flags enum
        // TODO: report more than one attribute?
        var isRefPathSkip = m.IsRefPathSkip();
        var isRefPathItem = m.IsRefPathItem();

        var isRefPath = isRefPathSkip || isRefPathItem;
        if (!isRefPath)
            return;

        if (!m.IsExtensionMethod || !m.ReturnsRef())
        {
            ctx.ReportDiagnostic(Diagnostic.Create(SignatureRule, m.Locations.First()));
            return;
        }

        var p = m.Parameters.First();
        if (p.RefKind == RefKind.None)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(SignatureRule, m.Locations.First()));
            return;
        }

        var rp = v.ToRefPath();
        if (rp.IsEmpty)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(ReturnExprRule, v.Syntax.GetLocation()));
            return;
        }

        if (isRefPathSkip)
        {
            if (rp.Segments.Length != 1 || rp.Segments[0] != p.Name)
                ctx.ReportDiagnostic(Diagnostic.Create(RefPathSkipValueRule, v.Syntax.GetLocation()));
        }

        if (isRefPathItem)
        {
            if (rp.Segments[0] != p.Name)
                ctx.ReportDiagnostic(Diagnostic.Create(RefPathItemValueRule, v.Syntax.GetLocation()));
        }
    }
}