using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class SymbolToDeclExtensions
{
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
}