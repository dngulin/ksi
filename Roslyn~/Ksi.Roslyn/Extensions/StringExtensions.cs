using System.Collections.Generic;
using System.Text;

namespace Ksi.Roslyn.Extensions;

public static class StringExtensions
{
    public static string CommaSeparated(this IEnumerable<string> self)
    {
        return string.Join(", ", self);
    }

    public static StringBuilder ToStringBuilder(this string self) => new StringBuilder(self);

    public static StringBuilder Unwrap(this StringBuilder self, string pattern, bool condition)
    {
        return self.Replace(pattern, condition ? pattern.Substring(1, pattern.Length - 2) : "");
    }
}