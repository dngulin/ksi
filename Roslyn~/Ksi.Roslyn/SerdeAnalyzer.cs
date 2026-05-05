using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SerdeAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"SERDE{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01MissingSerializable = Rule(01, DiagnosticSeverity.Error,
        "Missing [KsiSerializable] attribute",
        "Type should be annotated with [KsiSerializable] because it contains [KsiSerializeField] field `{0}`"
    );

    private static readonly DiagnosticDescriptor Rule02DuplicateFieldId = Rule(02, DiagnosticSeverity.Error,
        "Duplicated [KsiSerializeField] id",
        "Field id `{0}` is duplicated in serializable type `{1}`"
    );

    private static readonly DiagnosticDescriptor Rule03InvalidFieldType = Rule(03, DiagnosticSeverity.Error,
        "Invalid [KsiSerializeField] type",
        "Serializable field type should be primitive, [KsiSerializable] or TRefList<TSerializable>"
    );

    private static readonly DiagnosticDescriptor Rule04LowTypeAccessibility = Rule(04, DiagnosticSeverity.Error,
        "Low [KsiSerializable] accessibility",
        "Declaring a [KsiSerializable] struct with accessibility lower than `internal` " +
        "prevents from generation extension methods"
    );

    private static readonly DiagnosticDescriptor Rule05LowFieldAccessibility = Rule(05, DiagnosticSeverity.Error,
        "Low [KsiSerializeField] accessibility",
        "[KsiSerializeField] should be visible to serialization extension methods: `internal` or `public`"
    );

    private static readonly DiagnosticDescriptor Rule06StaticField = Rule(06, DiagnosticSeverity.Error,
        "Static [KsiSerializeField]",
        "[KsiSerializeField] cannot be a static field"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        Rule01MissingSerializable,
        Rule02DuplicateFieldId,
        Rule03InvalidFieldType,
        Rule04LowTypeAccessibility,
        Rule05LowFieldAccessibility,
        Rule06StaticField
    );

    public override void Initialize(AnalysisContext ctx)
    {
        ctx.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        ctx.EnableConcurrentExecution();

        ctx.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        ctx.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var field = (IFieldSymbol)ctx.Symbol;
        if (!field.GetAttributes().Any(a => a.Is(SymbolNames.KsiSerializeField)))
            return;

        if (field.IsStatic)
            ctx.Report(field.Locations.First(), Rule06StaticField);

        if (!field.ContainingType.IsKsiSerializable())
            ctx.Report(field.ContainingType.Locations.First(), Rule01MissingSerializable, field.Name);

        if (!field.Type.IsSerializableType())
            ctx.Report(field.Locations.First(), Rule03InvalidFieldType);

        if (field.DeclaredAccessibility < Accessibility.Internal)
            ctx.Report(field.Locations.First(), Rule05LowFieldAccessibility);
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var syntax = (StructDeclarationSyntax)ctx.Node;
        if (!syntax.AttributeLists.ContainsKsiSerializable())
            return;

        var t = ctx.SemanticModel.GetDeclaredSymbol(syntax, ctx.CancellationToken);
        if (t is null)
            return;

        if (t.InAssemblyAccessibility() < Accessibility.Internal)
            ctx.Report(syntax.GetLocation(), Rule04LowTypeAccessibility);

        var ids = new Dictionary<byte, IFieldSymbol>();
        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            if (f.IsStatic)
                continue;

            var a = f.GetAttributes().FirstOrDefault(x => x.Is(SymbolNames.KsiSerializeField));
            if (a == null || a.ConstructorArguments.Length != 1)
                continue;

            if (a.ConstructorArguments[0].Value is not byte id)
                continue;

            if (ids.ContainsKey(id))
                ctx.Report(f.Locations.First(), Rule02DuplicateFieldId, id, t.Name);
            else
                ids.Add(id, f);
        }
    }
}
