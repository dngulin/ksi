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
        const string ns = SymbolNames.Ksi;
        const string name = SymbolNames.Dealloc;
        const string attr = SymbolNames.Attribute;
        return attribute.Name.ToString() is name or $"{name}{attr}" or $"{ns}.{name}" or $"{ns}.{name}{attr}";
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
        const string ns = SymbolNames.Ksi;
        const string name = SymbolNames.ExplicitCopy;
        const string attr = SymbolNames.Attribute;
        return attribute.Name.ToString() is name or $"{name}{attr}" or $"{ns}.{name}" or $"{ns}.{name}{attr}";
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
        const string ns = SymbolNames.Ksi;
        const string name = SymbolNames.RefList;
        const string attr = SymbolNames.Attribute;
        return attribute.Name.ToString() is name or $"{name}{attr}" or $"{ns}.{name}" or $"{ns}.{name}{attr}";
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