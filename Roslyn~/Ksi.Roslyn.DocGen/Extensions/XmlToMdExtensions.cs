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
        var contents = self.Nodes().ToMd(comp);
        return self.Name.LocalName switch
        {
            "c" => $"`{contents.Replace("`", @"\`")}`",
            "see" => self.TryGetLink(comp, out var link) ? $"[{contents}]({link})" : contents,
            "para" => $"{self.OptPrefix("\n\n")}{contents}",
            "item" => $"\n- {contents}",
            "param" => $"`{self.NameAttr()}` — {contents.Decapitalize()}",
            "exception" => $"`{self.CrefShort()}` — {contents.Decapitalize()}",
            _ => contents
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

    private static bool TryGetLink(this XElement self, Compilation comp, out string link)
    {
        link = "";

        var cref = self.Attr("cref");
        if (cref == null)
            return false;

        var symbol = DocumentationCommentId.GetFirstSymbolForReferenceId(cref, comp);
        if (symbol is not INamedTypeSymbol { ContainingNamespace.Name: "Ksi" } t)
            return false;

        link = t.MdFileName();
        return true;
    }
}