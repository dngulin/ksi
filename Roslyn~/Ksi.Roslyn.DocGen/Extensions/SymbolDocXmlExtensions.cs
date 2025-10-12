using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class SymbolDocXmlExtensions
{
    private const string FallbackDocString = "<member><summary>Missing XML doc</summary></member>";

    public static XNode DocXml(this ISymbol self)
    {
        var xml = self.GetDocumentationCommentXml();
        if (string.IsNullOrEmpty(xml))
            xml = FallbackDocString;

        return XDocument.Parse(xml).Element("member") ?? XDocument.Parse(FallbackDocString).Element("member")!;
    }
}