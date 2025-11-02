using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn;

[Generator(LanguageNames.CSharp)]
public class KsiHashGenerator : IIncrementalGenerator
{
    private class HashTableInfo(
        INamedTypeSymbol t,
        TypeDeclarationSyntax tds,
        INamedTypeSymbol tSlot,
        INamedTypeSymbol tKey,
        INamedTypeSymbol? tValue)
    {
        public readonly INamedTypeSymbol Type = t;
        public readonly string PartialTypeName = tds.PartialTypeName();

        public readonly INamedTypeSymbol TSlot = tSlot;
        public readonly INamedTypeSymbol TKey = tKey;
        public readonly INamedTypeSymbol? TValue = tValue;
    }

    public void Initialize(IncrementalGeneratorInitializationContext initCtx)
    {
        var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) =>
            {
                if (node is not StructDeclarationSyntax sds)
                    return false;

                return sds.Modifiers.Any(SyntaxKind.PartialKeyword) && sds.AttributeLists.ContainsKsiHashTable();
            },
            transform: (ctx, ct) =>
            {
                var sds = (StructDeclarationSyntax)ctx.Node;
                var t = ctx.SemanticModel.GetDeclaredSymbol(sds, ct);
                if (t == null)
                    return null;

                if (!KsiHashAnalyzer.IsValidTable(t, sds, out var tSlot) || tSlot == null)
                    return null;

                if (!KsiHashAnalyzer.IsValidSlot(tSlot, out var tKey, out var tValue) || tKey == null)
                    return null;

                return new HashTableInfo(t, sds, tSlot, tKey, tValue);
            }
        );

        var collected = query.Collect();
        initCtx.RegisterSourceOutput(collected, (ctx, typeInfos) =>
        {
            var sb = new StringBuilder(16 * 1024);

            foreach (var typeInfo in typeInfos)
            {
                if (typeInfo == null)
                    continue;

                using (var file = AppendScope.Root(sb))
                {
                    AddUsings(file, typeInfo);
                    using (var ns = file.OptNamespace(typeInfo.Type.ContainingNamespace.FullyQualifiedName()))
                    {
                        if (typeInfo.TValue == null)
                        {
                            WriteHashSet(ns, typeInfo);
                        }
                        else
                        {
                            // OutHashMap
                        }
                    }
                }

                ctx.AddSource($"{typeInfo.Type.Name}.KsiHashTable.g.cs", sb.ToString());
            }
        });
    }

    private static void AddUsings(AppendScope file, HashTableInfo h)
    {
        var namespaces = new HashSet<string>()
        {
            "Ksi",
            h.TSlot.ContainingNamespace.FullyQualifiedName(),
            h.TKey.ContainingNamespace.FullyQualifiedName()
        };

        namespaces.Remove("");
        namespaces.Remove(h.Type.ContainingNamespace.FullyQualifiedName());

        if (namespaces.Count == 0)
            return;

        var usings = namespaces.ToArray();
        Array.Sort(usings);

        foreach (var u in usings)
            file.AppendLine($"using {u};");

        file.AppendLine("");
    }

    private static void WriteHashSet(AppendScope ns, HashTableInfo h)
    {
        var acc = SyntaxFacts.GetText(h.Type.InAssemblyAccessibility());
        var dealloc = h.TSlot.IsDealloc() ? ".Deallocated()" : "";
        var t = h.Type.Name;
        var tKey = h.TKey.FullTypeName();
        ns.AppendLine(string.Format(KsiHashTemplates.HashSetApi, acc, t, tKey, dealloc));
    }
}