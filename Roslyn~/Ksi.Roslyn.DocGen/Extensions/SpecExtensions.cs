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

    public static string Title(this TypeSpec self) =>
        self.Symbol.IsGenericType ? $@"{self.Symbol.Name}\<T\>" : self.Symbol.Name;

    public static string Title(this MethodSpec self)
    {
        var m = self.Symbol;
        var t = self.Symbol.ContainingType;

        switch (m.MethodKind)
        {
            case MethodKind.Constructor:
            {
                var prefix = t.IsGenericType ? $@"{t.Name}\<T\>" : t.Name;
                var suffix = m.Parameters.IsEmpty ?
                    "()" :
                    $"({string.Join(",", m.Parameters.Select(p => p.Type.ToString()))})";
                return prefix + suffix;
            }

            case MethodKind.Ordinary:
            {
                var prefix = m.IsGenericMethod ? $@"{m.Name}\<T\>" : m.Name;
                var suffix = m.Parameters.IsEmpty ?
                    "()" :
                    $"({string.Join(",", m.Parameters.Select(p => p.Type.Name))})";
                return prefix + suffix;
            }

            default:
                return self.Symbol.Name;
        }
    }
}