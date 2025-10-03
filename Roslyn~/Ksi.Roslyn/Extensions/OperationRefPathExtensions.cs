using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn.Extensions;

public static class OperationRefPathExtensions
{
    public static RefPath ToRefPath(this IOperation self, ITypeSymbol? implicitIndexingType = null)
    {
        var ctx = new RefPathBuildCtx(16);

        if (implicitIndexingType != null)
            ctx.PrependIndexer(implicitIndexingType);

        return self.BuildRefPath(ref ctx) ? ctx.ToRefPath() : RefPath.Empty;
    }

    public static bool ProducesRefPath(this IOperation self) => !self.ToRefPath().IsEmpty;

    private static bool BuildRefPath(this IOperation self, ref RefPathBuildCtx ctx)
    {
        while (true)
        {
            switch (self)
            {
                case ILocalReferenceOperation lr:
                    if (!lr.Local.IsRefOrWrappedRef())
                    {
                        ctx.Prepend(lr.Local.Name, lr.Local.Type);
                        return true;
                    }

                    if (!lr.Local.IsRef && lr.Local.Type.IsAccessScope())
                    {
                        ctx.PrependRootAccessScope(lr.Local.Name, lr.Local.Type);
                        return true;
                    }

                    var optVar = lr.FindRefVar();
                    if (optVar == null)
                        return false;

                    if (optVar.Value.Kind == RefVarKind.IteratorItemRef)
                        ctx.PrependIndexer(optVar.Value.Symbol.Type);

                    self = optVar.Value.Producer;
                    continue;

                case IParameterReferenceOperation pr:
                    ctx.Prepend(pr.Parameter.Name, pr.Parameter.Type);
                    return true;

                case IFieldReferenceOperation f:
                    ctx.Prepend(f.Field.Name, f.Field.Type);

                    if (f.Instance == null)
                        return true;

                    self = f.Instance;
                    continue;

                case IPropertyReferenceOperation pr:
                    if (pr.IsSpanIndexer())
                    {
                        ctx.PrependIndexer(pr.Property.Type);
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
                            ctx.PrependIndexer(m.ReturnType);
                        }
                        else if (m.IsRefListAsSpan())
                        {
                            ctx.PrependMethod(m.Name, m.ReturnType);
                        }
                        else if (m.IsRefPath(out var segments))
                        {
                            if (!ctx.PrependRefPath(segments, m.Name, m.ReturnType))
                                return false;
                        }
                        else
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