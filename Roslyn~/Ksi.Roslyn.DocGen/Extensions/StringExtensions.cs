using System.Text;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class StringExtensions
{
    private const string EscapeChars = @"\`*_{}[]<>()#+-|"; // Missing: .!

    public static string ToMd(this string self)
    {
        var sb = new StringBuilder(self.Length * 2);

        foreach (var c in self)
        {
            if (EscapeChars.Contains(c))
                sb.Append('\\');

            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string ToMdFragment(this string self)
    {
        var filtered = self
            .ToLower()
            .Select(c =>
            {
                if (char.IsDigit(c) || char.IsLetter(c))
                    return c;

                return char.IsWhiteSpace(c) ? '-' : ' ';
            })
            .Where(c => c != ' ')
            .ToArray();

        return "#" + new string(filtered);
    }

    public static string Decapitalize(this string self)
    {
        return self.Length switch
        {
            0 => self,
            1 => self.ToLower(),
            _ => char.ToLower(self[0]) + self[1..]
        };
    }
}