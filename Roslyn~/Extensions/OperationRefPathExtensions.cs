using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn.Extensions;

public static class OperationRefPathExtensions
{
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

    public static bool ProducesRefPath(this IOperation self) => !self.ToRefPath().IsEmpty;

    private static bool PrependNodePath(this IOperation self, List<string> path, ref RefPathSuffix suffix)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (!lr.Local.IsRefOrWrappedRef() || !lr.Local.IsRef && lr.Local.Type.IsAccessScope())
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

                case IPropertyReferenceOperation pr:
                    if (pr.IsSpanIndexer())
                    {
                        path.PrependRefSeg(RefPath.IndexerName, pr.Property.Type, ref suffix);
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
                            path.PrependRefSeg(RefPath.IndexerName, m.ReturnType, ref suffix);
                        }
                        else if (m.IsRefPathItem() || m.IsRefListAsSpan())
                        {
                            path.PrependRefSeg(m.Name + RefPath.ItemSuffix, m.ReturnType, ref suffix);
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

    private static void PrependRefSeg(this List<string> self, string seg, ITypeSymbol t, ref RefPathSuffix suffix)
    {
        self.Insert(0, seg);

        if (suffix.Finished)
            return;

        if (t.IsDynSizedOrWrapsDynSized())
        {
            suffix.Finished = true;
            return;
        }

        suffix.Length++;
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