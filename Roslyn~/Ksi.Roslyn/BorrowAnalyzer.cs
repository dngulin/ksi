using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BorrowAnalyzer: DiagnosticAnalyzer
{
    private static int _ruleId;

    private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"BORROW{++_ruleId:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor NonRefPathRule = Rule(
        DiagnosticSeverity.Error,
        "Non RefPath reference to DynSized data",
        "Operation produces non-RefPath reference to DynSized data that breaks reference lifetime analysis"
    );

    private static readonly DiagnosticDescriptor AssigningRule = Rule(
        DiagnosticSeverity.Error,
        "Assigning DynSized Reference",
        "Assigning or modifying a local DynSized reference is not supported by lifetime analyzer"
    );

    private static readonly DiagnosticDescriptor LocalRefInvalidationRule = Rule(
        DiagnosticSeverity.Error,
        "Local Reference Invalidation",
        "Passing a mutable reference argument to `{0}` " +
        "invalidates memory safety guaranties for the local variable `{1}` pointing to `{2}`. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor ArgumentAliasingRule = Rule(
        DiagnosticSeverity.Error,
        "Argument Reference Aliasing",
        "Passing a mutable reference argument to `{0}` alongside with a reference to `{1}` " +
        "invalidates memory safety rules within the calling method. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor EscapeExclusiveAccessRule = Rule(
        DiagnosticSeverity.Error,
        "Reference Escapes Access Scope",
        "Reference derived from the `ExclusiveAccess<T>` escapes the access scope"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        NonRefPathRule,
        AssigningRule,
        LocalRefInvalidationRule,
        ArgumentAliasingRule,
        EscapeExclusiveAccessRule
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeVariableAssignmentRef, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeRefArgs, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeInvocationRefBreaking, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeWrappedRefArg, OperationKind.Argument);
        context.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (d.Symbol.IsRefOrWrappedRef())
            AnalyzeRefOrWrappedRefVar(ctx, d);
    }

    private static void AnalyzeRefOrWrappedRefVar(OperationAnalysisContext ctx, IVariableDeclaratorOperation d)
    {
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
        if (op.ReferencesDynSized() && !op.ProducesRefPath())
            ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRule, op.Syntax.GetLocation()));
    }

    private static void AnalyzeVariableAssignmentRef(OperationAnalysisContext ctx)
    {
        var a = (ISimpleAssignmentOperation)ctx.Operation;
        var v = a.Value;

        switch (a.Target)
        {
            case ILocalReferenceOperation lr when IsRefAssignment(a, lr) && IsDynSizedAssignment(lr, v):
            case IParameterReferenceOperation pr when IsRefAssignment(pr) && IsDynSizedAssignment(pr, v):
            {
                ctx.ReportDiagnostic(Diagnostic.Create(AssigningRule, a.Syntax.GetLocation()));
                break;
            }
        }
    }

    private static bool IsRefAssignment(ISimpleAssignmentOperation a, ILocalReferenceOperation tgt)
        => (a.IsRef && tgt.Local.IsRef) || tgt.Local.Type.IsWrappedRef();
    private static bool IsRefAssignment(IParameterReferenceOperation tgt)
        => tgt.Parameter.Type.IsWrappedRef();

    private static bool IsDynSizedAssignment(ILocalReferenceOperation tgt, IOperation v)
        => tgt.Local.Type.IsDynSizedOrWrapsDynSized() || tgt.ReferencesDynSized() || v.ReferencesDynSized();
    private static bool IsDynSizedAssignment(IParameterReferenceOperation tgt, IOperation v)
        => tgt.Parameter.Type.WrapsDynSized() || v.ReferencesDynSized();

    private static void AnalyzeRefArgs(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;
        if (i.TargetMethod.ReturnsRefPath())
            return;

        foreach (var a in i.Arguments)
        {
            var p = a.Parameter;
            if (p == null || !p.IsRefOrWrappedRef())
                continue;

            var v = a.Value;
            if (v.ReferencesDynSized(false) && !v.ProducesRefPath())
                ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRule, v.Syntax.GetLocation()));
        }
    }

    private static void AnalyzeInvocationRefBreaking(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;

        var args = i.Arguments
            .Where(a => a.Parameter != null && a.Parameter.IsRefOrWrappedRef() && a.Value.ReferencesDynSized())
            .Select(a =>
            {
                var p = a.Parameter!;
                var v = a.Value;
                return (
                    IsMut: p.IsMut() && !p.IsDynNoResize(),
                    RefPath: v.ToRefPath(),
                    Location: v.Syntax.GetLocation()
                );
            })
            .Where(t => !t.RefPath.IsEmpty)
            .ToImmutableArray();

        if (args.All(a => !a.IsMut))
            return;

        AnalyzeBreakingRefs(ctx, i, args.Where(t => t.IsMut).Select(t => (t.RefPath, t.Location)).ToImmutableArray());
        AnalyzeArgAliases(ctx, args);
    }

    private static void AnalyzeBreakingRefs(OperationAnalysisContext ctx, IInvocationOperation op, ImmutableArray<(RefPath RefPath, Location Location)> args)
    {
        if (args.Length == 0)
            return;

        var body = op.GetEnclosingBody();
        if (body == null)
            return;

        var vars = body
            .FindLocalRefsWithLifetimeIntersectingPos(op.Syntax.SpanStart)
            .Where(v => v.ReferencesDynSized())
            .Select(v => (v.Symbol, RefPath: v.GetRefPath()))
            .Where(t => !t.RefPath.IsEmpty)
            .Select(t => (t.Symbol.Name, t.RefPath))
            .ToImmutableArray();

        if (vars.Length == 0)
            return;

        foreach (var (argRefPath, argLocation) in args)
        foreach (var (localRefName, localRefPath) in vars)
        {
            if (argRefPath.CanBeUsedToInvalidate(localRefPath))
                ctx.ReportDiagnostic(Diagnostic.Create(LocalRefInvalidationRule, argLocation, argRefPath, localRefName, localRefPath));
        }
    }

    private static void AnalyzeArgAliases(OperationAnalysisContext ctx, ImmutableArray<(bool IsMut, RefPath RefPath, Location Location)> args)
    {
        for (var i = 0; i < args.Length; i++)
        for (var j = 0; j < args.Length; j++)
        {
            if (i == j || !args[i].IsMut)
                continue;

            var a = args[i].RefPath;
            var b = args[j].RefPath;

            if (a.CanAlisWith(b))
                ctx.ReportDiagnostic(Diagnostic.Create(ArgumentAliasingRule, args[i].Location, a, b));
        }
    }

    private static void AnalyzeWrappedRefArg(OperationAnalysisContext ctx)
    {
        var a = (IArgumentOperation)ctx.Operation;

        var p = a.Parameter;
        if (p is not { RefKind: RefKind.Ref or RefKind.Out } || !p.Type.IsWrappedRef())
            return;

        var t = p.Type;
        var v = a.Value;

        switch (p.RefKind)
        {
            case RefKind.Ref when t.WrapsDynSized() || v.ReferencesDynSized():
            case RefKind.Out when v is ILocalReferenceOperation && (t.WrapsDynSized() || v.ReferencesDynSized()):
            case RefKind.Out when v is IParameterReferenceOperation && t.WrapsDynSized():
                ctx.ReportDiagnostic(Diagnostic.Create(AssigningRule, a.Syntax.GetLocation()));
                break;
            case RefKind.Out when v is IDeclarationExpressionOperation && t.WrapsDynSized():
                ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRule, a.Syntax.GetLocation()));
                break;
        }
    }

    private static void AnalyzeReturn(OperationAnalysisContext ctx)
    {
        var r = (IReturnOperation)ctx.Operation;
        if (r is not { ReturnedValue: { Type: { IsReferenceType: false } t } v })
            return;

        var retByRef = t.IsSpanOrReadonlySpan() || r.ReturnsByRef(ctx.CancellationToken);
        if (!retByRef)
            return;

        var path = v.ToRefPath();
        if (path.IsDerivedFromLocalAccessScope)
            ctx.ReportDiagnostic(Diagnostic.Create(EscapeExclusiveAccessRule, r.Syntax.GetLocation()));
    }
}