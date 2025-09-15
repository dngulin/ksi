using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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

    public static IEnumerable<IVariableDeclaratorOperation> FindLocalRefDeclaratorsBeforePos(this IOperation self, int pos)
    {
        return self.Descendants()
            .OfType<IVariableDeclaratorOperation>()
            .Where(op =>
                op.Symbol.IsRef && (op.Initializer != null
                    ? op.Initializer.Syntax.Span.End < pos
                    : op.Syntax.Span.End < pos));
    }

    public static bool ProducesRefPath(this IOperation self)
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
                    if (!m.ReturnsRefPath())
                        return false;

                    self = i.Arguments.First().Value;
                    continue;

                default:
                    return false;
            }
        }
    }

    public static bool ReferencesDynSizeInstance(this IOperation self, bool fullGraph = true)
    {
        while (true)
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
                    {
                        self = v;
                        continue;
                    }

                    return false;


                case IParameterReferenceOperation pr:
                    return pr.Parameter.RefKind != RefKind.None && pr.Parameter.Type.IsDynSized();

                case IFieldReferenceOperation f:
                    if (f.Type != null && f.Type.IsDynSized()) return true;

                    if (f.Instance != null)
                    {
                        self = f.Instance;
                        continue;
                    }

                    return false;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (!m.ReturnsReference())
                        return false;

                    if (m.ReturnType.IsDynSized())
                        return true;

                    if (m.Parameters.Where(p => p.RefKind != RefKind.None).Any(p => p.Type.IsDynSized()))
                        return true;

                    if (m.ReturnsRefPath())
                    {
                        self = i.Arguments.First().Value;
                        fullGraph = true;
                        continue;
                    }

                    if (!fullGraph)
                        return false;

                    return i.Arguments.Any(a => a.Value.ReferencesDynSizeInstance());

                default:
                    return false;
            }

            break;
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

    /// <summary>
    /// To measure path ending that contains only `Sized` types
    /// </summary>
    private struct RefPathSuffix
    {
        public bool Finished;
        public int Length;
    }

    public static RefPath ToRefPath(this IOperation self, ITypeSymbol? implicitIndexingType = null)
    {
        var path = new List<string>(16);
        var suffix = new RefPathSuffix();

        if (implicitIndexingType != null)
            path.PrependRefSeg(RefPath.IndexerName, implicitIndexingType, ref suffix);

        return self.PrependNodePath(path, ref suffix) ?
            new RefPath(path.ToImmutableArray(), path.Count - suffix.Length) :
            RefPath.Empty;
    }

    private static bool PrependNodePath(this IOperation self, List<string> path, ref RefPathSuffix suffix)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (!lr.Local.IsRef)
                    {
                        path.PrependRefSeg(lr.Local.Name, lr.Local.Type, ref suffix);
                        return true;
                    }

                    var d = lr.FindDeclarator();
                    var v = d.GetValueProducerRef(out var implicitIndexer);
                    if (v == null)
                        return false;

                    if (implicitIndexer)
                        path.PrependRefSeg(RefPath.IndexerName, d.Symbol.Type, ref suffix);

                    self = v;
                    continue;

                case IParameterReferenceOperation pr:
                    path.PrependRefSeg(pr.Parameter.Name, pr.Parameter.Type, ref suffix);
                    return true;

                case IFieldReferenceOperation f:
                    path.PrependRefSeg(f.Field.Name, f.Field.Type, ref suffix);

                    if (f.Instance == null)
                        return true;

                    self = f.Instance;
                    continue;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (!m.ReturnsReference() || !m.IsExtensionMethod)
                        return false;

                    if (m.IsRefListIndexer())
                    {
                        path.PrependRefSeg(RefPath.IndexerName, m.ReturnType, ref suffix);
                    }
                    else if (m.IsRefPathItem())
                    {
                        path.PrependRefSeg(m.Name + RefPath.ItemSuffix, m.ReturnType, ref suffix);
                    }
                    else if (!m.IsRefPathSkip())
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

    private static void PrependRefSeg(this List<string> self, string seg, ITypeSymbol t, ref RefPathSuffix suffix)
    {
        self.Insert(0, seg);

        if (suffix.Finished)
            return;

        if (t.IsDynSized())
        {
            suffix.Finished = true;
            return;
        }

        suffix.Length++;
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

    public static IMethodSymbol? GetEnclosingMethod(this IOperation self, CancellationToken ct)
    {
        var model = self.SemanticModel;

        var enclosing = model?.GetEnclosingSymbol(self.Syntax.SpanStart, ct);
        if (enclosing is IMethodSymbol method)
            return method;

        return null;

    }
}