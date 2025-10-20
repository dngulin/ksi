using System.Collections.Generic;

namespace Ksi.Roslyn.Extensions;

public static class StringExtensions
{
    public static string CommaSeparated(this IEnumerable<string> self)
    {
        return string.Join(", ", self);
    }
}