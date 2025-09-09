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

    public static IEnumerable<IArgumentOperation> GetMutableDynSizedArgs(this IInvocationOperation invocation)
    {
        return invocation.Arguments
            .Where(a =>
            {
                var p = a.Parameter;
                if (p is null || p.RefKind != RefKind.Ref || p.IsDynNoResize())
                    return false;

                return p.Type.IsDynSized();
            });
    }

    public static IEnumerable<IVariableDeclaratorOperation> FindLocalRefDeclaratorsBeforePos(this IOperation self, int pos)
    {
        return self.Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .Where(op =>
                op.Symbol.IsRef && (op.Initializer != null
                    ? op.Initializer.Syntax.Span.End < pos
                    : op.Syntax.Span.End < pos));
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

    public static bool ReferencesDynSizeInstance(this IOperation self, bool analyzeArguments = true)
    {
        switch (self)
        {
            case ILocalReferenceOperation lr:
                if (lr.Local.Type.IsDynSized())
                    return true;

                if (!lr.Local.IsRef)
                    return false;

                var v = lr.FindDeclarator().GetValueProducerRef(out _);
                if (v != null)
                    return v.ReferencesDynSizeInstance(analyzeArguments);

                return false;


            case IParameterReferenceOperation pr:
                return pr.Parameter.RefKind != RefKind.None && pr.Parameter.Type.IsDynSized();

            case IFieldReferenceOperation f:
                if (f.Type != null && f.Type.IsDynSized())
                    return true;

                if (f.Instance != null)
                    return f.Instance.ReferencesDynSizeInstance(analyzeArguments);

                return false;

            case IInvocationOperation i:
                var m = i.TargetMethod;
                if (!m.ReturnsByRef)
                    return false;

                if (m.ReturnType.IsDynSized())
                    return true;

                if (m.Parameters.Where(p => p.RefKind != RefKind.None).Any(p => p.Type.IsDynSized()))
                {
                    return true;
                }

                if (!analyzeArguments)
                    return false;

                return i.Arguments.Any(a => a.Value.ReferencesDynSizeInstance());

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

    public static RefVarInfo? GetRefVarInfo(this IVariableDeclaratorOperation self)
    {
        if (!self.Symbol.IsRef)
            return null;

        if (self.Initializer != null)
            return new RefVarInfo(self, RefVarKind.LocalSymbolRef, self.Initializer.Value);

        if (self.Parent is IForEachLoopOperation loop)
            return new RefVarInfo(self, RefVarKind.IteratorItemRef, loop.Collection.WithoutConversionOp());

        return null;
    }

    private static IOperation? GetValueProducerRef(this IVariableDeclaratorOperation self, out bool isRefIterator)
    {
        isRefIterator = false;

        if (self.Initializer != null)
            return self.Initializer.Value;

        if (self.Parent is IForEachLoopOperation l && l.Collection.WithoutConversionOp().IsRefListIterator(out var op))
        {
            isRefIterator = true;
            return op;
        }

        return null;
    }

    public static IOperation WithoutConversionOp(this IOperation self)
    {
        if (self is IConversionOperation conv)
            return conv.Operand;

        return self;
    }

    public static bool IsRefListIterator(this IOperation self, out IOperation? collection)
    {
        collection = null;
        if (self is not IInvocationOperation i)
            return false;

        var m = i.TargetMethod;
        if (!m.IsExtensionMethod || !m.ReturnType.IsRefLikeType)
            return false;

        var p = m.Parameters.First();
        if (p.RefKind == RefKind.None || !p.Type.IsRefList())
            return false;

        collection = i.Arguments.First().Value;
        return true;
    }

    public static RefPath ToRefPath(this IOperation self, bool addImplicitIndexer = false)
    {
        var path = new List<string>(16);

        if (addImplicitIndexer)
            path.Add(RefPath.IndexerName);

        return self.PrependNodePath(path) ?
            new RefPath(path.ToImmutableArray()) :
            new RefPath(ImmutableArray<string>.Empty);
    }

    private static bool PrependNodePath(this IOperation self, List<string> path)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (!lr.Local.IsRef)
                    {
                        path.Insert(0, lr.Local.Name);
                        return true;
                    }

                    var v = lr.FindDeclarator().GetValueProducerRef(out var implicitIndexer);
                    if (v == null)
                        return false;

                    if (implicitIndexer)
                        path.Insert(0, RefPath.IndexerName);

                    self = v;
                    continue;

                case IParameterReferenceOperation pr:
                    path.Insert(0, pr.Parameter.Name);
                    return true;

                case IFieldReferenceOperation f:
                    path.Insert(0, f.Field.Name);

                    if (f.Instance == null)
                        return true;

                    self = f.Instance;
                    continue;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (!m.ReturnsByRef || !m.IsExtensionMethod)
                        return false;

                    if (m.IsRefListIndexer())
                    {
                        path.Insert(0, RefPath.IndexerName);
                    }
                    else if (!m.IsDynReturnsSelf())
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

    public static TextSpan EstimateLifetimeOf(this IOperation self, IVariableDeclaratorOperation d)
    {
        if (d.Parent is IForEachLoopOperation l)
        {
            return l.Body.Syntax.Span;
        }

        var lifetime = d.Syntax.Span;

        foreach (var op in self.Descendants().OfType<ILocalReferenceOperation>())
        {
            if (op.Local.Name != d.Symbol.Name)
                continue;

            var span = op.Syntax.Span;
            if (span.End > lifetime.End)
                lifetime = TextSpan.FromBounds(lifetime.Start, span.End);
        }

        return lifetime;
    }
}