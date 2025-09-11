using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DynAnalyzer : DiagnosticAnalyzer
{
    private static int _ruleId;

    private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"DYNSIZED{++_ruleId:D2}",
            title: title,
            messageFormat: msg,
            category: "DynSized",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor FieldRule = Rule(
        DiagnosticSeverity.Error,
        "Field of Non-DynSized Structure",
        "Structure `{0}` can be a field only of a structure marked with the `DynSized` attribute"
    );

    private static readonly DiagnosticDescriptor RedundantRule = Rule(
        DiagnosticSeverity.Warning,
        "Redundant DynSized Attribute",
        "Structure `{0}` is marked with the `DynSized` attribute but doesn't have any `DynSized` fields"
    );

    private static readonly DiagnosticDescriptor NonExplicitRefenceRule = Rule(
        DiagnosticSeverity.Error,
        "Non-Explicit reference to DynSized data",
        "Non-Explicit reference to DynSized data breaks reference lifetime analysis"
    );

    private static readonly DiagnosticDescriptor LocalRefInvalidationRule = Rule(
        DiagnosticSeverity.Error,
        "Local Reference Invalidation",
        "Invocation invalidates memory safety guaranties for the `{0}` reference. " +
        "Mutable access to `{1}` can invalidate reference to `{2}`"
    );

    private static readonly DiagnosticDescriptor ArgumentRefInvalidationRule = Rule(
        DiagnosticSeverity.Error,
        "Argument Reference Invalidation",
        "Invocation invalidates memory safety guaranties. " +
        "Mutable access to `{0}` can invalidate reference to `{1}`"
    );

    private static readonly DiagnosticDescriptor ArgumentRefDuplicationRule = Rule(
        DiagnosticSeverity.Error,
        "Argument Reference Duplication",
        "Invocation invalidates memory safety guaranties. " +
        "Passing the same mutable reference to `{0}` breaks memory safety checks inside the method. " +
        "Consider to pass readonly/`DynNoResize` references to avoid the problem"
    );

    private static readonly DiagnosticDescriptor DebugRule = Rule(
        DiagnosticSeverity.Warning,
        "Debug",
        "{0}"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        FieldRule,
        RedundantRule,
        RedundantRule,
        NonExplicitRefenceRule,
        LocalRefInvalidationRule,
        ArgumentRefInvalidationRule,
        ArgumentRefDuplicationRule,
        DebugRule
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
        context.RegisterOperationAction(AnalyzeInvocationArgs, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeInvocationRefBreaking, OperationKind.Invocation);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var sym = (IFieldSymbol)ctx.Symbol;
        if (sym.Type.TypeKind != TypeKind.Struct || sym.ContainingType.TypeKind != TypeKind.Struct)
            return;

        if (sym.Type.IsDynSized() && !sym.ContainingType.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node);
        if (sym == null || !sym.IsDynSized())
            return;

        var hasDeallocFields = sym
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Type.IsDynSized());

        if (!hasDeallocFields)
            ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (!d.Symbol.IsRef)
            return;

        var i = d.Initializer;
        if (i != null)
        {
            AnalyzeReferenceOp(ctx, i.Value);
            return;
        }

        if (d.Parent is not IForEachLoopOperation loop)
            return;

        if (loop.Collection.WithoutConversionOp().IsRefListIterator(out var collParent))
            AnalyzeReferenceOp(ctx, collParent!);
    }

    private static void AnalyzeReferenceOp(OperationAnalysisContext ctx, IOperation op)
    {
        if (!op.ReferencesDynSizeInstance())
            return;

        if (!op.IsExplicitReference())
            ctx.ReportDiagnostic(Diagnostic.Create(NonExplicitRefenceRule, op.Syntax.GetLocation()));
    }

    private static void AnalyzeInvocationArgs(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;
        if (i.TargetMethod.ProducesExplicitReference())
            return;

        foreach (var a in i.Arguments)
        {
            var p = a.Parameter;
            if (p == null || p.RefKind == RefKind.None)
                continue;

            var v = a.Value;
            if (!v.ReferencesDynSizeInstance(false))
                continue;

            if (!v.IsExplicitReference())
                ctx.ReportDiagnostic(Diagnostic.Create(NonExplicitRefenceRule, v.Syntax.GetLocation()));
        }
    }

    private static void AnalyzeInvocationRefBreaking(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;

        var args = i.Arguments
            .Where(a => a.Parameter != null && a.Parameter.RefKind != RefKind.None)
            .Select(a =>
            {
                var p = a.Parameter!;
                var v = a.Value;
                return (
                    IsMut: p.RefKind == RefKind.Ref && !p.IsDynNoResize(),
                    RefPath: v.ReferencesDynSizeInstance() ? v.ToRefPath() : RefPath.Empty
                );
            })
            .Where(t => !t.RefPath.IsEmpty)
            .ToImmutableArray();

        if (args.All(a => !a.IsMut))
            return;

        AnalyzeBreakingRefs(ctx, i, args.Where(t => t.IsMut).Select(t => t.RefPath).ToImmutableArray());
        AnalyzeArgClashes(ctx, i, args);
    }

    private static void AnalyzeBreakingRefs(OperationAnalysisContext ctx, IInvocationOperation op, ImmutableArray<RefPath> args)
    {
        if (args.Length == 0)
            return;

        var body = op.GetEnclosingBody();
        if (body == null)
            return;

        var vars = body
            .FindLocalRefDeclaratorsBeforePos(op.Syntax.SpanStart)
            .Select(d => d.GetRefVarInfo())
            .SelectNonNull()
            .Where(v => v.ReferencesDynSizeInstance())
            .Select(v => (v.Declarator, RefPath: v.GetRefPath()))
            .Where(t => !t.RefPath.IsEmpty)
            .Select(t => (t.Declarator.Symbol.Name, t.RefPath, Lifetime: body.EstimateLifetimeOf(t.Declarator)))
            .ToImmutableArray();

        if (vars.Length == 0)
            return;

        var l = op.Syntax.GetLocation();

        foreach (var argRefPath in args)
        foreach (var (refName, refPath, refLifetime) in vars)
        {
            if (!refLifetime.IntersectsWith(op.Syntax.Span.End))
                continue;

            if (argRefPath.Invalidates(refPath))
                ctx.ReportDiagnostic(Diagnostic.Create(LocalRefInvalidationRule, l, refName, argRefPath, refPath));
        }
    }

    private static void AnalyzeArgClashes(OperationAnalysisContext ctx, IInvocationOperation op, ImmutableArray<(bool IsMut, RefPath RefPath)> args)
    {
        var l = op.Syntax.GetLocation();

        for (var i = 0; i < args.Length; i++)
        for (var j = 0; j < args.Length; j++)
        {
            if (i == j || !args[i].IsMut)
                continue;

            var a = args[i].RefPath;
            var b = args[j].RefPath;

            if (!a.PointsToDynSizedInstance)
                continue;

            switch (a.GetRelationTo(b))
            {
                case RefRelation.Parent:
                    ctx.ReportDiagnostic(Diagnostic.Create(ArgumentRefInvalidationRule, l, a, b));
                    break;
                case RefRelation.Same:
                    ctx.ReportDiagnostic(Diagnostic.Create(ArgumentRefDuplicationRule, l, a));
                    break;
            }
        }
    }
}