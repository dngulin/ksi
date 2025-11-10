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
        INamedTypeSymbol slotType,
        INamedTypeSymbol keyType,
        bool passKeyByRef,
        INamedTypeSymbol? valueType)
    {
        public readonly INamedTypeSymbol Type = t;
        public readonly INamedTypeSymbol SlotType = slotType;
        public readonly INamedTypeSymbol KeyType = keyType;
        public readonly bool PassKeyByRef = passKeyByRef;
        public readonly INamedTypeSymbol? ValueType = valueType;
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

                if (!KsiHashAnalyzer.IsValidTable(t, sds, out var tSlot, out var passKeyByRef) || tSlot == null)
                    return null;

                if (!KsiHashAnalyzer.IsValidSlot(tSlot, out var tKey, out var tValue) || tKey == null)
                    return null;

                return new HashTableInfo(t, tSlot, tKey, passKeyByRef, tValue);
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
                        if (typeInfo.ValueType == null)
                        {
                            WriteHashSet(ns, typeInfo);
                        }
                        else
                        {
                            WriteHashMap(ns, typeInfo);
                        }
                    }
                }

                ctx.AddSource($"{typeInfo.Type.Name}.KsiHashTable.g.cs", sb.ToString());
            }
        });
    }

    private static void AddUsings(AppendScope file, HashTableInfo h)
    {
        var namespaces = new HashSet<string> { "Ksi" };

        if (!h.SlotType.IsGloballyVisible())
            namespaces.Add(h.SlotType.ContainingNamespace.FullyQualifiedName());

        if (!h.KeyType.IsGloballyVisible())
            namespaces.Add(h.KeyType.ContainingNamespace.FullyQualifiedName());

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
        var accessibility = SyntaxFacts.GetText(h.Type.InAssemblyAccessibility());
        var keyIsExpCopy = h.KeyType.IsExplicitCopy();
        var code = KsiHashTemplates.HashSetApi
            .ToStringBuilder()
            .Replace("|accessibility|", accessibility)
            .Replace("|THashSet|", h.Type.Name)
            .Replace("|TKey|", h.KeyType.FullTypeName())
            .Unwrap("[in ]", h.PassKeyByRef)
            .Unwrap("[in `insertion]", h.PassKeyByRef && !keyIsExpCopy)
            .Unwrap("[.Move()]", keyIsExpCopy)
            .Unwrap("[key.Dealloc();\n                    ]", h.KeyType.IsDeallocOrRefListOverDealloc())
            .Unwrap("[.Deallocated()`self]", h.Type.IsDealloc())
            .Unwrap("[.Deallocated()`slot]", h.SlotType.IsDealloc())
            .ToString();
        ns.AppendLine(code);
    }

    private static void WriteHashMap(AppendScope ns, HashTableInfo h)
    {
        var accessibility = SyntaxFacts.GetText(h.Type.InAssemblyAccessibility());
        var keyIsExpCopy = h.KeyType.IsExplicitCopy();
        var code = KsiHashTemplates.HashMapApi
            .ToStringBuilder()
            .Replace("|accessibility|", accessibility)
            .Replace("|THashMap|", h.Type.Name)
            .Replace("|TKey|", h.KeyType.FullTypeName())
            .Replace("|TValue|", h.ValueType!.FullTypeName())
            .Replace("|RefPathSuffix|", GetRefPathSuffix(h))
            .Unwrap("[in ]", h.PassKeyByRef)
            .Unwrap("[in `insertion]", h.PassKeyByRef && !keyIsExpCopy)
            .Unwrap("[.Move()`key]", keyIsExpCopy)
            .Unwrap("[.Move()`value]", h.ValueType!.IsExplicitCopy())
            .Unwrap("[key.Dealloc();\n                    ]", h.KeyType.IsDeallocOrRefListOverDealloc())
            .Unwrap("[.Deallocated()`self]", h.Type.IsDealloc())
            .Unwrap("[.Deallocated()`slot]", h.SlotType.IsDealloc())
            .ToString();
        ns.AppendLine(code);
    }

    private static string GetRefPathSuffix(HashTableInfo h)
    {
        if (h.ValueType!.IsDynSized())
            return """
                   "[n]", "Value", "!"
                   """;

        if (h.SlotType.IsDynSized())
            return """
                   "[n]", "!", "Value"
                   """;

        return """
               "!", "[n]", "Value"
               """;
    }
}