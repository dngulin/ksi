using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn.Extensions;

public static class SyntaxExtensions
{
    public static bool IsUnmanagedConstraint(this TypeParameterConstraintSyntax self)
    {
        return self.IsKind(SyntaxKind.TypeConstraint) && self is TypeConstraintSyntax tcs && tcs.Type.IsUnmanaged;
    }

    public static bool ContainsDealloc(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.Dealloc);
    public static bool ContainsExplicitCopy(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.ExplicitCopy);
    public static bool ContainsRefList(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.RefList);
    public static bool ContainsKsiEntity(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.KsiEntity);
    public static bool ContainsKsiArchetype(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.KsiArchetype);
    public static bool ContainsKsiDomain(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.KsiDomain);
    public static bool ContainsKsiQuery(this SyntaxList<AttributeListSyntax> self) => self.Contains(SymbolNames.KsiQuery);

    private static bool Contains(this SyntaxList<AttributeListSyntax> self, string attr)
    {
        foreach (var l in self)
        foreach (var a in l.Attributes)
        {
            if (a.Is(attr))
                return true;
        }

        return false;
    }

    private static bool Is(this AttributeSyntax self, string name)
    {
        const string ns = SymbolNames.Ksi;
        const string attr = SymbolNames.Attribute;

        var decl = self.Name.ToString();
        return decl == name ||
               decl == $"{name}{attr}" ||
               decl == $"{ns}.{name}" ||
               decl == $"{ns}.{name}{attr}";
    }

    public static ExpressionSyntax GetTypeExpr(this GenericNameSyntax self)
    {
        return self.Parent switch
        {
            ObjectCreationExpressionSyntax ocs => ocs,
            _ => self
        };
    }

    public static string PartialTypeName(this TypeDeclarationSyntax self)
    {
        return $"{self.Identifier}{self.TypeParameterList}";
    }
}