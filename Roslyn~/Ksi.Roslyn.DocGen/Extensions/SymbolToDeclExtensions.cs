using System.Collections.Immutable;
using System.Text;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class SymbolToDeclExtensions
{
    public static string ToDecl(this INamedTypeSymbol symbol, Compilation comp)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntax is EnumDeclarationSyntax eds)
            return $"{eds.Modifiers} {eds.EnumKeyword} {eds.Identifier} {eds.BaseList}";

        if (syntax is not TypeDeclarationSyntax tds)
            return "// No type declaration found";

        var sb = new StringBuilder(128);

        var attributes = tds.AttributeLists.PublicOnes(comp.GetSemanticModel(syntax.SyntaxTree));
        if (attributes.Length > 0)
            sb.AppendLine($"[{string.Join(", ", attributes)}]");

        sb.Append($"{tds.Modifiers} {tds.Keyword} {tds.Identifier}");
        if (tds.TypeParameterList != null)
            sb.Append(tds.TypeParameterList);

        if (tds.BaseList != null)
            sb.Append($" {tds.BaseList}");

        if (tds.ConstraintClauses.Count > 0)
            sb.Append($" {tds.ConstraintClauses}");

        return sb.ToString();
    }

    public static string ToDecl(this IMethodSymbol symbol, Compilation comp)
    {
        var sb = new StringBuilder(128);
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        switch (syntax)
        {
            case MethodDeclarationSyntax mds:
                var attributes = mds.AttributeLists.PublicOnes(comp.GetSemanticModel(syntax.SyntaxTree));
                if (attributes.Length > 0)
                    sb.AppendLine($"[{string.Join(", ", attributes)}]");

                sb.Append($"{mds.Modifiers} {mds.ReturnType} {mds.Identifier}");

                if (mds.TypeParameterList != null)
                    sb.Append(mds.TypeParameterList);

                sb.Append(mds.ParameterList);

                if (mds.ConstraintClauses.Count > 0)
                    sb.Append($" {mds.ConstraintClauses}");

                return sb.ToString();

            case ConstructorDeclarationSyntax cds:
                sb.Append($"{cds.Modifiers} {cds.Identifier}{cds.ParameterList}");
                return sb.ToString();

            default:
                return "// No declaration found";
        }
    }

    public static string ToDecl(this IPropertySymbol symbol, Compilation comp)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        if (syntax is not PropertyDeclarationSyntax pds)
            return "// No declaration found";

        var sb = new StringBuilder(128);

        var attrs = pds.AttributeLists.PublicOnes(comp.GetSemanticModel(syntax.SyntaxTree));
        if (attrs.Length > 0)
            sb.AppendLine($"[{string.Join(", ", attrs)}]");

        sb.Append($"{pds.Modifiers} {pds.Type} {pds.Identifier}");

        if (symbol.IsReadOnly)
            sb.Append(" { get; }");
        else if (symbol.IsWriteOnly)
            sb.Append(" { set; }");
        else
            sb.Append(" { get; set; }");

        return sb.ToString();
    }

    public static string ToDecl(this IFieldSymbol symbol, Compilation comp)
    {
        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        switch (syntax)
        {
            case EnumMemberDeclarationSyntax mds:
            {
                var sb = new StringBuilder(128);

                var attrs = mds.AttributeLists.PublicOnes(comp.GetSemanticModel(syntax.SyntaxTree));
                if (attrs.Length > 0)
                    sb.AppendLine($"{attrs.Select(a => a.ToString()).CommaSeparated()}]");

                sb.AppendLine(mds.Identifier.ToString());
                if (mds.EqualsValue != null)
                    sb.Append($" {mds.EqualsValue}");

                return sb.ToString();
            }

            case FieldDeclarationSyntax fds:
            {
                var sb = new StringBuilder(128);

                var attrs = fds.AttributeLists.PublicOnes(comp.GetSemanticModel(syntax.SyntaxTree));
                if (attrs.Length > 0)
                    sb.AppendLine($"{attrs.Select(a => a.ToString()).CommaSeparated()}]");

                sb.Append($"{fds.Modifiers} {fds.Declaration}");

                return sb.ToString();
            }

            default:
                return "// No declaration found";
        }
    }

    private static ImmutableArray<AttributeSyntax> PublicOnes(this SyntaxList<AttributeListSyntax> lists, SemanticModel sm)
    {
        return lists
            .SelectMany(l => l.Attributes)
            .Where(a => sm.GetSymbolInfo(a.Name).Symbol?.ContainingType.IsPublic() ?? false)
            .ToImmutableArray();
    }
}