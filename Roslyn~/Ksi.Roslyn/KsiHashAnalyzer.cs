using System;
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
public class KsiHashAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"KSIHASH{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01MissingSymbol = Rule(01, DiagnosticSeverity.Error,
        "Missing symbol",
        "Type is marked with {0} and should declare the {1}"
    );

    private static readonly DiagnosticDescriptor Rule02InvalidField = Rule(02, DiagnosticSeverity.Error,
        "Invalid field",
        "Type is marked with {0} and shouldn't declare the {1} field"
    );

    private static readonly DiagnosticDescriptor Rule03InvalidSymbolSignature = Rule(03, DiagnosticSeverity.Error,
        "Invalid symbol signature",
        "The {0} has a wrong signature. It should be a {1}"
    );

    private static readonly DiagnosticDescriptor Rule04InvalidAccessibility = Rule(04, DiagnosticSeverity.Error,
        "Invalid accessibility",
        "Accessibility of the {0} is to low. It should be at least internal"
    );

    private static readonly DiagnosticDescriptor Rule05InvalidHashTableDecl = Rule(05, DiagnosticSeverity.Error,
        "Invalid KsiHashTable declaration",
        "KsiHashTable type should be a top-level partial struct"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01MissingSymbol,
        Rule02InvalidField,
        Rule03InvalidSymbolSignature,
        Rule04InvalidAccessibility,
        Rule05InvalidHashTableDecl
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
    {
        var sds = (StructDeclarationSyntax)ctx.Node;
        var attrs = sds.AttributeLists;

        if (attrs.ContainsKsiHashTableSlot())
            AnalyzeHashTableSlot(ctx, sds);

        if (attrs.ContainsKsiHashTable())
            AnalyzeHashTable(ctx, sds);
    }

    [Flags]
    private enum SlotFields
    {
        None = 0,
        State = 1 << 0,
        Key = 1 << 1,
        Value = 1 << 2,
    }

    private static void AnalyzeHashTableSlot(SyntaxNodeAnalysisContext ctx, StructDeclarationSyntax sds)
    {
        var t = ctx.SemanticModel.GetDeclaredSymbol(sds, ctx.CancellationToken);
        if (t == null)
            return;

        var loc = sds.Identifier.GetLocation();
        if (t.InAssemblyAccessibility() < Accessibility.Internal)
            ctx.Report(loc, Rule04InvalidAccessibility, $"{t.Name} struct");

        var fields = SlotFields.None;
        const string attr = SymbolNames.KsiHashTableSlot;

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            var fl = f.Locations.First();

            if (f is { DeclaredAccessibility: < Accessibility.Internal, Name: "State" or "Key" or "Value" })
                ctx.Report(fl, Rule04InvalidAccessibility, $"{f.Name} field");

            var invSym = Rule03InvalidSymbolSignature;
            switch (f.Name)
            {
                case "State":
                    fields |= SlotFields.State;
                    if (f.IsStatic || !f.Type.IsKsiHastTableSlotState())
                        ctx.Report(fl, invSym, "State field", "non-static field of KsiHastTableSlotState type");
                    break;

                case "Key":
                    fields |= SlotFields.Key;
                    if (f.IsStatic || !f.Type.IsStruct())
                        ctx.Report(fl, invSym, "Key field", "non-static value type field");
                    break;

                case "Value":
                    fields |= SlotFields.Value;
                    if (f.IsStatic || !f.Type.IsStruct())
                        ctx.Report(fl, invSym, "Value field", "non-static value type field");
                    break;

                default:
                    ctx.Report(fl, Rule02InvalidField, attr, f.Name);
                    break;
            }
        }

        if ((fields & SlotFields.State) == SlotFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, "State field");

        if ((fields & SlotFields.Key) == SlotFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, "Key field");
    }

    [Flags]
    private enum CollectionFields
    {
        None = 0,
        HashTable = 1 << 0,
        Count = 1 << 1,
    }

    private static void AnalyzeHashTable(SyntaxNodeAnalysisContext ctx, StructDeclarationSyntax sds)
    {
        var t = ctx.SemanticModel.GetDeclaredSymbol(sds, ctx.CancellationToken);
        if (t == null)
            return;

        var fields = CollectionFields.None;
        const string attr = SymbolNames.KsiHashTable;

        var loc = sds.Identifier.GetLocation();
        if (t.InAssemblyAccessibility() < Accessibility.Internal)
            ctx.Report(loc, Rule04InvalidAccessibility, $"{t.Name} struct");

        if (!sds.Modifiers.Any(SyntaxKind.PartialKeyword) || !t.IsTopLevel())
            ctx.Report(loc, Rule05InvalidHashTableDecl);

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            var fl = f.Locations.First();

            if (f is { DeclaredAccessibility: < Accessibility.Internal, Name: "HashTable" or "Count" })
                ctx.Report(fl, Rule04InvalidAccessibility, $"{f.Name} field");

            var invSym = Rule03InvalidSymbolSignature;
            switch (f.Name)
            {
                case "HashTable":
                    fields |= CollectionFields.HashTable;
                    if (f.IsStatic || f.Type is not INamedTypeSymbol nt || !nt.IsRefListOfKsiHashTableSlot())
                        ctx.Report(fl, invSym, "HashTable field", "non-static TRefList<THashTableSlot> field");
                    break;

                case "Count":
                    fields |= CollectionFields.Count;
                    if (f.IsStatic || f.Type.SpecialType != SpecialType.System_Int32)
                        ctx.Report(fl, invSym, "Count field", "non-static int field");
                    break;

                default:
                    ctx.Report(fl, Rule02InvalidField, attr, f.Name);
                    break;
            }
        }

        if ((fields & CollectionFields.HashTable) == CollectionFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, "HashTable field");

        if ((fields & CollectionFields.Count) == CollectionFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, "Count field");
    }
}