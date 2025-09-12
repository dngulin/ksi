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
        "Passing a mutable reference argument to `{0}` " +
        "invalidates memory safety guaranties for the local reference `{1}` pointing to `{2}`. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
    );

    private static readonly DiagnosticDescriptor ArgumentRefAliasingRule = Rule(
        DiagnosticSeverity.Error,
        "Argument Reference Aliasing",
        "Passing a mutable reference argument to `{0}` alongside with a reference to `{1}` " +
        "invalidates memory safety rules within the calling method. " +
        "Consider to pass a readonly/`DynNoResize` reference to avoid the problem"
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
        ArgumentRefAliasingRule,
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
        if (i.TargetMethod.ReturnsExplicitReference())
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
                    RefPath: v.ReferencesDynSizeInstance() ? v.ToRefPath() : RefPath.Empty,
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

        foreach (var (argRefPath, argLocation) in args)
        foreach (var (localRefName, localRefPath, localRefLifetime) in vars)
        {
            if (!localRefLifetime.IntersectsWith(op.Syntax.Span.End))
                continue;

            if (argRefPath.ClashesWithLocalRef(localRefPath))
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

            if (a.ClashesWithAnotherArg(b))
                ctx.ReportDiagnostic(Diagnostic.Create(ArgumentRefAliasingRule, args[i].Location, a, b));
        }
    }
}