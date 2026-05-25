using System.Collections.Immutable;
using System.Diagnostics;
using Ksi.Roslyn.DocGen.Extensions;
using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn.DocGen;

public class PublicApi(ImmutableArray<(string Category, ImmutableArray<TypeSpec>)> index)
{
    public readonly ImmutableArray<(string Category, ImmutableArray<TypeSpec>)> Index = index;

    private static class TypeNamePrefixes
    {
        public static readonly string[] Ecs = new[]
            { "KsiQuery", "KsiDomain", "KsiArchetype", "KsiEntity", "KsiComponent" };

        public static readonly string[] Hash = new[] { "KsiHash", "KsiPrime" };

        public static readonly string[] RefList = new[] { "RefList", "TempRefList", "ManagedRefList" };
    }


    public static PublicApi Gather()
    {
        var general = new List<TypeSpec>(16);
        var refList = new List<TypeSpec>(3);
        var hashTable = new List<TypeSpec>(32);
        var ecs = new List<TypeSpec>(32);
        var serialization = new List<TypeSpec>(32);

        // Get "Ksi" namespace
        var compilation = KsiCompilation.Create();
        var globalNs = compilation.Assembly.GlobalNamespace;
        var ksiNs = globalNs.GetNamespaceMembers().First(ns => ns.Name == "Ksi");
        var namespaces = new List<INamespaceSymbol> { ksiNs };
        namespaces.AddRange(ksiNs.GetNamespaceMembers());

        // Collect types
        foreach (var ns in namespaces)
        foreach (var t in ns.GetTypeMembers())
        {
            if (t.DeclaredAccessibility != Accessibility.Public)
                continue;

            var xml = t.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(xml))
                continue;

            var category = t switch
            {
                not null when t.ContainingNamespace.Name == "Serialization" => serialization,
                not null when t.Name.StartsWith("KsiSerial") => serialization,
                not null when TypeNamePrefixes.Ecs.Any(p => t.Name.StartsWith(p)) => ecs,
                not null when TypeNamePrefixes.Hash.Any(p => t.Name.StartsWith(p)) => hashTable,
                not null when TypeNamePrefixes.RefList.Any(p => t.Name.StartsWith(p)) => refList,
                not null => general,
                _ => throw new UnreachableException()
            };

            category.Add(new TypeSpec(t, compilation));
        }

        foreach (var category in new[] {refList, serialization})
            GroupConstructionAndExtensionMethods(category);

        foreach (var category in new[] {general, refList, hashTable, ecs, serialization})
            category.Sort(static (a, b) => a.SortingKey.CompareTo(b.SortingKey));

        var index = ImmutableArray.Create<(string Category, ImmutableArray<TypeSpec>)>(
            ("General", general.ToImmutableArray()),
            (@"TRefList\<T\> Variants", refList.ToImmutableArray()),
            ("Hash Tables", hashTable.ToImmutableArray()),
            ("Entity Component System", ecs.ToImmutableArray()),
            ("Serialization", serialization.ToImmutableArray())
        );

        return new PublicApi(index);
    }

    private static void GroupConstructionAndExtensionMethods(List<TypeSpec> category)
    {
        foreach (var ts in category.Where(ts => ts.Symbol.IsStatic))
        {
            for (var i = ts.StaticMethods.Count - 1; i >= 0; i--)
            {
                if (category.Any(c => c.TryAddExternalMethod(ts.StaticMethods[i])))
                    ts.StaticMethods.RemoveAt(i);
            }
        }

        foreach (var c in category)
        {
            Comparison<MethodSpec> mCmp = static (a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal);
            c.ConstructionMethods.Sort(mCmp);
            c.ExternalMethods.Sort(mCmp);
        }

        category.RemoveAll(static ts => ts.IsEmpty && !ts.Symbol.Name.EndsWith("Attribute"));
    }
}