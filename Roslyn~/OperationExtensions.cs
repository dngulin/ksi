using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Ksi.Roslyn;

public static class OperationExtensions
{
    public static IOperation? GetEnclosingBody(this IOperation op)
    {
        for (var p = op; p != null; p = p.Parent)
        {
            if (p is IMethodBodyOperation || p is IConstructorBodyOperation)
                return p;
        }
        return null;
    }

    public static ImmutableArray<IArgumentOperation> GetMutableRefListArgs(this IInvocationOperation invocation)
    {
        return invocation.Arguments
            .Where(a =>
            {
                var p = a.Parameter;
                if (p is null)
                    return false;

                return p.RefKind == RefKind.Ref && p.Type.IsRefList();
            })
            .ToImmutableArray();
    }

    public static ImmutableArray<IVariableDeclaratorOperation> FindLocalRefDeclaratorsBeforePos(this IOperation self, int pos)
    {
        return self.Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .Where(op => op.Symbol.IsRef && (op.Initializer != null ? op.Initializer.Syntax.Span.End < pos : op.Syntax.Span.End < pos))
            .ToImmutableArray();
    }

    public static ImmutableDictionary<string, TextSpan> EstimateLifetimes(this IOperation self, ImmutableArray<IVariableDeclaratorOperation> declarators)
    {
        var result = new Dictionary<string, TextSpan>();

        foreach (var d in declarators)
        {
            if (d.Parent is IForEachLoopOperation loop)
            {
                result.Add(d.Symbol.Name, loop.Body.Syntax.Span);
            }
            else
            {
                result.Add(d.Symbol.Name, d.Syntax.Span);
            }
        }

        foreach (var op in self.Descendants().OfType<ILocalReferenceOperation>())
        {
            if (!result.TryGetValue(op.Local.Name, out var lifetime))
                continue;

            var span = op.Syntax.Span;
            if (span.End > lifetime.End)
                result[op.Local.Name] = TextSpan.FromBounds(lifetime.Start, span.End);
        }

        return result.ToImmutableDictionary();
    }

    public static bool IsExplicitReference(this IOperation self)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation:
                case IParameterReferenceOperation:
                    return true;

                case IFieldReferenceOperation f:
                    if (f.Instance == null) return true;

                    self = f.Instance;
                    continue;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (!m.ReturnsByRef || !m.IsExtensionMethod || !m.ProducesExplicitReference())
                    {
                        return false;
                    }

                    self = i.Arguments.First().Value;
                    continue;

                default:
                    return false;
            }
        }
    }

    public static bool ReferencesDynSizeInstance(this IOperation self)
    {
        switch (self)
        {
            case ILocalReferenceOperation lr:
                if (lr.Local.Type.IsDynSized())
                    return true;

                if (!lr.Local.IsRef)
                    return false;

                var declarator = lr.FindDeclarator();
                if (declarator.Initializer != null)
                    return declarator.Initializer.Value.ReferencesDynSizeInstance();

                return false;


            case IParameterReferenceOperation pr:
                return pr.Parameter.RefKind != RefKind.None && pr.Parameter.Type.IsDynSized();

            case IFieldReferenceOperation f:
                if (f.Type != null && f.Type.IsDynSized())
                    return true;

                if (f.Instance != null)
                    return f.Instance.ReferencesDynSizeInstance();

                return false;

            case IInvocationOperation i:
                var m = i.TargetMethod;
                if (!m.ReturnsByRef)
                    return false;

                if (m.ReturnType.IsDynSized())
                    return true;

                foreach (var a in i.Arguments)
                {
                    var p = a.Parameter;
                    if (p == null || p.RefKind == RefKind.None)
                        continue;

                    if (p.Type.IsDynSized())
                        return true;

                    if (a.Value.ReferencesDynSizeInstance())
                        return true;
                }

                return false;

            default:
                return false;
        }
    }

    public static IVariableDeclaratorOperation FindDeclarator(this ILocalReferenceOperation self)
    {
        return self
            .GetEnclosingBody()
            .Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .First(d => SymbolEqualityComparer.Default.Equals(d.Symbol, self.Local));
    }
}