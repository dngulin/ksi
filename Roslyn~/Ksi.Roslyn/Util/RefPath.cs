using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Ksi.Roslyn.Util;

public readonly struct RefPath
{
    public const string Indexer = "[n]";
    public const string MethodSuffix = "()";
    private const string DynSeparator = "!";

    public static RefPath Empty => new RefPath(ImmutableArray<string>.Empty, 0, false);

    public readonly ImmutableArray<string> Segments;
    public readonly int DynSizedLength;
    public readonly int ExplicitLength;
    public readonly bool IsDerivedFromLocalAccessScope;

    public RefPath(ImmutableArray<string> segments, int dynSizedLength, bool isDerivedFromLocalAccessScope)
    {
        Segments = segments;
        DynSizedLength = dynSizedLength;
        ExplicitLength = segments.TakeWhile(i => !i.EndsWith(MethodSuffix)).Count();
        IsDerivedFromLocalAccessScope = isDerivedFromLocalAccessScope;
    }

    public bool IsEmpty => Segments.IsEmpty;
    public bool PointsToDynSized => Segments.Length == DynSizedLength;

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Segments.Length; i++)
        {
            if (i != 0 && Segments[i] != Indexer)
                sb.Append('.');

            sb.Append(Segments[i]);

            if (i == DynSizedLength - 1)
                sb.Append(DynSeparator);
        }

        return sb.ToString();
    }

    public static bool TryCreateFromSegments(ImmutableArray<string?> segments, out RefPath result)
    {
        result = Empty;

        if (segments.IsEmpty)
            return true;

        if (segments.Any(s => !IsValidSegment(s)))
            return false;

        var dynSepCount = segments.Count(s => s == DynSeparator);
        if (dynSepCount > 1)
            return false;

        var dynSizedLen = dynSepCount == 0 ? 0 : segments.TakeWhile(s => s != DynSeparator).Count();

        result = new RefPath(segments.Where(s => s != DynSeparator).ToImmutableArray()!, dynSizedLen, false);
        return true;
    }

    private static bool IsValidSegment(string? segment)
    {
        if (segment == null)
            return false;

        if (segment is Indexer or DynSeparator)
            return true;

        if (segment.EndsWith(MethodSuffix))
            segment = segment.Substring(0, segment.Length - MethodSuffix.Length);

        return SyntaxFacts.IsValidIdentifier(segment);
    }
}

public enum RefRelation
{
    Unrelated,
    Sibling,
    Parent,
    Same,
    Child
}

public static class RefExtensions {
    private static RefRelation GetExplicitPathRelationTo(in this RefPath self, in RefPath other)
    {
        if (self.Segments.Length == 0 || other.Segments.Length == 0 || self.Segments[0] != other.Segments[0])
            return RefRelation.Unrelated;

        var len = Math.Min(self.ExplicitLength, other.ExplicitLength);
        for (var i = 0; i < len; i++)
        {
            if (self.Segments[i] != other.Segments[i])
                return RefRelation.Sibling;
        }

        if (self.ExplicitLength < other.ExplicitLength)
            return RefRelation.Parent;

        if (self.ExplicitLength > other.ExplicitLength)
            return RefRelation.Child;

        return RefRelation.Same;
    }

    private static bool HasResizableItemsSince(in this RefPath self, int start)
    {
        var end = Math.Min(self.DynSizedLength + 1, self.Segments.Length);

        for (var i = start; i < end; i++)
        {
            var item = self.Segments[i];
            if (item == RefPath.Indexer || item.EndsWith(RefPath.MethodSuffix))
                return true;
        }

        return false;
    }

    public static bool CanBeUsedToInvalidate(in this RefPath self, in RefPath other)
    {
        if (!self.PointsToDynSized)
        {
            // Having this ref it is not possible to invalidate any other
            return false;
        }

        switch (self.GetExplicitPathRelationTo(other))
        {
            case RefRelation.Unrelated:
            case RefRelation.Sibling:
                return false;

            case RefRelation.Parent:
            case RefRelation.Same:
                return other.HasResizableItemsSince(self.ExplicitLength);

            case RefRelation.Child:
                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static bool CanAlisWith(in this RefPath self, in RefPath other)
    {
        if (!self.PointsToDynSized && !other.PointsToDynSized)
            return false;

        switch (self.GetExplicitPathRelationTo(other))
        {
            case RefRelation.Unrelated:
            case RefRelation.Sibling:
                return false;

            case RefRelation.Parent:
                return other.PointsToDynSized || other.HasResizableItemsSince(self.ExplicitLength);

            case RefRelation.Same:
                return self.ExplicitLength <= self.DynSizedLength || other.ExplicitLength <= other.DynSizedLength;

            case RefRelation.Child:
                return self.PointsToDynSized;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}