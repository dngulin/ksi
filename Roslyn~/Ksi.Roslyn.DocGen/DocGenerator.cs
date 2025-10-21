using System.Collections.Immutable;
using Ksi.Roslyn.DocGen.Extensions;

namespace Ksi.Roslyn.DocGen;

public static class DocGenerator
{
    private const string Navbar =
        """
        
        > \[ [Traits](../traits.md)
        > \| [Collections](../collections.md)
        > \| [Referencing](../borrow-checker-at-home.md)
        > \| [ECS](../ecs.md)
        > \| {0}
        > \]
        """;

    public static void WriteToDirectory(string path, PublicApi api)
    {
        RemoveGeneratedFiles(path);
        WriteIndex(path, api);
        WriteTypes(path, api.Attributes);
        WriteTypes(path, api.Collections);
        WriteTypes(path, api.OtherTypes);
    }

    private static void RemoveGeneratedFiles(string path)
    {
        foreach (var f in Directory.GetFiles(path, "*.g.md", SearchOption.TopDirectoryOnly))
            File.Delete(f);
    }

    private static void WriteIndex(string path, PublicApi api)
    {
        using var writer = new StreamWriter(path + "/index.g.md");

        writer.WriteLine("# API Reference");
        writer.WriteLine(Navbar, "**API**");

        writer.WriteLine();
        writer.WriteLine("The API Reference is generated from XML documentation comments.\n");
        writer.WriteLine("Extension methods are grouped together with their target types.");

        WriteIndexSection(writer, "Attributes", api.Attributes);
        WriteIndexSection(writer, "Collections", api.Collections);
        WriteIndexSection(writer, "Other Types", api.OtherTypes);
    }

    private static void WriteIndexSection(StreamWriter writer, string title, ImmutableArray<TypeSpec> types)
    {
        writer.WriteLine("\n\n## " + title);

        writer.WriteLine();
        foreach (var t in types.Where(t => !t.Symbol.IsStatic || !t.IsEmpty))
            writer.WriteLine($"- [{t.Symbol.ToMd()}]({t.Symbol.MdFileName()})");
    }

    private static void WriteTypes(string path, ImmutableArray<TypeSpec> types)
    {
        foreach (var t in types)
        {
            if (!t.Symbol.IsStatic || !t.IsEmpty)
                WriteType(path, t);
        }
    }

    private static void WriteType(string path, TypeSpec t)
    {
        using var writer = new StreamWriter(path + "/" + t.Symbol.MdFileName());

        writer.WriteLine("# " + t.Title);
        writer.WriteLine(Navbar, $"**[API](index.g.md) / {t.Title}**");

        Write(writer, t.Summary);
        Write(writer, t.Declaration);

        WriteToc(writer, t);

        WriteMethods(writer, t.Constructors, "Constructors");
        WriteMethods(writer, t.ExternalConstructors, "Static Creation Methods");
        WriteProperties(writer, t.Properties);
        WriteMethods(writer, t.Methods, "Methods");
        WriteMethods(writer, t.StaticMethods, "Static Methods");
        WriteMethods(writer, t.ExternalMethods, "Extension Methods");
    }

    private static void WriteToc(StreamWriter writer, TypeSpec t)
    {
        WriteTocSection(writer, t.Constructors.Select(x => (x.Title, x.Summary)).ToArray(), "Constructors");
        WriteTocSection(writer, t.ExternalConstructors.Select(x => (x.Title, x.Summary)).ToArray(), "Static Creation Methods");
        WriteTocSection(writer, t.Properties.Select(x => (x.Title, x.Summary)).ToArray(), "Properties");
        WriteTocSection(writer, t.Methods.Select(x => (x.Title, x.Summary)).ToArray(), "Methods");
        WriteTocSection(writer, t.StaticMethods.Select(x => (x.Title, x.Summary)).ToArray(), "Static Methods");
        WriteTocSection(writer, t.ExternalMethods.Select(x => (x.Title, x.Summary)).ToArray(), "Extension Methods");
    }

    private static void WriteTocSection(StreamWriter writer, (string Caption, string Desc)[] entries, string title)
    {
        if (entries.Length == 0)
            return;

        writer.WriteLine();
        writer.WriteLine(title);
        foreach (var (caption, desc) in entries)
        {
            var brief = desc.TrimEnd('.').Replace('\n', ' ').Split(". ").First().Decapitalize();
            var frag = caption.ToMdFragment();
            writer.WriteLine($"- [{caption}]({frag}) — {brief}");
        }
    }

    private static void WriteMethods(StreamWriter writer, IReadOnlyList<MethodSpec> methods, string title)
    {
        if (methods.Count == 0)
            return;

        writer.WriteLine("\n\n## " + title);
        foreach (var m in methods)
            WriteMethod(writer, m);
    }

    private static void WriteMethod(StreamWriter writer, MethodSpec m)
    {
        writer.WriteLine("\n\n### " + m.Title);

        Write(writer, m.Summary);
        Write(writer, m.Declaration);
        Write(writer, m.Parameters, "Parameters", "- ");
        Write(writer, m.Returns?.Decapitalize(), "Returns ");
        Write(writer, m.Exceptions, "> [!CAUTION]\n> Possible exceptions: ", "> - ");
    }

    private static void WriteProperties(StreamWriter writer, IReadOnlyList<PropertySpec> properties)
    {
        if (properties.Count == 0)
            return;

        writer.WriteLine("\n\n## Properties");
        foreach (var p in properties)
            WriteProperty(writer, p);
    }

    private static void WriteProperty(StreamWriter writer, PropertySpec p)
    {
        writer.WriteLine("\n\n### " + p.Title);

        Write(writer, p.Summary);
        Write(writer, p.Declaration);
        Write(writer, p.Exceptions, "> [!CAUTION]\n> Possible exceptions: ", "> - ");
    }

    private static void Write(StreamWriter writer, string text)
    {
        writer.WriteLine();
        writer.WriteLine(text);
    }

    private static void Write(StreamWriter writer, string? text, string prefix)
    {
        if (text == null)
            return;

        writer.WriteLine();
        writer.Write(prefix);
        writer.WriteLine(text);
    }

    private static void Write(StreamWriter writer, ImmutableArray<string> texts, string prefix, string itemPrefix)
    {
        if (texts.Length == 0)
            return;

        writer.WriteLine();
        writer.WriteLine(prefix);

        foreach (var text in texts)
        {
            writer.Write(itemPrefix);
            writer.WriteLine(text);
        }
    }
}