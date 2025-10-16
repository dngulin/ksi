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
        private class DeallocInfo(Accessibility acc, INamedTypeSymbol t)
        {
            public readonly string Accessibility = SyntaxFacts.GetText(acc);
            public readonly string Type = t.FullTypeName();
            public readonly string Namespace = t.ContainingNamespace.FullyQualifiedName();
            public readonly CodeGenTraits Traits = t.GetCodeGenTraits();
            public readonly List<string> Usings = new List<string>(8);
            public readonly List<string> Fields = new List<string>(16);
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
                    var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                    if (t == null)
                        return null;

                    var acc = t.InAssemblyAccessibility();
                    if (acc < Accessibility.Internal)
                        return null;

                    var result = new DeallocInfo(acc, t);
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
                    result.Usings.AddRange(usings.Where(u => u != ""));
                    result.Usings.Sort();

                    return result;
                }
            );

            var collected = query.Collect();

            initCtx.RegisterSourceOutput(collected, (ctx, entries) =>
            {
                var sb = new StringBuilder(16 * 1024);

                foreach (var entry in entries)
                {
                    if (entry == null)
                        continue;

                    var t = entry.Type;
                    var acc = entry.Accessibility;

                    using (var file = AppendScope.Root(sb))
                    {
                        foreach (var u in entry.Usings)
                            file.AppendLine($"using {u};");

                        file.AppendLine("");

                        using (var ns = file.OptNamespace(entry.Namespace))
                        {
                            ns.AppendLine("/// <summary>");
                            ns.AppendLine($"/// Deallocation extensions for {t}.");
                            ns.AppendLine("/// </summary>");
                            using (var cls = ns.Sub($"{acc} static class {t.Replace('.', '_')}_Dealloc"))
                            {
                                EmitDeallocMethods(cls, entry);

                                var kinds = entry.Traits.ToRefListKinds();
                                kinds.Emit(cls, RefListDeallocItemsAndSelf, RefListDeallocOnlyItems, t);
                                kinds.Emit(cls, RefListDeallocated, t);
                                kinds.Emit(cls, RefListSpecialized, t);
                            }
                        }
                    }

                    ctx.AddSource($"{t}.Dealloc.g.cs", sb.ToString());
                }
            });
        }

        private static void EmitDeallocMethods(in AppendScope cls, DeallocInfo entry)
        {
            var t = entry.Type;
            cls.AppendLine("/// <summary>");
            cls.AppendLine("/// Deallocate all owned resources by the structure.");
            cls.AppendLine("/// </summary>");
            cls.AppendLine("""/// <param name="self">Structure to deallocate</param>""");
            using (var m = cls.PubStat($"void Dealloc(this ref {t} self)"))
            {
                foreach (var f in entry.Fields)
                    m.AppendLine($"self.{f}.Dealloc();");
            }

            cls.AppendLine(string.Format(DeallocatedExtension, t));
        }
    }
}