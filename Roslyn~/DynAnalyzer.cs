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

    private static readonly DiagnosticDescriptor NonRefPathRefenceRule = Rule(
        DiagnosticSeverity.Error,
        "Non RefPath reference to DynSized data",
        "Non RefPath reference to DynSized data breaks reference lifetime analysis"
    );

    private static readonly DiagnosticDescriptor LocalRefInvalidationRule = Rule(
        DiagnosticSeverity.Error,
        "Local Reference Invalidation",
        "Passing a mutable reference argument to `{0}` " +
        "invalidates memory safety guaranties for the local variable `{1}` pointing to `{2}`. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor ArgumentRefAliasingRule = Rule(
        DiagnosticSeverity.Error,
        "Argument Reference Aliasing",
        "Passing a mutable reference argument to `{0}` alongside with a reference to `{1}` " +
        "invalidates memory safety rules within the calling method. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor DynNoResizeRule = Rule(
        DiagnosticSeverity.Error,
        "DynNoResize Violation",
        "Passing as an argument a mutable reference to `{0}` that is derived from the `DynNoResize` parameter. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor DynNoResizeAnnotationRule = Rule(
        DiagnosticSeverity.Warning,
        "Redundant DynNoResize Annotation",
        "DynNoResize attribute is added to non-compatible parameter and has no effect."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        FieldRule,
        RedundantRule,
        NonRefPathRefenceRule,
        LocalRefInvalidationRule,
        ArgumentRefAliasingRule,
        DynNoResizeRule,
        DynNoResizeAnnotationRule
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        context.RegisterOperationAction(AnalyzeVariableDeclaratorRef, OperationKind.VariableDeclarator);
        context.RegisterOperationAction(AnalyzeVariableAssignmentRef, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeInvocationArgs, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeInvocationRefBreaking, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeDynNoResizeArgs, OperationKind.Invocation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
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

        var hasDynSizedFields = sym
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Field)
            .Cast<IFieldSymbol>()
            .Any(field => !field.IsStatic && field.Type.IsDynSized());

        if (!hasDynSizedFields)
            ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));
    }

    private static void AnalyzeVariableDeclaratorRef(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (!d.Symbol.IsRefOrWrappedRef())
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

    private static void AnalyzeVariableAssignmentRef(OperationAnalysisContext ctx)
    {
        var a = (ISimpleAssignmentOperation)ctx.Operation;

        if (a.Target is not ILocalReferenceOperation lr)
            return;

        if (lr.Local.IsRef && a.IsRef || lr.Local.Type.IsWrappedRef())
            AnalyzeReferenceOp(ctx, a.Target);
    }

    private static void AnalyzeReferenceOp(OperationAnalysisContext ctx, IOperation op)
    {
        if (op.ReferencesDynSizeInstance() && !op.ProducesRefPath())
            ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRefenceRule, op.Syntax.GetLocation()));
    }

    private static void AnalyzeInvocationArgs(OperationAnalysisContext ctx)
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
            if (v.ReferencesDynSizeInstance(false) && !v.ProducesRefPath())
                ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRefenceRule, v.Syntax.GetLocation()));
        }
    }

    private static void AnalyzeInvocationRefBreaking(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;

        var args = i.Arguments
            .Where(a => a.Parameter != null && a.Parameter.IsRefOrWrappedRef() && a.Value.ReferencesDynSizeInstance())
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
            .FindLocalRefsWitsLifetimeIntersectingPos(op.Syntax.SpanStart)
            .Where(v => v.ReferencesDynSizeInstance())
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
                ctx.ReportDiagnostic(Diagnostic.Create(ArgumentRefAliasingRule, args[i].Location, a, b));
        }
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

                return p.Type.IsDynSized() && !p.IsDynNoResize();
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
            .Where(p => p.IsMut() && p.Type.IsDynSized() && p.IsDynNoResize())
            .Select(p => p.Name)
            .ToImmutableArray();

        if (noResizeParams.IsEmpty)
            return;

        foreach (var (refPath, location) in resizableDynArgs)
        {
            var root = refPath.Segments[0];
            if (noResizeParams.Contains(root))
                ctx.ReportDiagnostic(Diagnostic.Create(DynNoResizeRule, location, refPath));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        var m = (IMethodSymbol)ctx.Symbol;

        foreach (var p in m.Parameters)
        {
            if (!p.IsDynNoResize())
                continue;

            if (!p.IsMut() || !p.Type.IsDynSized())
                ctx.ReportDiagnostic(Diagnostic.Create(DynNoResizeAnnotationRule, p.Locations.First()));
        }
    }
}