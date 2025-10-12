using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class SymbolToMdExtensions
{
    private static readonly SymbolDisplayFormat DefaultFormat = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType,
        propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
        kindOptions: SymbolDisplayKindOptions.None,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                              SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix
    );

    private static readonly SymbolDisplayFormat StaticFormat = DefaultFormat
        .WithMemberOptions(
            SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType
        );

    public static string ToMd(this ITypeSymbol self) => self.ToDisplayString(DefaultFormat).ToMd();

    public static string ToMd(this IMethodSymbol self)
    {
        if (self.IsExtensionMethod)
        {
            var parameters = self.Parameters;
            var first = parameters.First().ToDisplayString(StaticFormat);
            var rest = parameters.Length == 1
                ? ""
                : parameters.Skip(1).Select(p => p.ToDisplayString(StaticFormat)).Aggregate((a, b) => $"{a}, {b}");

            return $"({first}).{self.Name}({rest})".ToMd();
        }

        var format = self.IsStatic ? StaticFormat : DefaultFormat;
        return self.ToDisplayString(format).ToMd();
    }
}