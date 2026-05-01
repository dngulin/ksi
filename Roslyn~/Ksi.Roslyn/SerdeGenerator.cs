using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn;

[Generator(LanguageNames.CSharp)]
public class SerdeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initCtx)
    {
        var query = initCtx.SyntaxProvider
            .CreateSyntaxProvider<(INamedTypeSymbol Type, (IFieldSymbol Field, byte Id)[] Fields)?>(
                predicate: (node, _) =>
                {
                    if (node is not StructDeclarationSyntax s)
                        return false;

                    return s.AttributeLists.ContainsKsiSerializable();
                },
                transform: (ctx, ct) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ct) is not {} t || t.TypeParameters.Length != 0)
                        return null;

                    var fields = t.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Where(f => !f.IsStatic)
                        .Select(f => (Field: f, Attr: f.GetAttributes().FirstOrDefault(a => a.Is(SymbolNames.KsiSerializeField))))
                        .Where(x => x.Attr is { ConstructorArguments.Length: 1 })
                        .Select(x => (x.Field, Id: (byte)x.Attr!.ConstructorArguments[0].Value!))
                        .OrderBy(x => x.Id)
                        .ToArray();

                    return (Type: t, Fields: fields);
                }
            ).Where(x => x.HasValue).Select((x, _) => x!.Value);

        initCtx.RegisterSourceOutput(query, (ctx, info) =>
        {
            var (t, fields) = info;
            var sb = new StringBuilder(16 * 1024);

            using (var file = AppendScope.Root(sb))
            {
                file.AppendLine("using Ksi;");
                file.AppendLine("using Ksi.Serialization;");
                file.AppendLine("");

                using (var ns = file.OptNamespace(t.ContainingNamespace.FullyQualifiedName()))
                using (var type = ns.PubStat($"class {t.FullTypeName().Replace(".", "_")}_SerdeExtensions"))
                using (var method = type.PubStat($"int GetSerializedSize(this in {t.FullTypeName()} self)"))
                {
                    method.AppendLine("const int fieldIdLen = sizeof(byte);");
                    method.AppendLine("var result = 0;");
                    method.AppendLine("");

                    const string sizeOf = "KsiSerializedSize";

                    foreach (var (f, id) in fields)
                    {
                        if (f.Type is not INamedTypeSymbol ft)
                            continue;

                        if (ft.IsSerializablePrimitive()) // Primitive
                        {
                            method.AppendLine($"// {id:d3}: {f.Name}");
                            using var fScope = method.Sub($"if (self.{f.Name} != default)");
                            fScope.AppendLine($"result += {sizeOf}.Primitive(sizeof({ft.FullTypeName()}));");
                        }
                        else if (ft.IsKsiSerializable()) // Struct
                        {
                            method.AppendLine($"// {id:d3}: {f.Name}");
                            using var fScope = method.Sub();
                            fScope.AppendLine($"var len = self.{f.Name}.GetSerializedSize();");

                            using var lenScope = fScope.Sub("if (len > 0)");
                            lenScope.AppendLine("result += fieldIdLen + len;");
                        }
                        else if (ft.IsRefList()) // Repeated primitive or struct
                        {
                            if (!ft.TryGetGenericArg(out var gt) || gt == null)
                                continue;

                            if (gt.IsSerializablePrimitive()) // Repeated primitive
                            {
                                method.AppendLine($"// {id:d3}: {f.Name}");
                                using var fScope = method.Sub();
                                fScope.AppendLine($"var count = self.{f.Name}.Count();");

                                using var countScope = fScope.Sub("if (count > 0)");
                                countScope.AppendLine($"result += fieldIdLen + {sizeOf}.RepeatedPrimitive(sizeof({gt.FullTypeName()}), count);");
                            }
                            else if (gt.IsKsiSerializable()) // Repeated struct
                            {
                                method.AppendLine($"// {id:d3}: {f.Name}");
                                using var fScope = method.Sub();
                                fScope.AppendLine("var len = 0;");
                                fScope.AppendLine($"var count = self.{f.Name}.Count();");
                                using var countScope = fScope.Sub("if (count > 0)");

                                using (var loop = countScope.Sub($"foreach (ref readonly var item in self.{f.Name}.RefReadonlyIter())"))
                                {
                                    loop.AppendLine("len += item.GetSerializedSize();");
                                }

                                countScope.AppendLine($"result += fieldIdLen + {sizeOf}.RepeatedStruct(len, count);");
                            }
                        }

                        method.AppendLine("");
                    }

                    method.AppendLine($"return {sizeOf}.Struct(result);");
                }
            }

            ctx.AddSource($"{t.MetadataName}.Serde.g.cs", sb.ToString());
        });
    }


}