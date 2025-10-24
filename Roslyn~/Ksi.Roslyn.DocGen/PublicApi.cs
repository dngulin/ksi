using System.Collections.Immutable;
using Ksi.Roslyn.DocGen.Extensions;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen;

public class PublicApi(
    ImmutableArray<TypeSpec> attributes,
    ImmutableArray<TypeSpec> collections,
    ImmutableArray<TypeSpec> otherTypes)
{
    public readonly ImmutableArray<TypeSpec> Attributes = attributes;
    public readonly ImmutableArray<TypeSpec> Collections = collections;
    public readonly ImmutableArray<TypeSpec> OtherTypes = otherTypes;

    public static PublicApi Gather()
    {
        var attributes = new List<TypeSpec>(16);
        var collections = new List<TypeSpec>(3);
        var otherTypes = new List<TypeSpec>(32);

        // Get "Ksi" namespace
        var compilation = KsiCompilation.Create();
        var globalNs = compilation.Assembly.GlobalNamespace;
        var ksiNs = globalNs.GetNamespaceMembers().First(ns => ns.Name == "Ksi");

        // Collect types
        foreach (var t in ksiNs.GetTypeMembers())
        {
            if (t.DeclaredAccessibility != Accessibility.Public)
                continue;

            var xml = t.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml))
                continue;

            var spec = new TypeSpec(t, compilation);

            if (t.Name.EndsWith("Attribute"))
                attributes.Add(spec);
            else if (t.IsRefList())
                collections.Add(spec);
            else
                otherTypes.Add(spec);
        }

        // Move extension methods and external constructors to collections
        foreach (var ts in otherTypes.Where(ts => ts.Symbol.IsStatic))
        {
            for (var i = ts.StaticMethods.Count - 1; i >= 0; i--)
            {
                if (collections.Any(c => c.TryAddExternalMethod(ts.StaticMethods[i])))
                    ts.StaticMethods.RemoveAt(i);
            }
        }

        foreach (var c in collections)
        {
            Comparison<MethodSpec> mCmp = static (a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal);
            c.ExternalConstructors.Sort(mCmp);
            c.ExternalMethods.Sort(mCmp);
        }


        Comparison<TypeSpec> tCmp = static (a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal);
        attributes.Sort(tCmp);
        otherTypes.Sort(tCmp);

        return new PublicApi(
            attributes.ToImmutableArray(),
            collections.ToImmutableArray(),
            otherTypes.ToImmutableArray()
        );
    }
}