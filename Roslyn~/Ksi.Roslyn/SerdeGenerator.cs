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
                    var syn = (StructDeclarationSyntax)ctx.Node;
                    if (ctx.SemanticModel.GetDeclaredSymbol(syn, ct) is not { } t || t.TypeParameters.Length != 0)
                        return null;

                    const string fAttr = SymbolNames.KsiSerializeField;
                    var fields = t.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Where(f => !f.IsStatic)
                        .Select(f => (Field: f, Attr: f.GetAttributes().FirstOrDefault(a => a.Is(fAttr))))
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
            var acc = SyntaxFacts.GetText(t.InAssemblyAccessibility());

            var sb = new StringBuilder(16 * 1024);

            using (var file = AppendScope.Root(sb))
            {
                file.AppendLine("using Ksi;");
                file.AppendLine("using Ksi.Serialization;");
                file.AppendLine("using System.Runtime.InteropServices;");
                file.AppendLine("");

                using (var ns = file.OptNamespace(t.ContainingNamespace.FullyQualifiedName()))
                {
                    var fullNameSafe = t.FullTypeName().Replace(".", "_");
                    using (var type = ns.Sub($"{acc} static class {fullNameSafe}_SerdeExtensions"))
                    {
                        EmitGetSize(type, t, fields);
                        type.AppendLine("");
                        EmitPrepend(type, t, fields);
                        type.AppendLine("");
                        EmitAppend(type, t);
                        type.AppendLine("");
                        EmitSerializeTo(type, t);
                        type.AppendLine("");
                        EmitInitializeFrom(type, t, fields);
                    }
                }
            }

            ctx.AddSource($"{t.MetadataName}.Serde.g.cs", sb.ToString());
        });
    }

    private static void EmitGetSize(AppendScope type, INamedTypeSymbol t, (IFieldSymbol Field, byte Id)[] fields)
    {
        using var method = type.PubStat($"int GetSerializedSize(this in {t.FullTypeName()} self)");
        method.AppendLine("const int fieldIdLen = sizeof(byte);");
        method.AppendLine("var size = 0;");
        method.AppendLine("");

        const string sizeOf = "PrefixedSizeOf";

        foreach (var (f, id) in fields)
        {
            if (f.Type is not INamedTypeSymbol ft)
                continue;

            method.AppendLine($"// {id:d3}: {f.Name}");

            if (ft.IsSerializablePrimitive()) // Primitive
            {
                method.AppendOneLineBlock(
                    $"if (self.{f.Name} != default)",
                    $"size += fieldIdLen + {sizeOf}.Primitive(sizeof({ft.FullTypeName()}));"
                );
            }
            else if (ft.IsKsiSerializable()) // Struct
            {
                using var fScope = method.Sub();
                fScope.AppendLine($"var len = self.{f.Name}.GetSerializedSize();");
                fScope.AppendOneLineBlock("if (len > 0)", "size += fieldIdLen + len;");
            }
            else if (ft.IsRefList()) // Repeated primitive or struct
            {
                if (!ft.TryGetGenericArg(out var gt) || gt == null)
                    continue;

                if (gt.IsSerializablePrimitive()) // Repeated primitive
                {
                    var count = $"self.{f.Name}.Count()";
                    var size = $"sizeof({gt.FullTypeName()})";
                    method.AppendOneLineBlock(
                        $"if ({count} > 0)",
                        $"size += fieldIdLen + {sizeOf}.RepeatedPrimitive({size}, {count});"
                    );
                }
                else if (gt.IsKsiSerializable()) // Repeated struct
                {
                    var count = $"self.{f.Name}.Count()";

                    using var countScope = method.Sub($"if ({count} > 0)");
                    countScope.AppendLine("var len = 0;");

                    countScope.AppendOneLineBlock(
                        $"foreach (ref readonly var item in self.{f.Name}.RefReadonlyIter())",
                        "len += item.GetSerializedSize();"
                    );

                    countScope.AppendLine("");
                    countScope.AppendLine($"size += fieldIdLen + {sizeOf}.RepeatedStruct(len, {count});");
                }
            }

            method.AppendLine("");
        }

        method.AppendLine($"return {sizeOf}.Struct(size);");
    }

    private static void EmitPrepend(AppendScope type, INamedTypeSymbol t, (IFieldSymbol Field, byte Id)[] fields)
    {
        using var method =
            type.PubStat($"void Prepend(this System.IO.BinaryWriter writer, in {t.FullTypeName()} value)");
        method.AppendLine("var initialPos = writer.BaseStream.Position;");
        method.AppendLine("");

        foreach (var (f, id) in fields.Reverse())
        {
            if (f.Type is not INamedTypeSymbol ft)
                continue;

            method.AppendLine($"// {id:d3}: {f.Name}");

            if (ft.IsSerializablePrimitive())
            {
                using var fScope = method.Sub($"if (value.{f.Name} != default)");

                var (pk, ps) = GetPrimitiveInfo(ft);
                var val = GetPrimitiveValueExpr(ft, $"value.{f.Name}");

                fScope.AppendLine($"writer.Prepend({val});");
                fScope.AppendLine($"writer.Prepend(ValueQualifier.Primitive(PrimitiveKind.{pk}, PrimitiveSize.{ps}).Packed());");
                fScope.AppendLine($"writer.Prepend((byte){id});");
            }
            else if (ft.IsKsiSerializable())
            {
                using var fScope = method.Sub();
                fScope.AppendLine("var contentPos = writer.BaseStream.Position;");
                fScope.AppendLine($"writer.Prepend(value.{f.Name});");
                fScope.AppendLine("var len = (uint)(contentPos - writer.BaseStream.Position);");

                using (var inner = fScope.Sub("if (len > 0)"))
                {
                    inner.AppendLine("writer.PrependLenPrefix(len, out var lps);");
                    inner.AppendLine("writer.Prepend(ValueQualifier.Struct(lps).Packed());");
                    inner.AppendLine($"writer.Prepend((byte){id});");
                }
            }
            else if (ft.IsRefList())
            {
                if (!ft.TryGetGenericArg(out var gt) || gt == null)
                    continue;

                var count = $"value.{f.Name}.Count()";

                if (gt.IsSerializablePrimitive())
                {
                    using var fScope = method.Sub($"if ({count} > 0)");
                    var (pk, ps) = GetPrimitiveInfo(gt);

                    fScope.AppendLine("var contentPos = writer.BaseStream.Position;");
                    fScope.AppendLine($"writer.Prepend({GetPrimitiveSpanExpr(gt, f.Name)});");
                    fScope.AppendLine("var len = (uint)(contentPos - writer.BaseStream.Position);");
                    fScope.AppendLine("writer.PrependLenPrefix(len, out var lps);");
                    fScope.AppendLine($"writer.Prepend(ValueQualifier.RepeatedPrimitive(PrimitiveKind.{pk}, PrimitiveSize.{ps}, lps).Packed());");
                    fScope.AppendLine($"writer.Prepend((byte){id});");
                }
                else if (gt.IsKsiSerializable())
                {
                    using var fScope = method.Sub($"if ({count} > 0)");
                    fScope.AppendLine("var contentPos = writer.BaseStream.Position;");
                    fScope.AppendOneLineBlock(
                        $"foreach (ref readonly var item in value.{f.Name}.RefReadonlyIterReversed())",
                        "writer.Prepend(item);"
                    );
                    fScope.AppendLine($"writer.PrependLenPrefix((uint){count}, out var cps);");
                    fScope.AppendLine("var len = (uint)(contentPos - writer.BaseStream.Position);");
                    fScope.AppendLine("writer.PrependLenPrefix(len, out var lps);");
                    fScope.AppendLine("writer.Prepend(ValueQualifier.RepeatedStruct(lps, cps).Packed());");
                    fScope.AppendLine($"writer.Prepend((byte){id});");
                }
            }

            method.AppendLine("");
        }

        method.AppendLine("var totalLen = (uint)(initialPos - writer.BaseStream.Position);");
        method.AppendLine("writer.PrependLenPrefix(totalLen, out var totalLps);");
        method.AppendLine("writer.Prepend(ValueQualifier.Struct(totalLps).Packed());");
    }

    private static void EmitAppend(AppendScope type, INamedTypeSymbol t)
    {
        using var method = type.PubStat($"void Append(this System.IO.BinaryWriter writer, in {t.FullTypeName()} value)");
        method.AppendLine("var size = value.GetSerializedSize();");
        method.AppendLine("var initialPos = writer.BaseStream.Position;");
        method.AppendLine("writer.BaseStream.Position += size;");
        method.AppendLine("writer.Prepend(value);");
        method.AppendLine("writer.BaseStream.Position = initialPos + size;");
    }

    private static void EmitSerializeTo(AppendScope type, INamedTypeSymbol t)
    {
        using var method = type.PubStat($"void SerializeTo(this in {t.FullTypeName()} value, System.IO.BinaryWriter writer)");
        method.AppendLine("writer.Append(value);");
    }

    private static void EmitInitializeFrom(AppendScope type, INamedTypeSymbol t, (IFieldSymbol Field, byte Id)[] fields)
    {
        using var method =
            type.PubStat($"void InitializeFrom(this ref {t.FullTypeName()} self, System.IO.BinaryReader reader)");

        method.AppendLine("var q = ValueQualifier.Unpack(reader.ReadByte());");
        method.AppendLine("var endPos = reader.BaseStream.Position + reader.ReadLenPrefix(q.LenPrefixSize);");
        method.AppendLine("");

        using var loop = method.Sub("while (reader.BaseStream.Position < endPos)");
        loop.AppendLine("var fieldId = reader.ReadByte();");
        loop.AppendLine("var fieldQ = ValueQualifier.Unpack(reader.ReadByte());");
        loop.AppendLine("");

        using var sw = loop.Sub("switch (fieldId)");
        foreach (var (f, id) in fields)
        {
            if (f.Type is not INamedTypeSymbol ft)
                continue;

            sw.AppendLine($"// {id:d3}: {f.Name}");
            using var cs = sw.Sub($"case {id}:");

            if (ft.IsSerializablePrimitive())
            {
                var readMethod = GetReadMethodName(ft);
                cs.AppendLine(ft.SpecialType == SpecialType.System_Boolean
                    ? $"self.{f.Name} = reader.{readMethod}() != 0;"
                    : $"self.{f.Name} = {GetReadCast(ft)}reader.{readMethod}();");
            }
            else if (ft.IsKsiSerializable())
            {
                cs.AppendLine($"self.{f.Name}.InitializeFrom(reader);");
            }
            else if (ft.IsRefList())
            {
                if (!ft.TryGetGenericArg(out var gt) || gt == null)
                {
                    cs.AppendLine("break;");
                    continue;
                }

                cs.AppendLine($"self.{f.Name}.Clear();");
                if (gt.IsSerializablePrimitive())
                {
                    cs.AppendLine("var fieldLen = reader.ReadLenPrefix(fieldQ.LenPrefixSize);");
                    cs.AppendLine($"var count = (int)(fieldLen / sizeof({gt.FullTypeName()}));");
                    cs.AppendLine($"self.{f.Name}.AppendDefault(count);");
                    cs.AppendLine($"reader.Read(MemoryMarshal.AsBytes(self.{f.Name}.AsSpan()));");
                }
                else if (gt.IsKsiSerializable())
                {
                    cs.AppendLine("var fieldLen = reader.ReadLenPrefix(fieldQ.LenPrefixSize);");
                    cs.AppendLine("var itemCount = (int)reader.ReadLenPrefix(fieldQ.ItemCountPrefixSize());");
                    cs.AppendLine($"self.{f.Name}.AppendDefault(itemCount);");
                    cs.AppendOneLineBlock(
                        $"foreach (ref var item in self.{f.Name}.RefIter())",
                        "item.InitializeFrom(reader);"
                    );
                }
            }

            cs.AppendLine("break;");
        }

        using var def = sw.Sub("default:");
        def.AppendLine("reader.Skip(fieldQ);");
        def.AppendLine("break;");
    }

    private static (string Kind, string Size) GetPrimitiveInfo(INamedTypeSymbol t)
    {
        if (t.TypeKind == TypeKind.Enum)
            return GetPrimitiveInfo(t.EnumUnderlyingType!);

        var kind = t.SpecialType switch
        {
            SpecialType.System_SByte
                or SpecialType.System_Int16
                or SpecialType.System_Int32
                or SpecialType.System_Int64 => "SignedInt",
            SpecialType.System_Boolean or SpecialType.System_Char
                or SpecialType.System_Byte
                or SpecialType.System_UInt16
                or SpecialType.System_UInt32
                or SpecialType.System_UInt64 => "UnsignedInt",
            SpecialType.System_Single or SpecialType.System_Double => "FloatPoint",
            _ => "Unknown"
        };

        var size = t.SpecialType switch
        {
            SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Boolean => "_8",
            SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Char => "_16",
            SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Single => "_32",
            SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Double => "_64",
            _ => "Unknown"
        };

        return (kind, size);
    }

    private static string GetPrimitiveValueExpr(ITypeSymbol t, string valueName)
    {
        if (t.TypeKind == TypeKind.Enum)
        {
            var ut = ((INamedTypeSymbol)t).EnumUnderlyingType!;
            return $"({ut.FullTypeName()}){valueName}";
        }

        return t.SpecialType switch
        {
            SpecialType.System_Boolean => $"{valueName} ? (byte)1 : (byte)0",
            SpecialType.System_Char => $"(ushort){valueName}",
            _ => valueName
        };
    }

    private static string GetPrimitiveSpanExpr(ITypeSymbol gt, string fieldName)
    {
        return gt.SpecialType == SpecialType.System_Byte ?
            $"value.{fieldName}.AsReadOnlySpan()" :
            $"MemoryMarshal.AsBytes(value.{fieldName}.AsReadOnlySpan())";
    }

    private static string GetReadMethodName(INamedTypeSymbol t)
    {
        if (t.TypeKind == TypeKind.Enum)
            return GetReadMethodName(t.EnumUnderlyingType!);

        return t.SpecialType switch
        {
            SpecialType.System_Boolean => "ReadByte",
            SpecialType.System_Char => "ReadUInt16",
            SpecialType.System_SByte => "ReadSByte",
            SpecialType.System_Byte => "ReadByte",
            SpecialType.System_Int16 => "ReadInt16",
            SpecialType.System_UInt16 => "ReadUInt16",
            SpecialType.System_Int32 => "ReadInt32",
            SpecialType.System_UInt32 => "ReadUInt32",
            SpecialType.System_Int64 => "ReadInt64",
            SpecialType.System_UInt64 => "ReadUInt64",
            SpecialType.System_Single => "ReadSingle",
            SpecialType.System_Double => "ReadDouble",
            _ => "ReadUnknown"
        };
    }

    private static string GetReadCast(INamedTypeSymbol t)
    {
        if (t.TypeKind == TypeKind.Enum)
            return $"({t.FullTypeName()})";

        return t.SpecialType == SpecialType.System_Char ? "(char)" : "";
    }
}