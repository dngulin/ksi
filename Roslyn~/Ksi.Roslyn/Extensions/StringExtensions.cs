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
        return self.Replace(pattern, condition ? pattern.Unwrapped() : "");
    }

    private static string Unwrapped(this string self)
    {
        var idx = self.LastIndexOf('`');
        var len = idx >= 0 ? idx : self.Length - 1;
        return self.Substring(1, len - 1);
    }
}