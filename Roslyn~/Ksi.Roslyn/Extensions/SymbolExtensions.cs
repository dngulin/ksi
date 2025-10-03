using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Ksi.Roslyn.Util.SymbolNames;

namespace Ksi.Roslyn.Extensions;

public static class SymbolExtensions
{
    public static bool IsRefOrWrappedRef(this ILocalSymbol self) => self.IsRef || self.Type.IsWrappedRef();

    public static bool Is(this AttributeData attribute, string attributeName)
    {
        return attribute.AttributeClass != null && attribute.AttributeClass.Name == attributeName + Attribute;
    }

    public static Location GetDeclaredTypeLocation(this IFieldSymbol self, CancellationToken ct)
    {
        var s = self.DeclaringSyntaxReferences.First().GetSyntax(ct);

        return s.Parent is VariableDeclarationSyntax d ?
            d.Type.GetLocation() :
            self.Locations.First();
    }

    public static string FullyQualifiedName(this INamespaceSymbol self)
    {
        if (self.IsGlobalNamespace || self.ContainingNamespace.IsGlobalNamespace)
            return self.Name;

        var segments = new List<string>(16);

        while (!self.IsGlobalNamespace)
        {
            segments.Insert(0, self.Name);
            self = self.ContainingNamespace;
        }

        return string.Join(".", segments);
    }
}