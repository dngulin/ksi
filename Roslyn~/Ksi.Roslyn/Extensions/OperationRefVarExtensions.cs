using System.Collections.Generic;
using System.Linq;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn.Extensions;

public static class OperationRefVarExtensions
{
    public static IEnumerable<RefVarInfo> FindVarsWithIntersectedLifetime(this IInvocationOperation self)
    {
        var body = self.GetEnclosingBody();
        if (body == null)
            yield break;

        var variables = new Dictionary<ILocalSymbol, (RefVarInfo, IBlockOperation)>(17, SymbolEqualityComparer.Default);
        var invocationPos = self.Syntax.SpanStart;
        var invocationReached = false;

        foreach (var op in body.Descendants())
        {
            switch (op)
            {
                case IInvocationOperation invocation when !invocationReached && invocation == self:
                {
                    invocationReached = true;
                    break;
                }

                case IVariableInitializerOperation initializer when !invocationReached:
                {
                    if (initializer.Syntax.Span.IntersectsWith(invocationPos))
                        continue;

                    if (initializer.Parent is not IVariableDeclaratorOperation d || !d.Symbol.IsRefOrWrappedRef())
                        continue;

                    var info = new RefVarInfo(d.Symbol, RefVarKind.LocalSymbolRef, initializer.Value);
                    var block = d.GetContainingBlock();

                    variables[d.Symbol] = (info, block);
                    break;
                }

                case IForEachLoopOperation loop when !invocationReached:
                {
                    if (!loop.Body.Syntax.Span.IntersectsWith(invocationPos))
                        continue;

                    if (loop.LoopControlVariable is not IVariableDeclaratorOperation d || !d.Symbol.IsRef)
                        continue;

                    yield return new RefVarInfo(d.Symbol, RefVarKind.IteratorItemRef, loop.Collection.Unwrapped());
                    break;
                }

                case ILocalReferenceOperation r:
                {
                    if (!variables.TryGetValue(r.Local, out var entry))
                        continue;

                    var (info, declBlock) = entry;
                    var lifetimeIntersected = invocationReached || CheckInLoopReference(r, declBlock, invocationPos);
                    if (!lifetimeIntersected)
                        continue;

                    variables.Remove(r.Local);
                    yield return info;
                    break;
                }
            }
        }
    }

    private static bool CheckInLoopReference(ILocalReferenceOperation r, IBlockOperation declBlock, int pos)
    {
        var loopBody = r.FindParentLoopBodyWithin(declBlock);
        return loopBody != null && loopBody.Syntax.Span.IntersectsWith(pos);
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

        if (self.Parent is IForEachLoopOperation l && l.Collection.Unwrapped().IsRefListIterator(out var op))
        {
            kind = RefVarKind.IteratorItemRef;
            return op;
        }

        return null;
    }
}