using System;
using System.Collections.Immutable;
using System.Text;

namespace Ksi.Roslyn;

public readonly struct RefPath(ImmutableArray<string> path, int dynSizedLength)
{
    public const string IndexerName = "[n]";

    public static RefPath Empty => new RefPath(ImmutableArray<string>.Empty, 0);

    public readonly ImmutableArray<string> Path = path;
    private readonly int _dynSizedLength = dynSizedLength;

    public bool IsEmpty => Path.IsEmpty;
    public bool PointsToDynSizedInstance => Path.Length == _dynSizedLength;

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < Path.Length; i++)
        {
            if (i != 0 && Path[i] != IndexerName)
                sb.Append('.');

            sb.Append(Path[i]);

            if (i == _dynSizedLength - 1)
                sb.Append('!');
        }

        return sb.ToString();
    }
}

public enum RefRelation
{
    Unrelated,
    Parent,
    Sibling,
    Same,
    Child
}

public static class RefExtensions {
    public static RefRelation GetRelationTo(in this RefPath self, in RefPath other)
    {
        if (self.Path.Length == 0 || other.Path.Length == 0 || self.Path[0] != other.Path[0])
            return RefRelation.Unrelated;

        var len = Math.Min(self.Path.Length, other.Path.Length);
        for (var i = 0; i < len; i++)
        {
            if (self.Path[i] != other.Path[i])
                return RefRelation.Sibling;
        }

        if (self.Path.Length < other.Path.Length)
            return RefRelation.Parent;

        if (self.Path.Length > other.Path.Length)
            return RefRelation.Child;

        return RefRelation.Same;
    }

    public static bool Invalidates(in this RefPath self, in RefPath other)
    {
        return self.PointsToDynSizedInstance && self.GetRelationTo(other) == RefRelation.Parent;
    }
}