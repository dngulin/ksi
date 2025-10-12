using System.Xml.Linq;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class XmlToMdExtensions
{
    public static string? ToMd(this XNode self, string elementName)
    {
        return (self as XContainer)?.Element(elementName)?.ToMd();
    }

    public static string[] ManyToMd(this XNode self, string elementName)
    {
        return (self as XContainer)?
               .Elements(elementName)
               .Select(e => e.ToMd())
               .ToArray()
               ?? [];
    }

    private static string ToMd(this XNode self)
    {
        return self switch
        {
            XElement element => element.ToMd(),
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

    private static string ToMd(this XElement self)
    {
        return self.Name.LocalName switch
        {
            "c" => $"`{self.Nodes().ToMd().Replace("`", @"\`")}`",
            "para" => $"{self.OptPrefix("\n\n")}{self.Nodes().ToMd()}",
            "item" => $"\n- {self.Nodes().ToMd()}",
            "param" => $"`{self.NameAttr()}` — " + self.Nodes().ToMd().Decapitalize(),
            "exception" => $"`{self.CrefAttrShort()}` — " + self.Nodes().ToMd().Decapitalize(),
            _ => self.Nodes().ToMd()
        };
    }

    private static string ToMd(this IEnumerable<XNode> self) => self.Aggregate("", (acc, e) => acc + e.ToMd());

    private static string Attr(this XElement self, string name) => self.Attribute(name)?.Value ?? "???";

    private static string NameAttr(this XElement self) => self.Attr("name");
    private static string CrefAttrShort(this XElement self) => self.Attr("cref").Split('.').Last();

    private static string OptPrefix(this XElement self, string value) => self.PreviousNode == null ? "" : value;
}