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
        var eqc = SymbolEqualityComparer.Default;
        var variables = new Dictionary<ILocalSymbol, (RefVarInfo Info, int LifetimeStart)>(17, eqc);

        foreach (var op in self.Descendants())
        {
            switch (op)
            {
                case IVariableDeclaratorOperation d:
                {
                    if (d.Syntax.Span.End > pos || !d.Symbol.IsRefOrWrappedRef())
                        continue;

                    var optV = d.GetVarInfoWithLifetime();
                    if (optV == null)
                        continue;

                    var v = optV.Value;
                    if (v.Info.Kind == RefVarKind.IteratorItemRef)
                    {
                        if (v.Lifetime.IntersectsWith(pos)) yield return v.Info;
                        continue;
                    }

                    variables[d.Symbol] = (v.Info, v.Lifetime.Start);
                    break;
                }

                case ILocalReferenceOperation r:
                {
                    if (!variables.TryGetValue(r.Local, out var v))
                        continue;

                    var lifetime = TextSpan.FromBounds(v.LifetimeStart, r.Syntax.Span.End);
                    if (lifetime.IntersectsWith(pos))
                    {
                        variables.Remove(r.Local);
                        yield return v.Info;
                    }

                    break;
                }
            }
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
                        if (!SymbolEqualityComparer.Default.Equals(self.Local, d.Symbol))
                            return null;

                        var p = d.GetRefVarProducerOp(out var varKind);
                        if (p == null)
                            return null;

                        return new RefVarInfo(d.Symbol, varKind, p);
                    }

                    default:
                        return null;
                }
            })
            .LastOrDefault(v => v != null);
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