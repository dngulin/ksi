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

    public static bool ContainsDealloc(this SyntaxList<AttributeListSyntax> lists)
    {
        foreach (var l in lists)
        foreach (var a in l.Attributes)
        {
            if (a.IsDealloc())
                return true;
        }

        return false;
    }

    private static bool IsDealloc(this AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return name is
            SymbolNames.Dealloc or
            SymbolNames.Dealloc + SymbolNames.Attribute or
            $"{SymbolNames.Dealloc}.{SymbolNames.Dealloc}" or
            $"{SymbolNames.Dealloc}.{SymbolNames.Dealloc}{SymbolNames.Attribute}";
    }

    public static bool ContainsExplicitCopy(this SyntaxList<AttributeListSyntax> lists)
    {
        foreach (var l in lists)
        foreach (var a in l.Attributes)
        {
            if (a.IsExplicitCopy())
                return true;
        }

        return false;
    }

    private static bool IsExplicitCopy(this AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return name is
            SymbolNames.ExplicitCopy or
            SymbolNames.ExplicitCopy + SymbolNames.Attribute or
            $"{SymbolNames.Ksi}.{SymbolNames.ExplicitCopy}" or
            $"{SymbolNames.Ksi}.{SymbolNames.ExplicitCopy}{SymbolNames.Attribute}";
    }

    public static bool ContainsRefList(this SyntaxList<AttributeListSyntax> lists)
    {
        foreach (var l in lists)
        foreach (var a in l.Attributes)
        {
            if (a.IsRefList())
                return true;
        }

        return false;
    }

    private static bool IsRefList(this AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return name is
            SymbolNames.RefList or
            SymbolNames.RefList + SymbolNames.Attribute or
            $"{SymbolNames.Ksi}.{SymbolNames.RefList}" or
            $"{SymbolNames.Ksi}.{SymbolNames.RefList}{SymbolNames.Attribute}";
    }

    public static ExpressionSyntax GetTypeExpr(this GenericNameSyntax self)
    {
        return self.Parent switch
        {
            ObjectCreationExpressionSyntax ocs => ocs,
            _ => self
        };
    }
}