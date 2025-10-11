using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen.Extensions;

public static class SpecExtensions
{
    public static bool TryAddExternalMethod(this TypeSpec self, MethodSpec method)
    {
        var ts = self.Symbol;
        var ms = method.Symbol;

        if (!ms.IsStatic)
            return false;

        if (ms.IsExtensionMethod)
        {
            var p = ms.Parameters.First();
            if (p.Type is INamedTypeSymbol pt && pt.Eq(ts))
            {
                self.ExternalMethods.Add(method);
                return true;
            }

            return false;
        }

        if (ms is { ReturnsByRef: false, ReturnType: INamedTypeSymbol rt } && rt.Eq(ts))
        {
            self.ExternalConstructors.Add(method);
            return true;
        }

        return false;
    }

    private static bool Eq(this INamedTypeSymbol self, INamedTypeSymbol other)
    {
        if (self.IsGenericType != other.IsGenericType)
            return false;

        if (!self.IsGenericType)
            return SymbolEqualityComparer.Default.Equals(self, other);

        self = self.IsUnboundGenericType ? self : self.ConstructUnboundGenericType();
        other = other.IsUnboundGenericType ? other : other.ConstructUnboundGenericType();

        return SymbolEqualityComparer.Default.Equals(self, other);
    }

    public static string FileName(this TypeSpec self) => $"type-{self.Symbol.Name}.g.md";
}