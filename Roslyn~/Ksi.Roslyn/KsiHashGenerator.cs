using System.Text;
using Ksi.Roslyn.Extensions;
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

                if (typeInfo.TValue == null)
                {
                    // OutHashSet
                }
                else
                {
                    // OutHashMap
                }

                sb.AppendLine("// FOO");
                ctx.AddSource($"{typeInfo.Type.MetadataName}.KsiHashTable.g.cs", sb.ToString());

                sb.Clear();
            }
        });
    }
}