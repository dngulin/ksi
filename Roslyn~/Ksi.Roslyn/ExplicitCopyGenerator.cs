using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Ksi.Roslyn.ExplicitCopyTemplates;

namespace Ksi.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class ExplicitCopyGenerator : IIncrementalGenerator
    {
        private class TypeInfo(string typeName)
        {
            public readonly string TypeName = typeName;
            public string? Namespace;
            public CodeGenTraits Traits;
            public bool IsDealloc;
            public string[] Usings = [];
            public readonly List<(string, bool)> Fields = new List<(string, bool)>();
        }

        public void Initialize(IncrementalGeneratorInitializationContext initCtx)
        {
            var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) =>
                {
                    if (node is not StructDeclarationSyntax structDecl)
                        return false;

                    if (structDecl.TypeParameterList != null)
                        return false;

                    return structDecl.AttributeLists.ContainsExplicitCopy();
                },
                transform: (ctx, ct) =>
                {
                    var c = (StructDeclarationSyntax)ctx.Node;

                    var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                    var result = new TypeInfo(c.Identifier.ValueText);

                    if (t == null)
                        return result;

                    result.Namespace = t.ContainingNamespace.FullyQualifiedName();
                    result.Traits = t.GetCodeGenTraits();
                    result.IsDealloc = t.IsDealloc();

                    var usings = new HashSet<string>();

                    foreach (var m in t.GetMembers())
                    {
                        if (m is not IFieldSymbol f || f.IsStatic || f.DeclaredAccessibility == Accessibility.Private)
                            continue;

                        if (f.Type is not INamedTypeSymbol ft || ft.IsJaggedRefList())
                            continue;

                        var isExplicitCopy = ft.IsExplicitCopy();
                        result.Fields.Add((f.Name, isExplicitCopy));

                        if (!isExplicitCopy)
                            continue;

                        usings.Add(ft.ContainingNamespace.FullyQualifiedName());

                        if (ft.IsRefList() && ft.TryGetGenericArg(out var gt) && gt!.IsExplicitCopy())
                            usings.Add(gt!.ContainingNamespace.FullyQualifiedName());
                    }

                    var kinds = result.Traits.ToRefListKinds();
                    if (kinds != RefListKinds.None)
                        usings.Add(SymbolNames.Ksi);

                    usings.Remove(result.Namespace);
                    result.Usings = usings.Where(u => u != "").ToArray();

                    Array.Sort(result.Usings);

                    return result;
                }
            );

            var collected = query.Collect();

            initCtx.RegisterSourceOutput(collected, (ctx, entries) =>
            {
                var sb = new StringBuilder(16 * 1024);

                foreach (var entry in entries)
                {
                    if (entry.Namespace == null)
                    {
                        sb.AppendLine($"#error Failed to get a declared symbol for the `{entry.TypeName}`");
                        ctx.AddSource($"{entry.TypeName}ExplicitCopy.g.cs", sb.ToString());
                        sb.Clear();
                        continue;
                    }

                    using (var file = AppendScope.Root(sb))
                    {
                        foreach (var u in entry.Usings)
                            file.AppendLine($"using {u};");

                        file.AppendLine("");

                        using (var ns = file.OptNamespace(entry.Namespace))
                        using (var cls = ns.PubStat($"class {entry.TypeName}ExplicitCopy"))
                        {
                            EmitExplicitCopyMethods(cls, entry);

                            var template = entry.IsDealloc ? RefListExtensionsForDeallocItems : RefListExtensions;
                            entry.Traits.ToRefListKinds().Emit(cls, template, entry.TypeName);
                        }
                    }

                    ctx.AddSource($"{entry.TypeName}ExplicitCopy.g.cs", sb.ToString());
                }
            });
        }

        private static void EmitExplicitCopyMethods(in AppendScope cls, TypeInfo entry)
        {
            var t = entry.TypeName;
            using (var m = cls.PubStat($"void CopyFrom(this ref {t} self, in {t} other)"))
            {
                foreach (var (f, copy) in entry.Fields)
                    m.AppendLine(copy ? $"self.{f}.CopyFrom(other.{f});" : $"self.{f} = other.{f};");
            }

            cls.AppendLine("");
            using (var m = cls.PubStat($"void CopyTo(this in {t} self, ref {t} other)"))
            {
                m.AppendLine("other.CopyFrom(self);");
            }
        }
    }
}