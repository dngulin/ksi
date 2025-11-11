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
        private class ExpCopyTypeInfo(Accessibility acc, INamedTypeSymbol t)
        {
            public readonly string Accessibility = SyntaxFacts.GetText(acc);
            public readonly string Type = t.FullTypeName();
            public readonly string Namespace = t.ContainingNamespace.FullyQualifiedName();
            public readonly CodeGenTraits Traits = t.GetCodeGenTraits();
            public readonly bool IsDealloc = t.IsDealloc();
            public readonly List<string> Usings = new List<string>(8);
            public readonly List<(string, bool)> Fields = new List<(string, bool)>(16);
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
                    var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct);
                    if (t == null)
                        return null;

                    var acc = t.InAssemblyAccessibility();
                    if (acc < Accessibility.Internal)
                        return null;

                    var result = new ExpCopyTypeInfo(acc, t);
                    var usings = new HashSet<string>();

                    foreach (var m in t.GetMembers())
                    {
                        if (m is not IFieldSymbol f || f.IsStatic || f.IsPrivate())
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
                    result.Usings.AddRange(usings.Where(u => u != ""));
                    result.Usings.Sort();

                    return result;
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

                    var acc = typeInfo.Accessibility;
                    var t = typeInfo.Type;

                    using (var file = AppendScope.Root(sb))
                    {
                        foreach (var u in typeInfo.Usings)
                            file.AppendLine($"using {u};");

                        file.AppendLine("");

                        using (var ns = file.OptNamespace(typeInfo.Namespace))
                        {
                            ns.AppendLine("/// <summary>");
                            ns.AppendLine($"/// Explicit copy extensions for {t}.");
                            ns.AppendLine("/// </summary>");
                            using (var cls = ns.Sub($"{acc} static class {t.Replace('.', '_')}_ExplicitCopy"))
                            {
                                EmitExplicitCopyMethods(cls, typeInfo);

                                var template = typeInfo.IsDealloc ? RefListExtensionsForDeallocItems : RefListExtensions;
                                typeInfo.Traits.ToRefListKinds().Emit(cls, template, t);
                            }
                        }
                    }

                    ctx.AddSource($"{t}.ExplicitCopy.g.cs", sb.ToString());
                }
            });
        }

        private static void EmitExplicitCopyMethods(in AppendScope cls, ExpCopyTypeInfo entry)
        {
            var t = entry.Type;
            cls.AppendLine("/// <summary>");
            cls.AppendLine("/// Copies all structure fields from another using explicit copy extension methods.");
            cls.AppendLine("/// All items existing before copying are removed.");
            cls.AppendLine("/// </summary>");
            cls.AppendLine("""/// <param name="self">Destination structure</param>""");
            cls.AppendLine("""/// <param name="other">Source structure</param>""");
            using (var m = cls.PubStat($"void CopyFrom(this ref {t} self, in {t} other)"))
            {
                foreach (var (f, copy) in entry.Fields)
                    m.AppendLine(copy ? $"self.{f}.CopyFrom(other.{f});" : $"self.{f} = other.{f};");
            }

            cls.AppendLine("");
            cls.AppendLine("/// <summary>");
            cls.AppendLine("/// Copies all structure fields to another using explicit copy extension methods.");
            cls.AppendLine("/// All items existing before copying are removed.");
            cls.AppendLine("/// </summary>");
            cls.AppendLine("""/// <param name="self">Source structure</param>""");
            cls.AppendLine("""/// <param name="other">Destination structure</param>""");
            using (var m = cls.PubStat($"void CopyTo(this in {t} self, ref {t} other)"))
            {
                m.AppendLine("other.CopyFrom(self);");
            }
        }
    }
}