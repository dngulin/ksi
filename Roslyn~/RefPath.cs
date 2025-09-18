using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Ksi.Roslyn;

public readonly struct RefPath
{
    public const string IndexerName = "[n]";
    public const string ItemSuffix = "()";

    public static RefPath Empty => new RefPath(ImmutableArray<string>.Empty, 0);

    public readonly ImmutableArray<string> Path;
    public readonly int DynSizedLength;
    public readonly int ExplicitLength;

    public RefPath(ImmutableArray<string> path, int dynSizedLength)
    {
        Path = path;
        DynSizedLength = dynSizedLength;
        ExplicitLength = path.TakeWhile(i => !i.EndsWith(ItemSuffix)).Count();
    }

    public bool IsEmpty => Path.IsEmpty;
    public bool PointsToDynSized => Path.Length == DynSizedLength;

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Path.Length; i++)
        {
            if (i != 0 && Path[i] != IndexerName)
                sb.Append('.');

            sb.Append(Path[i]);

            if (i == DynSizedLength - 1)
                sb.Append('!');
        }

        return sb.ToString();
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
    public static RefRelation GetExplicitPathRelationTo(in this RefPath self, in RefPath other)
    {
        if (self.Path.Length == 0 || other.Path.Length == 0 || self.Path[0] != other.Path[0])
            return RefRelation.Unrelated;

        var len = Math.Min(self.ExplicitLength, other.ExplicitLength);
        for (var i = 0; i < len; i++)
        {
            if (self.Path[i] != other.Path[i])
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
        var end = Math.Min(self.DynSizedLength + 1, self.Path.Length);

        for (var i = start; i < end; i++)
        {
            var item = self.Path[i];
            if (item == RefPath.IndexerName || item.EndsWith(RefPath.ItemSuffix))
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