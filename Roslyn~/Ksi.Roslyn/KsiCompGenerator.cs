using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class KsiCompGenerator : IIncrementalGenerator
    {
        private class TypeInfo(INamedTypeSymbol type)
        {
            public readonly string Type = type.FullTypeName();
            public readonly string Namespace = type.ContainingNamespace.FullyQualifiedName();
            public readonly List<string> Fields = new List<string>();
        }

        public void Initialize(IncrementalGeneratorInitializationContext initCtx)
        {
            var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) =>
                {
                    if (node is not StructDeclarationSyntax sds)
                        return false;

                    return sds.Modifiers.Any(SyntaxKind.PartialKeyword) && sds.AttributeLists.ContainsKsiDomain();
                },
                transform: (ctx, ct) =>
                {
                    var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                    if (t == null)
                        return null;

                    var typeInfo = new TypeInfo(t);
                    foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
                    {
                        if (f.IsStatic || f.DeclaredAccessibility == Accessibility.Private)
                            continue;

                        if (f.Type is not INamedTypeSymbol ft || !(ft.IsKsiArchetype() || ft.IsRefListOfEntities()))
                            continue;

                        typeInfo.Fields.Add(f.Name);
                    }

                    return typeInfo;
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

                    var t = typeInfo.Type;

                    using (var file = AppendScope.Root(sb))
                    using (var ns = file.OptNamespace(typeInfo.Namespace))
                    {
                        var items = typeInfo.Fields.Select((f, idx) => $"{f} = {idx + 1}");
                        var indent = new string(' ', AppendScope.Indent.Length * (ns.Depth + 2));
                        var handle = string.Format(KsiCompTemplates.KsiHandle, t, string.Join($",\n{indent}", items));
                        ns.AppendLine(handle);
                    }

                    ctx.AddSource($"{typeInfo.Type}.KsiHandle.g.cs", sb.ToString());
                }
            });
        }
    }
}