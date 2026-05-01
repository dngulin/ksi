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
                    method.AppendLine("const int fieldIdAndQualifierLen = fieldIdLen + ValueQualifier.PackedSize;");
                    method.AppendLine("var result = 0;");
                    method.AppendLine("");

                    foreach (var (f, _) in fields)
                    {
                        if (f.Type is not INamedTypeSymbol ft)
                            continue;

                        if (ft.IsSerializablePrimitive()) // Primitive
                        {
                            using var fScope = method.Sub($"if (self.{f.Name} != default)");
                            fScope.AppendLine($"result += fieldIdAndQualifierLen + sizeof({ft.FullTypeName()});");
                        }
                        else if (ft.IsKsiSerializable()) // Struct
                        {
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
                                using var fScope = method.Sub($"if (self.{f.Name}.Count() > 0)");
                                fScope.AppendLine($"var len = self.{f.Name}.Count() * sizeof({gt.FullTypeName()});");
                                fScope.AppendLine("var lenPfx = ValueQualifier.GetLenPrefix((uint) len);");
                                fScope.AppendLine("result += fieldIdLen + lenPfx.InBytes() + len;");
                            }
                            else if (gt.IsKsiSerializable()) // Repeated struct
                            {
                                using var fScope = method.Sub();
                                fScope.AppendLine("var len = 0;");
                                fScope.AppendLine("");

                                using (var loop = fScope.Sub($"foreach (ref readonly var item in self.{f.Name}.RefReadonlyIter())"))
                                {
                                    loop.AppendLine("len += item.GetSerializedSize();");
                                }

                                fScope.AppendLine("");
                                using var lenScope = fScope.Sub("if (len > 0)");
                                lenScope.AppendLine("var lenPfx = ValueQualifier.GetLenPrefix((uint) len);");
                                lenScope.AppendLine($"var cntPfx = ValueQualifier.GetLenPrefix((uint) self.{f.Name}.Count());");
                                lenScope.AppendLine("result += fieldIdLen + lenPfx.InBytes() + cntPfx.InBytes() + len;");
                            }
                        }

                        method.AppendLine("");
                    }

                    method.AppendLine("return ValueQualifier.PackedSize + ValueQualifier.GetLenPrefix((uint) result).InBytes() + result;");
                }
            }

            ctx.AddSource($"{t.MetadataName}.Serde.g.cs", sb.ToString());
        });
    }


}