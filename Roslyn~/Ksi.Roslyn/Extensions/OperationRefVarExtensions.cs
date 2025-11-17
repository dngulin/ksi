using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                    var lifetimeIntersected = (invocationReached && SameBranches(declBlock, r, self)) ||
                                              CheckInLoopReference(r, declBlock, invocationPos);
                    if (!lifetimeIntersected)
                        continue;

                    variables.Remove(r.Local);
                    yield return info;
                    break;
                }
            }
        }
    }

    private static bool SameBranches(IBlockOperation root, ILocalReferenceOperation localRef, IInvocationOperation inv)
    {
        var rNodes = GetBranchingNodes(localRef, root);
        var iNodes = GetBranchingNodes(inv, root);

        for (var i = 0; i < Math.Min(rNodes.Length, iNodes.Length); i++)
        {
            if (rNodes[i] != iNodes[i])
                return false;
        }

        return true;
    }

    private static ImmutableArray<IOperation> GetBranchingNodes(IOperation op, IBlockOperation root)
    {
        if (op.Parent == null || op.Parent == root)
            return ImmutableArray<IOperation>.Empty;

        var builder = ImmutableArray.CreateBuilder<IOperation>();

        for (var p = op; p != null; p = p.Parent)
        {
            if (p.Parent == root)
                break;

            if (p.Parent is IConditionalOperation or ISwitchOperation or ISwitchExpressionOperation)
                builder.Insert(0, p);
        }

        return builder.ToImmutable();
    }

    private static bool CheckInLoopReference(ILocalReferenceOperation r, IBlockOperation declBlock, int pos)
    {
        var loopBody = r.FindParentLoopBodyWithin(declBlock);
        return loopBody != null && loopBody.Syntax.Span.IntersectsWith(pos);
    }

    public static RefVarInfo? FindRefVar(this ILocalReferenceOperation self)
    {
        var body = self.GetEnclosingBody();
        if (body == null)
            return null;

        var eqc = SymbolEqualityComparer.Default;
        foreach (var op in body.Descendants())
        {
            if (op == self)
                break;

            if (op is not IVariableDeclaratorOperation d || !eqc.Equals(d.Symbol, self.Local))
                continue;

            var p = d.GetRefVarProducerOp(out var varKind);
            if (p == null)
                return null;

            return new RefVarInfo(d.Symbol, varKind, p);
        }

        return null;
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