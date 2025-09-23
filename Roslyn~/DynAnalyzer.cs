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

    private static readonly DiagnosticDescriptor ExplicitCopyRule = Rule(
        DiagnosticSeverity.Error,
        "ExplicitCopy Attribute Required",
        "Missing `ExplicitCopy` attribute for a struct `{0}` marked with `DynSized` attribute"
    );

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
        "Operation produces non-RefPath reference to DynSized data that breaks reference lifetime analysis"
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

    private static readonly DiagnosticDescriptor ReassignWrappedRefParameterRule = Rule(
        DiagnosticSeverity.Error,
        "Reassigning DynSized Wrapped Reference Parameter",
        "Reassigning a parameter that wraps a DynSized reference is not supported by lifetime analyzer. " +
        "Consider to introduce a local variable"
    );

    private static readonly DiagnosticDescriptor FieldOfReferenceTypeRule = Rule(
        DiagnosticSeverity.Error,
        "Field Of Reference Type",
        "Type `{0}` cannot be a field of a reference type. Consider to wrap it with `ExclusiveAccess<{0}>`"
    );

    private static readonly DiagnosticDescriptor ReferenceTypeArgumentRule = Rule(
        DiagnosticSeverity.Error,
        "Type Argument Of Reference Type",
        "Type `{0}` cannot be a type argument of a reference type. Consider to wrap it with `ExclusiveAccess<{0}>`"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        ExplicitCopyRule,
        FieldRule,
        RedundantRule,
        NonRefPathRefenceRule,
        LocalRefInvalidationRule,
        ArgumentRefAliasingRule,
        DynNoResizeRule,
        DynNoResizeAnnotationRule,
        ReassignWrappedRefParameterRule,
        FieldOfReferenceTypeRule,
        ReferenceTypeArgumentRule
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
        context.RegisterOperationAction(AnalyzeVariableAssignmentRef, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeInvocationArgs, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeInvocationRefBreaking, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeDynNoResizeArgs, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeWrappedRefArgs, OperationKind.Invocation);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
        context.RegisterSyntaxNodeAction(AnalyzeArrayType, SyntaxKind.ArrayType);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var sym = (IFieldSymbol)ctx.Symbol;
        if (!sym.Type.IsDynSized())
            return;

        if (sym.ContainingType.TypeKind == TypeKind.Struct && !sym.ContainingType.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));

        if (sym.ContainingType.TypeKind == TypeKind.Class)
        {
            var loc = sym.GetDeclaredTypeLocation(ctx.CancellationToken);
            ctx.ReportDiagnostic(Diagnostic.Create(FieldOfReferenceTypeRule, loc, sym.Type.Name));
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
            ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));

        if (!sym.IsExplicitCopy())
            ctx.ReportDiagnostic(Diagnostic.Create(ExplicitCopyRule, sym.Locations.First(), sym.Name));
    }

    private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
    {
        var d = (IVariableDeclaratorOperation)ctx.Operation;
        if (d.Symbol.IsRefOrWrappedRef())
            AnalyzeRefOrWrappedRefVar(ctx, d);

        var t = d.Symbol.Type switch
        {
            IArrayTypeSymbol a when a.ElementType.IsDynSized() => a.ElementType,
            INamedTypeSymbol n when n.IsGenericReferenceTypeOverDynSized(out var dyn) => dyn,
            _ => null
        };

        if (t != null)
        {
            var loc = d.GetDeclaredTypeLocation();
            ctx.ReportDiagnostic(Diagnostic.Create(ReferenceTypeArgumentRule, loc, t.Name));
        }
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

    private static void AnalyzeVariableAssignmentRef(OperationAnalysisContext ctx)
    {
        var a = (ISimpleAssignmentOperation)ctx.Operation;

        switch (a.Target)
        {
            case ILocalReferenceOperation lr when lr.Local.IsRef && a.IsRef || lr.Local.Type.IsWrappedRef():
                AnalyzeReferenceOp(ctx, a.Target);
                break;

            case IParameterReferenceOperation pr when pr.Parameter.Type.WrapsDynSized():
                ctx.ReportDiagnostic(Diagnostic.Create(ReassignWrappedRefParameterRule, a.Syntax.GetLocation()));
                break;
        }
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
            .FindLocalRefsWithLifetimeIntersectingPos(op.Syntax.SpanStart)
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
                ctx.ReportDiagnostic(Diagnostic.Create(DynNoResizeRule, location, refPath));
        }
    }

    private static void AnalyzeWrappedRefArgs(OperationAnalysisContext ctx)
    {
        var i = (IInvocationOperation)ctx.Operation;

        foreach (var a in i.Arguments)
        {
            var p = a.Parameter;
            if (p is not { RefKind: RefKind.Ref or RefKind.Out } || !p.Type.IsWrappedRef())
                continue;

            switch (p.RefKind)
            {
                case RefKind.Ref when p.Type.WrapsDynSized() || a.Value.ReferencesDynSizeInstance():
                case RefKind.Out when p.Type.WrapsDynSized():
                    ctx.ReportDiagnostic(Diagnostic.Create(NonRefPathRefenceRule, a.Syntax.GetLocation()));
                    break;
            }
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
                ctx.ReportDiagnostic(Diagnostic.Create(DynNoResizeAnnotationRule, p.Locations.First()));
        }
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
    {
        var s = (GenericNameSyntax)ctx.Node;
        if (s.IsUnboundGenericName || s.Parent is VariableDeclarationSyntax)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
        if (i.Type is not INamedTypeSymbol { IsReferenceType: true, IsGenericType: true } t)
            return;

        if (t.IsExclusiveAccess())
            return;

        foreach (var a in t.TypeArguments)
        {
            if (a is INamedTypeSymbol na && na.IsDynSized())
                ctx.ReportDiagnostic(Diagnostic.Create(ReferenceTypeArgumentRule, s.GetLocation(), na.Name));
        }
    }

    private static void AnalyzeArrayType(SyntaxNodeAnalysisContext ctx)
    {
        var a = (ArrayTypeSyntax)ctx.Node;
        if (a.Parent is VariableDeclarationSyntax)
            return;

        var i = ctx.SemanticModel.GetTypeInfo(a.ElementType, ctx.CancellationToken);
        if (i.Type is not INamedTypeSymbol { TypeKind: TypeKind.Struct } t)
            return;

        if (t.IsDynSized())
            ctx.ReportDiagnostic(Diagnostic.Create(ReferenceTypeArgumentRule, a.GetLocation(), t.Name));
    }
}