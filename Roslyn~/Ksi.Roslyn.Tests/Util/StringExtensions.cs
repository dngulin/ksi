using Ksi.Roslyn.Util;

namespace Ksi.Roslyn.Tests.Util;

public static class StringExtensions
{
    public static string IndentFormat(this string self, int indent, params object[] args)
    {
        return string.Format(self.Indented(indent), args) + "\n";
    }

    private static string Indented(this string self, int indent)
    {
        var spaces = new string(' ', indent * AppendScope.Indent.Length);
        return string.Join("\n", self.Split("\n").Select(s => spaces + s));
    }

    public static string WithIndent(this string self, int indent)
    {
        var spaces = new string(' ', indent * AppendScope.Indent.Length);
        return string.Join($"\n{spaces}", self.Split('\n'));
    }
}