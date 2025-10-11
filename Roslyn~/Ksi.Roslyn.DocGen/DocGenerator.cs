using System.Collections.Immutable;
using Ksi.Roslyn.DocGen.Extensions;

namespace Ksi.Roslyn.DocGen;

public static class DocGenerator
{
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
        WriteIndexSection(writer, "Attributes", api.Attributes);
        WriteIndexSection(writer, "Collections", api.Collections);
        WriteIndexSection(writer, "Other Types", api.OtherTypes);
    }

    private static void WriteIndexSection(StreamWriter writer, string title, ImmutableArray<TypeSpec> types)
    {
        writer.WriteLine("\n\n## " + title);

        writer.WriteLine();
        foreach (var t in types.Where(t => !t.Symbol.IsStatic || !t.IsEmpty))
            writer.WriteLine($"- [{t.Symbol.ToMd()}]({t.FileName()})");
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
        using var writer = new StreamWriter(path + "/" + t.FileName());

        writer.WriteLine("# " + t.Symbol.ToMd());

        writer.WriteLine();
        writer.WriteLine(t.Summary);

        writer.WriteLine();
        writer.WriteLine(t.Declaration);

        WriteMethods(writer, t.Constructors, "Constructors");
        WriteMethods(writer, t.ExternalConstructors, "Static Creation Methods");
        WriteProperties(writer, t.Properties);
        WriteMethods(writer, t.Methods, "Methods");
        WriteMethods(writer, t.StaticMethods, "Static Methods");
        WriteMethods(writer, t.ExternalMethods, "Extension Methods");
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
        writer.WriteLine("\n\n### " + m.Symbol.ToMd());

        writer.WriteLine();
        writer.WriteLine(m.Summary);

        writer.WriteLine();
        writer.WriteLine(m.Declaration);

        if (m.Parameters.Length > 0)
        {
            writer.WriteLine("\nParameters");
            foreach (var p in m.Parameters)
                writer.WriteLine($"- {p}");
        }

        if (m.Returns != null)
            writer.WriteLine("\nReturns " + m.Returns);
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
        writer.WriteLine("\n\n### " + p.Symbol.Name);

        writer.WriteLine();
        writer.WriteLine(p.Summary);

        writer.WriteLine();
        writer.WriteLine(p.Declaration);
    }
}