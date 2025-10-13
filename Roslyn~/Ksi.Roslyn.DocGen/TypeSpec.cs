using System.Collections.Immutable;
using Ksi.Roslyn.DocGen.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen;

public sealed class TypeSpec
{
    public readonly INamedTypeSymbol Symbol;

    public readonly ImmutableArray<MethodSpec> Constructors;
    public readonly ImmutableArray<PropertySpec> Properties;
    public readonly ImmutableArray<MethodSpec> Methods;

    // Extensions and static constructors can be moved to another type
    public List<MethodSpec> StaticMethods { get; } = new List<MethodSpec>(16);

    // Can be received from another type
    public List<MethodSpec> ExternalConstructors { get; } = new List<MethodSpec>(8);
    public List<MethodSpec> ExternalMethods { get; } = new List<MethodSpec>(64);

    public bool IsEmpty => Constructors.IsEmpty &&
                           Properties.IsEmpty &&
                           Methods.IsEmpty &&
                           StaticMethods.Count == 0 &&
                           ExternalConstructors.Count == 0 &&
                           ExternalMethods.Count == 0;

    public readonly string Title;
    public readonly string Declaration;
    public readonly string Summary;

    public TypeSpec(INamedTypeSymbol symbol, Compilation comp)
    {
        Symbol = symbol;

        Constructors = symbol.Constructors
            .Where(m => m is { DeclaredAccessibility: Accessibility.Public, IsImplicitlyDeclared: false })
            .Select(m => new MethodSpec(m, comp))
            .ToImmutableArray();

        Properties = symbol.GetMembers()
            .Where(m => m is { DeclaredAccessibility: Accessibility.Public, IsImplicitlyDeclared: false })
            .OfType<IPropertySymbol>()
            .Select(p => new PropertySpec(p, comp))
            .ToImmutableArray();

        var methods = new List<MethodSpec>();
        foreach (var m in symbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (m.DeclaredAccessibility != Accessibility.Public || m.IsImplicitlyDeclared || m.MethodKind != MethodKind.Ordinary)
                continue;

            if (!m.IsStatic)
                methods.Add(new MethodSpec(m, comp));
            else
                StaticMethods.Add(new MethodSpec(m, comp));
        }

        Methods = methods.ToImmutableArray();

        Title = symbol.ToMd();
        Declaration = $"```csharp\n{symbol.ToDecl(comp)}\n```";
        Summary = symbol.DocXml().ToMd("summary", comp)!;
    }
}

public sealed class MethodSpec
{
    public readonly IMethodSymbol Symbol;
    public readonly string Title;

    public readonly string Declaration;
    public readonly string Summary;
    public readonly ImmutableArray<string> Parameters;
    public readonly string? Returns;
    public readonly ImmutableArray<string> Exceptions;

    public MethodSpec(IMethodSymbol symbol, Compilation comp)
    {
        Symbol = symbol;

        Title = symbol.ToMd();
        Declaration = $"```csharp\n{symbol.ToDecl(comp)}\n```";

        var xml = symbol.DocXml();
        Summary = xml.ToMd("summary", comp)!;
        Parameters = xml.ManyToMd("param", comp).ToImmutableArray();
        Returns = xml.ToMd("returns", comp);
        Exceptions = xml.ManyToMd("exception", comp).ToImmutableArray();
    }
}

public sealed class PropertySpec
{
    public readonly IPropertySymbol Symbol;
    public readonly string Title;

    public readonly string Declaration;
    public readonly string Summary;
    public readonly ImmutableArray<string> Exceptions;

    public PropertySpec(IPropertySymbol symbol, Compilation comp)
    {
        Symbol = symbol;
        Title = symbol.Name;
        Declaration = $"```csharp\n{symbol.ToDecl(comp)}\n```";

        var xml = symbol.DocXml();
        Summary = xml.ToMd("summary", comp)!;
        Exceptions = xml.ManyToMd("exception", comp).ToImmutableArray();
    }
}