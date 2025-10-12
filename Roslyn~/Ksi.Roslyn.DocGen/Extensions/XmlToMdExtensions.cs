using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class XmlToMdExtensions
{
    public static string? ToMd(this XNode self, string elementName, Compilation comp)
    {
        return (self as XContainer)?.Element(elementName)?.ToMd(comp);
    }

    public static string[] ManyToMd(this XNode self, string elementName, Compilation comp)
    {
        return (self as XContainer)?
               .Elements(elementName)
               .Select(e => e.ToMd(comp))
               .ToArray()
               ?? [];
    }

    private static string ToMd(this XNode self, Compilation comp)
    {
        return self switch
        {
            XElement element => element.ToMd(comp),
            XCData cdata => $"```\n{cdata.Value.Trim()}\n```",
            XComment comment => $"<!--{comment.Value.Trim()}-->",
            XText text => text.Value.XmlTextToMd(self.PreviousNode != null, self.NextNode != null),
            _ => ""
        };
    }

    private static string XmlTextToMd(this string self, bool keepLeadingSpace, bool keepTrailingSpace)
    {
        var prefix = keepLeadingSpace && self.StartsWith(' ') ? " " : "";
        var suffix = keepTrailingSpace && self.EndsWith(' ') ? " " : "";

        const StringSplitOptions splitOpt = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        var value = string.Join("\n", self.Split('\n', splitOpt));

        return $"{prefix}{value}{suffix}";
    }

    private static string ToMd(this XElement self, Compilation comp)
    {
        var text = self.Nodes().ToMd(comp);
        return self.Name.LocalName switch
        {
            "c" => $"`{self.Value.Replace("`", @"\`")}`",
            "see" => self.TrySee(comp, ref text, out var link) ? $"[{text}]({link})" : $"`{text}`",
            "para" => $"{self.OptPrefix("\n\n")}{text}",
            "item" => $"\n- {text}",
            "param" => $"`{self.NameAttr()}` — {text.Decapitalize()}",
            "exception" => $"`{self.CrefShort()}` — {text.Decapitalize()}",
            _ => text
        };
    }

    private static string ToMd(this IEnumerable<XNode> self, Compilation comp)
    {
        return self.Aggregate("", (acc, e) => acc + e.ToMd(comp));
    }

    private static string? Attr(this XElement self, string name) => self.Attribute(name)?.Value;

    private static string NameAttr(this XElement self) => self.Attr("name") ?? "???";
    private static string CrefShort(this XElement self) => self.Attr("cref")?.Split('.').Last() ?? "???";

    private static string OptPrefix(this XElement self, string value) => self.PreviousNode == null ? "" : value;

    private static bool TrySee(this XElement self, Compilation comp, ref string title, out string link)
    {
        link = "";

        var cref = self.Attr("cref");
        if (cref == null)
            return false;

        var symbol = DocumentationCommentId.GetFirstSymbolForReferenceId(cref, comp);
        if (symbol is not INamedTypeSymbol t)
            return false;

        if (title == "")
            title = t.ToMd();

        var ns = t.ContainingNamespace.ToDisplayString();
        if (ns == "Ksi")
        {
            link = t.MdFileName();
        }
        else
        {
            var name = t.MetadataName.Replace('`', '-');
            link = $"https://learn.microsoft.com/en-us/dotnet/api/{ns}.{name}?view=netstandard-2.1";
        }

        return true;
    }
}