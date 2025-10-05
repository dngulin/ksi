namespace Ksi.Roslyn.Tests.Util;

public static class StringExtensions
{
    public static string IndentFormat(this string self, int indent, params object[] args)
    {
        return string.Format(self.Indented(indent), args) + "\n";
    }

    private static string Indented(this string self, int indent)
    {
        return string.Join("\n", self.Split("\n").Select(s => new string(' ', indent * 4) + s));
    }
}