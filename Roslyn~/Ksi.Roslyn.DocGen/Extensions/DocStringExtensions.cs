using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class DocStringExtensions
{
    private const string FallbackDocString = "<member><summary>Missing XML doc</summary></member>";

    public static XNode DocXml(this ISymbol self)
    {
        var xml = self.GetDocumentationCommentXml();
        if (string.IsNullOrEmpty(xml))
            xml = FallbackDocString;

        return XDocument.Parse(xml).Element("member")!;
    }

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
            XText text => text.Value.ToMd(),
            _ => ""
        };
    }

    private static string ToMd(this string self)
    {
        const StringSplitOptions splitOpt = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        return string.Join("\n", self.Trim().Split('\n', splitOpt)).Trim();
    }

    private static string ToMd(this XElement self)
    {
        var separator = self.PreviousNode == null ? "" : "\n\n";
        return self.Name.LocalName switch
        {
            "para" => $"{separator}{self.Nodes().ToMd()}",
            "item" => $"\n- {self.Nodes().ToMd()}",
            "param" => $"`{self.Attr("name")}` — " + self.Nodes().ToMd().Decapitalize(),
            "exception" => $"`{self.Attr("cref").Split('.').LastOrDefault()}` — " + self.Nodes().ToMd().Decapitalize(),
            _ => self.Nodes().ToMd()
        };
    }

    private static string ToMd(this IEnumerable<XNode> self) => self.Aggregate("", (acc, e) => acc + e.ToMd());

    private static string Attr(this XElement self, string name) => self.Attribute(name)?.Value ?? "???";

    public static string ToDecl(this INamedTypeSymbol symbol)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntax is not TypeDeclarationSyntax tds)
            return "// No type declaration found";

        var sb = new StringBuilder(128);

        sb.Append(tds.Modifiers);
        sb.Append(' ');
        sb.Append(tds.Keyword);
        sb.Append(' ');
        sb.Append(tds.Identifier);

        if (tds.TypeParameterList != null)
            sb.Append(tds.TypeParameterList);

        if (tds.BaseList != null)
        {
            sb.Append(' ');
            sb.Append(tds.BaseList);
        }

        if (tds.ConstraintClauses.Count > 0)
        {
            sb.Append(' ');
            sb.Append(tds.ConstraintClauses);
        }

        return sb.ToString();
    }

    public static string ToDecl(this IMethodSymbol symbol)
    {
        var sb = new StringBuilder(128);
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        switch (syntax)
        {
            case MethodDeclarationSyntax mds:
                sb.Append(mds.Modifiers);
                sb.Append(' ');
                sb.Append(mds.ReturnType);
                sb.Append(' ');
                sb.Append(mds.Identifier);

                if (mds.TypeParameterList != null)
                    sb.Append(mds.TypeParameterList);

                sb.Append(mds.ParameterList);

                if (mds.ConstraintClauses.Count > 0)
                {
                    sb.Append(' ');
                    sb.Append(mds.ConstraintClauses);
                }

                return sb.ToString();

            case ConstructorDeclarationSyntax cds:
                sb.Append(cds.Modifiers);
                sb.Append(' ');
                sb.Append(cds.Identifier);
                sb.Append(cds.ParameterList);
                return sb.ToString();

            default:
                return "// No declaration found";
        }
    }

    public static string ToDecl(this IPropertySymbol symbol)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntax is not PropertyDeclarationSyntax pds)
            return "// No declaration found";

        var sb = new StringBuilder(128);

        sb.Append(pds.Modifiers);
        sb.Append(' ');
        sb.Append(pds.Type);
        sb.Append(' ');
        sb.Append(pds.Identifier);

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