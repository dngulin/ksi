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

public partial class KsiCompGenerator
{
    private class ArchetypeTypeInfo(INamedTypeSymbol type)
    {
        public readonly string Type = type.FullTypeName();
        public readonly string Namespace = type.ContainingNamespace.FullyQualifiedName();
        public readonly List<string> Fields = new List<string>();
        public readonly List<string> Usings = new List<string>();
    }

    private static void GenerateKsiArchetypeExtensions(IncrementalGeneratorInitializationContext initCtx)
    {
        var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is StructDeclarationSyntax sds && sds.AttributeLists.ContainsKsiArchetype(),
            transform: (ctx, ct) =>
            {
                var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                if (t == null)
                    return null;

                var typeInfo = new ArchetypeTypeInfo(t);
                var usings = new HashSet<string>();

                foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
                {
                    if (f.IsStatic || f.DeclaredAccessibility == Accessibility.Private)
                        continue;

                    if (f.Type is not INamedTypeSymbol ft || !ft.IsRefListOfComponents())
                        continue;

                    typeInfo.Fields.Add(f.Name);
                    usings.Add(f.ContainingNamespace.FullyQualifiedName());
                }

                usings.Add(SymbolNames.Ksi);
                usings.Remove(typeInfo.Namespace);
                typeInfo.Usings.AddRange(usings.Where(u => u != ""));
                typeInfo.Usings.Sort();

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

                using (var file = AppendScope.Root(sb))
                {
                    foreach (var u in typeInfo.Usings)
                        file.AppendLine($"using {u};");

                    if (typeInfo.Usings.Count > 0)
                        file.AppendLine("");

                    using (var ns = file.OptNamespace(typeInfo.Namespace))
                    {
                        var fields = typeInfo.Fields;

                        var indent = new string(' ', AppendScope.Indent.Length * (ns.Depth + 2));
                        Func<string, string, string> indented = (a, b) => $"{a}\n{indent}{b}";

                        var handle = string.Format(
                            KsiCompTemplates.ArchetypeExtensions,
                            typeInfo.Type,
                            fields.Count == 0 ? "return 0;" : $"return self.{fields[0]}.Count();",
                            fields.Select(f => $"self.{f}.RefAdd();").Aggregate(indented),
                            fields.Select(f => $"self.{f}.RemoveAt(index);").Aggregate(indented),
                            fields.Select(f => $"self.{f}.Clear();").Aggregate(indented)
                        );
                        ns.AppendLine(handle);
                    }
                }

                ctx.AddSource($"{typeInfo.Type}.KsiArchetypeExtensions.g.cs", sb.ToString());
            }
        });
    }
}