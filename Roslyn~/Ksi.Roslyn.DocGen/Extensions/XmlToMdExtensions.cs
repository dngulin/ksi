using System.Text;
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
            XText text => text.Value.XmlTextToMd(self.PreviousNode.IsInlineNode(), self.NextNode.IsInlineNode()),
            _ => ""
        };
    }

    private static bool IsInlineNode(this XNode? self)
    {
        return self is XElement { Name.LocalName: "c" or "see" };
    }

    private static string XmlTextToMd(this string self, bool keepLeadingSpace, bool keepTrailingSpace)
    {
        if (self.Length == 0)
            return self;

        if (self.All(char.IsWhiteSpace))
            return keepLeadingSpace && keepTrailingSpace ? self[..1] : "";

        var prefix = keepLeadingSpace && char.IsWhiteSpace(self, 0) ? self[..1] : "";
        var suffix = keepTrailingSpace && char.IsWhiteSpace(self, self.Length - 1) ? self[^1..] : "";

        const StringSplitOptions splitOpt = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        var value = string.Join("\n", self.TrimStart().Split('\n', splitOpt));

        return $"{prefix}{value}{suffix}";
    }

    private static string ToMd(this XElement self, Compilation comp)
    {
        var text = self.Nodes().ToMd(comp);
        switch (self.Name.LocalName)
        {
            case "c":
            {
                var inlineCode = self.Value.Replace("`", @"\`");
                return $"`{inlineCode}`";
            }
            case "see":
            {
                return self.TryGetCref(comp, ref text, out var link) ? $"[{text}]({link})" : $"`{text}`";
            }
            case "para":
            {
                return $"{self.ParSep()}{text}";
            }
            case "item":
            {
                return $"\n- {text}";
            }
            case "param":
            {
                var name = self.NameAttr();
                var desc = text.Decapitalize();
                return $"`{name}` — {desc}";
            }
            case "exception":
            {
                var title = "";
                var desc = text.Decapitalize();
                return self.TryGetCref(comp, ref title, out var link)
                    ? $"[{title}]({link}) — {desc}"
                    : $"`{title}` — {desc}";
            }
            default:
                return text;
        }
    }

    private static string ToMd(this IEnumerable<XNode> self, Compilation comp)
    {
        return self.Aggregate(new StringBuilder(1024), (sb, node) => sb.Append(node.ToMd(comp))).ToString();
    }

    private static string? Attr(this XElement self, string name) => self.Attribute(name)?.Value;

    private static string NameAttr(this XElement self) => self.Attr("name") ?? "???";

    private static string ParSep(this XElement self)
    {
        var prev = self.PreviousNode;
        if (prev is XText text)
            prev = text.PreviousNode;

        return prev is XElement { Name.LocalName: "para" } ? "\n\n" : "";
    }

    private static bool TryGetCref(this XElement self, Compilation comp, ref string title, out string link)
    {
        link = "";

        var cref = self.Attr("cref");
        if (cref == null)
            return false;

        var symbol = DocumentationCommentId.GetFirstSymbolForReferenceId(cref, comp);
        if (symbol is not INamedTypeSymbol t)
        {
            if (title == "")
                title = cref.Split('.').Last();

            return false;
        }

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