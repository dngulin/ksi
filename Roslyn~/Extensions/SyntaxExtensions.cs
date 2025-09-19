using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Ksi.Roslyn.SymbolNames;

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
        return name == Dealloc || name == Dealloc + Suffix;
    }

    public static bool ContainsNoCopy(this SyntaxList<AttributeListSyntax> lists)
    {
        foreach (var l in lists)
        foreach (var a in l.Attributes)
        {
            if (a.IsNoCopy())
                return true;
        }

        return false;
    }

    private static bool IsNoCopy(this AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return name == NoCopy || name == NoCopy + Suffix;
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
        return name == RefList || name == RefList + Suffix;
    }
}