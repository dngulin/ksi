using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Ksi.Roslyn.DeallocTemplates;

namespace Ksi.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class DeallocGenerator : IIncrementalGenerator
    {
        private class DeallocInfo(string typeName)
        {
            public readonly string TypeName = typeName;
            public string? Namespace;
            public CodeGenTraits Traits;
            public string[] Usings = [];
            public readonly List<string> Fields = new List<string>();
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

                    return structDecl.AttributeLists.ContainsDealloc();
                },
                transform: (ctx, ct) =>
                {
                    var c = (StructDeclarationSyntax)ctx.Node;

                    var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                    var result = new DeallocInfo(c.Identifier.ValueText);

                    if (t == null)
                        return result;

                    result.Namespace = t.ContainingNamespace.FullyQualifiedName();
                    result.Traits = t.GetCodeGenTraits();

                    var usings = new HashSet<string>();

                    foreach (var m in t.GetMembers())
                    {
                        if (m is not IFieldSymbol f || f.Type.TypeKind != TypeKind.Struct || f.IsStatic)
                            continue;

                        if (f.DeclaredAccessibility == Accessibility.Private)
                            continue;

                        if (f.Type is not INamedTypeSymbol ft || !ft.IsDeallocOrRefListOverDealloc())
                            continue;

                        if (ft.IsJaggedRefList())
                            continue;

                        result.Fields.Add(f.Name);
                        usings.Add(ft.ContainingNamespace.FullyQualifiedName());

                        if (ft.IsRefList() && ft.TryGetGenericArg(out var gt) && gt!.IsDealloc())
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
                        ctx.AddSource($"{entry.TypeName}Dealloc.g.cs", sb.ToString());
                        sb.Clear();
                        continue;
                    }

                    using (var file = AppendScope.Root(sb))
                    {
                        foreach (var u in entry.Usings)
                            file.AppendLine($"using {u};");

                        file.AppendLine("");

                        using (var ns = file.OptNamespace(entry.Namespace))
                        using (var cls = ns.PubStat($"class {entry.TypeName}Dealloc"))
                        {
                            EmitDeallocMethods(cls, entry);

                            var kinds = entry.Traits.ToRefListKinds();
                            kinds.Emit(cls, RefListDeallocItemsAndSelf, RefListDeallocOnlyItems, entry.TypeName);
                            kinds.Emit(cls, RefListDeallocated, entry.TypeName);
                            kinds.Emit(cls, RefListSpecialized, entry.TypeName);
                        }
                    }

                    ctx.AddSource($"{entry.TypeName}Dealloc.g.cs", sb.ToString());
                }
            });
        }

        private static void EmitDeallocMethods(in AppendScope cls, DeallocInfo entry)
        {
            var t = entry.TypeName;
            using (var m = cls.PubStat($"void Dealloc(this ref {t} self)"))
            {
                foreach (var f in entry.Fields)
                    m.AppendLine($"self.{f}.Dealloc();");
            }

            cls.AppendLine(string.Format(DeallocatedExtension, t));
        }
    }
}