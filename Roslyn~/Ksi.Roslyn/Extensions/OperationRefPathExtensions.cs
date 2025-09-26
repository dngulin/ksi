using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn.Extensions;

public static class OperationRefPathExtensions
{
    private struct RefPathBuildCtx
    {
        public bool Finished;
        public int SuffixLength;
        public bool DerivedFromLocalAccessScope;
    }

    public static RefPath ToRefPath(this IOperation self, ITypeSymbol? implicitIndexingType = null)
    {
        var path = new List<string>(16);
        var ctx = new RefPathBuildCtx();

        if (implicitIndexingType != null)
            path.PrependRefSeg(RefPath.IndexerName, implicitIndexingType, ref ctx);

        return self.PrependNodePath(path, ref ctx) ?
            new RefPath(path.ToImmutableArray(), path.Count - ctx.SuffixLength, ctx.DerivedFromLocalAccessScope) :
            RefPath.Empty;
    }

    public static bool ProducesRefPath(this IOperation self) => !self.ToRefPath().IsEmpty;

    private static bool PrependNodePath(this IOperation self, List<string> path, ref RefPathBuildCtx ctx)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (!lr.Local.IsRefOrWrappedRef())
                    {
                        path.PrependRefSeg(lr.Local.Name, lr.Local.Type, ref ctx);
                        return true;
                    }

                    if (!lr.Local.IsRef && lr.Local.Type.IsAccessScope())
                    {
                        ctx.DerivedFromLocalAccessScope = true;
                        path.PrependRefSeg(lr.Local.Name, lr.Local.Type, ref ctx);
                        return true;
                    }

                    var optVar = lr.FindRefVar();
                    if (optVar == null)
                        return false;

                    if (optVar.Value.Kind == RefVarKind.IteratorItemRef)
                        path.PrependRefSeg(RefPath.IndexerName, optVar.Value.Symbol.Type, ref ctx);

                    self = optVar.Value.Producer;
                    continue;

                case IParameterReferenceOperation pr:
                    path.PrependRefSeg(pr.Parameter.Name, pr.Parameter.Type, ref ctx);
                    return true;

                case IFieldReferenceOperation f:
                    path.PrependRefSeg(f.Field.Name, f.Field.Type, ref ctx);

                    if (f.Instance == null)
                        return true;

                    self = f.Instance;
                    continue;

                case IPropertyReferenceOperation pr:
                    if (pr.IsSpanIndexer())
                    {
                        path.PrependRefSeg(RefPath.IndexerName, pr.Property.Type, ref ctx);
                        self = pr.Instance!;
                        continue;
                    }

                    if (pr.IsAccessScopeValue())
                    {
                        self = pr.Instance!;
                        continue;
                    }

                    return false;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (m.IsExtensionMethod)
                    {
                        if (!m.ReturnsRefOrSpan())
                            return false;

                        if (m.IsRefListIndexer())
                        {
                            path.PrependRefSeg(RefPath.IndexerName, m.ReturnType, ref ctx);
                        }
                        else if (m.IsRefPathItem() || m.IsRefListAsSpan())
                        {
                            path.PrependRefSeg(m.Name + RefPath.ItemSuffix, m.ReturnType, ref ctx);
                        }
                        else if (!m.IsRefPathSkip())
                        {
                            return false;
                        }

                        self = i.Arguments.First().Value;
                        continue;
                    }

                    if (m.IsSpanSlice())
                    {
                        self = i.Instance!;
                        continue;
                    }

                    return false;

                default:
                    return false;
            }
        }
    }

    private static void PrependRefSeg(this List<string> self, string seg, ITypeSymbol t, ref RefPathBuildCtx ctx)
    {
        self.Insert(0, seg);

        if (ctx.Finished)
            return;

        if (t.IsDynSizedOrWrapsDynSized())
        {
            ctx.Finished = true;
            return;
        }

        ctx.SuffixLength++;
    }

    public static bool ReferencesDynSized(this IOperation self, bool fullGraph = true)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (lr.Local.Type.IsDynSizedOrWrapsDynSized())
                        return true;

                    if (!lr.Local.IsRefOrWrappedRef())
                        return false;

                    var optVar = lr.FindRefVar();
                    if (optVar != null)
                    {
                        self = optVar.Value.Producer;
                        continue;
                    }

                    return false;


                case IParameterReferenceOperation pr:
                    return pr.Parameter.IsRefOrWrappedRef() && pr.Parameter.Type.IsDynSizedOrWrapsDynSized();

                case IFieldReferenceOperation f:
                    if (f.Type != null && f.Type.IsDynSizedOrWrapsDynSized()) return true;

                    if (f.Instance != null)
                    {
                        self = f.Instance;
                        continue;
                    }

                    return false;

                case IPropertyReferenceOperation pr:
                    if (pr.IsSpanIndexer())
                    {
                        self = pr.Instance!;
                        continue;
                    }

                    if (pr.IsAccessScopeValue())
                    {
                        self = pr.Instance!;
                        continue;
                    }

                    return false;

                case IInvocationOperation i:
                    var m = i.TargetMethod;
                    if (!m.ReturnsRefOrSpan())
                        return false;

                    if (m.ReturnType.IsDynSizedOrWrapsDynSized())
                        return true;

                    if (m.Parameters.Where(p => p.IsRefOrWrappedRef()).Any(p => p.Type.IsDynSizedOrWrapsDynSized()))
                        return true;

                    if (m.IsRefPathExtension())
                    {
                        self = i.Arguments.First().Value;
                        continue;
                    }

                    if (m.IsSpanSlice())
                    {
                        self = i.Instance!;
                        continue;
                    }

                    if (!fullGraph)
                        return false;

                    return i.Arguments.Any(a => a.Value.ReferencesDynSized());

                default:
                    return false;
            }
        }
    }

    private static bool IsSpanIndexer(this IPropertyReferenceOperation self)
    {
        if (!self.Property.IsIndexer || self.Property.RefKind == RefKind.None)
            return false;

        if (self.Instance?.Type == null)
            return false;

        return self.Instance.Type.IsSpanOrReadonlySpan();
    }

    private static bool IsAccessScopeValue(this IPropertyReferenceOperation self)
    {
        if (self.Property.IsIndexer || self.Property.RefKind == RefKind.None)
            return false;

        if (self.Instance?.Type == null || self.Property.Name != "Value")
            return false;

        return self.Instance.Type.IsAccessScope();
    }
}