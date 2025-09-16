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

    public static IEnumerable<RefVarInfo> FindLocalRefsWitsLifetimeIntersectingPos(this IOperation self, int pos)
    {
        var variables = new Dictionary<string, (RefVarInfo Info, TextSpan Lifetime)>(17);

        foreach (var op in self.Descendants())
        {
            switch (op)
            {
                case IVariableDeclaratorOperation d:
                    if (!d.Symbol.IsRef || d.Syntax.Span.End > pos)
                        continue;

                    variables[d.Symbol.Name] = d.GetVarInfoWithLifetime();
                    break;

                case ILocalReferenceOperation r:
                    if (!variables.TryGetValue(r.Local.Name, out var v))
                        continue;

                    var isIterItem = v.Info.Kind == RefVarKind.IteratorItemRef;

                    if (r.IsReassignedRef(out var newValue) && newValue != null)
                    {
                        var oldLifeTime = isIterItem ?
                            TextSpan.FromBounds(v.Lifetime.Start, newValue.Syntax.Span.End) :
                            v.Lifetime;

                        if (oldLifeTime.IntersectsWith(pos))
                        {
                            variables.Remove(r.Local.Name);
                            yield return v.Info;
                            continue;
                        }

                        var newLifeTime = isIterItem ?
                            new TextSpan(newValue.Syntax.Span.End, v.Lifetime.End) :
                            new TextSpan(newValue.Syntax.Span.End, 0);

                        var info = new RefVarInfo(v.Info.Symbol, RefVarKind.LocalSymbolRef, newValue);
                        variables[r.Local.Name] = (info, newLifeTime);
                    }
                    else if (!isIterItem)
                    {
                        var extendedLifetime = TextSpan.FromBounds(v.Lifetime.Start, r.Syntax.Span.End);
                        if (extendedLifetime.IntersectsWith(pos))
                        {
                            variables.Remove(r.Local.Name);
                            yield return v.Info;
                            continue;
                        }

                        variables[r.Local.Name] = (v.Info, extendedLifetime);
                    }
                    break;
            }
        }

        foreach (var (varInfo, lifetime) in variables.Values)
        {
            if (lifetime.IntersectsWith(pos))
                yield return varInfo;
        }
    }

    private static (RefVarInfo Info, TextSpan Lifetime) GetVarInfoWithLifetime(this IVariableDeclaratorOperation d)
    {
        if (d.Parent is IForEachLoopOperation l)
        {
            var producer = l.Collection.WithoutConversionOp();
            var iterInfo = new RefVarInfo(d.Symbol, RefVarKind.IteratorItemRef, producer);
            return (iterInfo, l.Body.Syntax.Span);
        }

        var varInfo = new RefVarInfo(d.Symbol, RefVarKind.LocalSymbolRef, d.Initializer!);
        var lifetime = new TextSpan(d.Initializer!.Syntax.SpanStart, 0);

        return (varInfo, lifetime);
    }

    private static bool IsReassignedRef(this ILocalReferenceOperation self, out IOperation? value)
    {
        if (self.Parent.IsRefAssignmentOf(self.Local, out value))
            return true;

        value = null;
        return false;
    }

    private static bool IsRefAssignmentOf(this IOperation? self, ILocalSymbol local, out IOperation? value)
    {
        value = null;

        if (self is not ISimpleAssignmentOperation { IsRef: true } a)
            return false;

        if (a.Target is not ILocalReferenceOperation r)
            return false;

        if (r.Local.Name != local.Name)
            return false;

        value = a.Value;
        return true;
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

                    var optVar = lr.FindRefVar();
                    if (optVar != null)
                    {
                        self = optVar.Value.Producer;
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
        }
    }

    private static RefVarInfo? FindRefVar(this ILocalReferenceOperation self)
    {
        return self
            .GetEnclosingBody()
            .Descendants()
            .Select(op =>
            {
                if (op.Syntax.Span.End >= self.Syntax.SpanStart)
                    return null as RefVarInfo?;

                switch (op)
                {
                    case IVariableDeclaratorOperation d:
                        if (d.Symbol.Name != self.Local.Name)
                            return null;

                        var p = d.GetValueProducerRef(out var varKind);
                        if (p == null)
                            return null;

                        return new RefVarInfo(d.Symbol, varKind, p);

                    case ISimpleAssignmentOperation { IsRef: true, Target: ILocalReferenceOperation r } a:
                        if (r.Local.Name != self.Local.Name)
                            return null;

                        return new RefVarInfo(r.Local, RefVarKind.LocalSymbolRef, a.Value);

                    default:
                        return null;
                }
            })
            .LastOrDefault(v => v != null);

    }

    private static IOperation? GetValueProducerRef(this IVariableDeclaratorOperation self, out RefVarKind kind)
    {
        kind = RefVarKind.LocalSymbolRef;

        if (self.Initializer != null)
            return self.Initializer.Value;

        if (self.Parent is IForEachLoopOperation l && l.Collection.WithoutConversionOp().IsRefListIterator(out var op))
        {
            kind = RefVarKind.IteratorItemRef;
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

                    var optVar = lr.FindRefVar();
                    if (optVar == null)
                        return false;

                    if (optVar.Value.Kind == RefVarKind.IteratorItemRef)
                        path.PrependRefSeg(RefPath.IndexerName, optVar.Value.Symbol.Type, ref suffix);

                    self = optVar.Value.Producer;
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

    public static IMethodSymbol? GetEnclosingMethod(this IOperation self, CancellationToken ct)
    {
        var model = self.SemanticModel;

        var enclosing = model?.GetEnclosingSymbol(self.Syntax.SpanStart, ct);
        if (enclosing is IMethodSymbol method)
            return method;

        return null;

    }
}