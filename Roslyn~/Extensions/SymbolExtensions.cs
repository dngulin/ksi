using Microsoft.CodeAnalysis;
using static Ksi.Roslyn.SymbolNames;

namespace Ksi.Roslyn.Extensions
{
    public static class SymbolExtensions
    {
        public static bool IsRefOrWrappedRef(this ILocalSymbol self) => self.IsRef || self.Type.IsWrappedRef();

        public static bool Is(this AttributeData attribute, string attributeName)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == attributeName + Suffix;
        }
    }
}