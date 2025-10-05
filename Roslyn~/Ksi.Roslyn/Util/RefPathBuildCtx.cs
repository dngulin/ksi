using System.Collections.Generic;
using System.Collections.Immutable;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.Util;

public struct RefPathBuildCtx(int len)
{
    public readonly List<string> Segments = new List<string>(len);
    public bool SuffixFinished;
    public int SuffixLength;
    public bool DerivedFromLocalAccessScope;
}

public static class RefPathBuildExtensions
{
    public static RefPath ToRefPath(this in RefPathBuildCtx self)
    {
        return new RefPath(
            self.Segments.ToImmutableArray(),
            self.Segments.Count - self.SuffixLength,
            self.DerivedFromLocalAccessScope
        );
    }

    public static void Prepend(this ref RefPathBuildCtx self, string seg, ITypeSymbol t)
    {
        self.Segments.Insert(0, seg);

        if (self.SuffixFinished)
            return;

        if (t.IsDynSizedOrWrapsDynSized())
        {
            self.SuffixFinished = true;
            return;
        }

        self.SuffixLength++;
    }

    public static void PrependIndexer(this ref RefPathBuildCtx self, ITypeSymbol t)
    {
        self.Prepend(RefPath.Indexer, t);
    }

    public static void PrependMethod(this ref RefPathBuildCtx self, string name, ITypeSymbol t)
    {
        self.Prepend(name + RefPath.MethodSuffix, t);
    }

    public static void PrependRootAccessScope(this ref RefPathBuildCtx self, string name, ITypeSymbol t)
    {
        self.DerivedFromLocalAccessScope = true;
        self.Prepend(name, t);
    }

    public static bool PrependRefPath(this ref RefPathBuildCtx self, ImmutableArray<string?> segments, string method, ITypeSymbol t)
    {
        if (segments.IsEmpty)
        {
            self.PrependMethod(method, t);
            return true;
        }

        if (!RefPath.TryCreateFromSegments(segments, out var path))
            return false;

        // Path will be prepended without the first node that will be derived from the parent operation lately
        // So, let's estimate trimmed length parameters
        var len = path.Segments.Length - 1;
        var dynSizedLen = path.DynSizedLength > 0 ? path.DynSizedLength - 1 : 0;

        var suffixLen = len - dynSizedLen;
        if (self.SuffixFinished && suffixLen > 0)
            return false;

        self.SuffixLength += suffixLen;
        self.SuffixFinished = dynSizedLen > 0;

        for (var i = path.Segments.Length - 1; i >= 1; i--)
            self.Segments.Insert(0, path.Segments[i]);

        return true;
    }
}