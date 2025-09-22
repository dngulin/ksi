using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Ksi.Roslyn.Extensions;

public static class OperationRefVarExtensions
{
    public static IEnumerable<RefVarInfo> FindLocalRefsWithLifetimeIntersectingPos(this IOperation self, int pos)
    {
        var variables = new Dictionary<string, (RefVarInfo Info, TextSpan Lifetime)>(17);

        foreach (var op in self.Descendants())
        {
            switch (op)
            {
                case IVariableDeclaratorOperation d:
                    if (!d.Symbol.IsRefOrWrappedRef() || d.Syntax.Span.End > pos)
                        continue;

                    var optVar = d.GetVarInfoWithLifetime();
                    if (optVar == null)
                        continue;

                    variables[d.Symbol.Name] = optVar.Value;
                    break;

                case ILocalReferenceOperation r:
                    if (!variables.TryGetValue(r.Local.Name, out var v))
                        continue;

                    var isIterItem = v.Info.Kind == RefVarKind.IteratorItemRef;

                    if (r.IsReassigned(out var newValue) && newValue != null)
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

    private static (RefVarInfo Info, TextSpan Lifetime)? GetVarInfoWithLifetime(this IVariableDeclaratorOperation d)
    {
        if (d.Initializer != null)
        {
            var varInfo = new RefVarInfo(d.Symbol, RefVarKind.LocalSymbolRef, d.Initializer!.Value);
            var lifetime = new TextSpan(d.Initializer!.Syntax.SpanStart, 0);

            return (varInfo, lifetime);
        }

        if (d.Parent is IForEachLoopOperation l)
        {
            var producer = l.Collection.WithoutConversionOp();
            var iterInfo = new RefVarInfo(d.Symbol, RefVarKind.IteratorItemRef, producer);
            return (iterInfo, l.Body.Syntax.Span);
        }

        return null;
    }

    private static bool IsReassigned(this ILocalReferenceOperation self, out IOperation? value)
    {
        if (self.Parent.IsAssignmentOf(self.Local, out value))
            return true;

        value = null;
        return false;
    }

    private static bool IsAssignmentOf(this IOperation? self, ILocalSymbol local, out IOperation? value)
    {
        value = null;

        if (self is not ISimpleAssignmentOperation { Target: ILocalReferenceOperation r } a)
            return false;

        if (!a.AssignsRefOrRefLike(r) || r.Local.Name != local.Name)
            return false;

        value = a.Value;
        return true;
    }

    public static RefVarInfo? FindRefVar(this ILocalReferenceOperation self)
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
                    {
                        if (d.Symbol.Name != self.Local.Name)
                            return null;

                        var p = d.GetRefVarProducerOp(out var varKind);
                        if (p == null)
                            return null;

                        return new RefVarInfo(d.Symbol, varKind, p);
                    }

                    case ISimpleAssignmentOperation { Target: ILocalReferenceOperation r } a:
                    {
                        if (!a.AssignsRefOrRefLike(r) || r.Local.Name != self.Local.Name)
                            return null;

                        return new RefVarInfo(r.Local, RefVarKind.LocalSymbolRef, a.Value);
                    }

                    case IArgumentOperation { Value: ILocalReferenceOperation r } a:
                    {
                        if (a.Parameter is not { RefKind: RefKind.Out } p || !p.Type.IsWrappedRef())
                            return null;

                        return new RefVarInfo(r.Local, RefVarKind.UnknownWrappedRef, a);
                    }

                    default:
                        return null;
                }
            })
            .LastOrDefault(v => v != null);
    }

    private static bool AssignsRefOrRefLike(this ISimpleAssignmentOperation self, ILocalReferenceOperation target)
    {
        return self.IsRef || target.Local.Type.IsRefLikeType;
    }

    private static IOperation? GetRefVarProducerOp(this IVariableDeclaratorOperation self, out RefVarKind kind)
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
}