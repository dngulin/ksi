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
        "The {0} has a wrong signature. It should be {1}"
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

        const string stateField = SymbolNames.State + " field";
        const string keyField = SymbolNames.Key + " field";
        const string valueField = SymbolNames.Value + " field";

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            var fl = f.Locations.First();

            if (IsInaccessibleHashTableSlotField(f))
                ctx.Report(fl, Rule04InvalidAccessibility, $"{f.Name} field");

            var invSym = Rule03InvalidSymbolSignature;
            switch (f.Name)
            {
                case SymbolNames.State:
                    fields |= SlotFields.State;
                    if (f.IsStatic || !f.Type.IsKsiHastTableSlotState())
                        ctx.Report(fl, invSym, stateField, "a non-static field of KsiHastTableSlotState type");
                    break;

                case SymbolNames.Key:
                    fields |= SlotFields.Key;
                    if (f.IsStatic || !f.Type.IsStruct())
                        ctx.Report(fl, invSym, keyField, "a non-static value type field");
                    break;

                case SymbolNames.Value:
                    fields |= SlotFields.Value;
                    if (f.IsStatic || !f.Type.IsStruct())
                        ctx.Report(fl, invSym, valueField, "a non-static value type field");
                    break;

                default:
                    ctx.Report(fl, Rule02InvalidField, attr, f.Name);
                    break;
            }
        }

        if ((fields & SlotFields.State) == SlotFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, stateField);

        if ((fields & SlotFields.Key) == SlotFields.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, keyField);
    }

    private static bool IsInaccessibleHashTableSlotField(IFieldSymbol f)
    {
        return f is
        {
            DeclaredAccessibility: < Accessibility.Internal,
            Name: SymbolNames.State or SymbolNames.Key or SymbolNames.Value
        };
    }

    [Flags]
    private enum CollectionSymbols
    {
        None = 0,
        HashTableFiled = 1 << 0,
        CountField = 1 << 1,
        HashMethod = 1 << 2,
        EqMethod = 1 << 3,
    }

    private static void AnalyzeHashTable(SyntaxNodeAnalysisContext ctx, StructDeclarationSyntax sds)
    {
        var t = ctx.SemanticModel.GetDeclaredSymbol(sds, ctx.CancellationToken);
        if (t == null)
            return;

        var symbols = CollectionSymbols.None;
        const string attr = SymbolNames.KsiHashTable;
        const string hashTableField = SymbolNames.HashTable + " field";
        const string countField = SymbolNames.Count + " field";
        const string hashMethod = SymbolNames.Hash + " method";
        const string eqMethod = SymbolNames.Eq + " method";

        var loc = sds.Identifier.GetLocation();
        if (t.InAssemblyAccessibility() < Accessibility.Internal)
            ctx.Report(loc, Rule04InvalidAccessibility, $"{t.Name} struct");

        if (!sds.Modifiers.Any(SyntaxKind.PartialKeyword) || !t.IsTopLevel())
            ctx.Report(loc, Rule05InvalidHashTableDecl);

        INamedTypeSymbol? tSlot = null;

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            var fl = f.Locations.First();

            if (IsInaccessibleHashTableField(f))
                ctx.Report(fl, Rule04InvalidAccessibility, $"{f.Name} field");

            var invSym = Rule03InvalidSymbolSignature;
            switch (f.Name)
            {
                case SymbolNames.HashTable:
                    symbols |= CollectionSymbols.HashTableFiled;
                    if (f.IsStatic || f.Type is not INamedTypeSymbol nt || !nt.IsRefListOfKsiHashTableSlot())
                        ctx.Report(fl, invSym, hashTableField, "a non-static TRefList<THashTableSlot> field");
                    else
                        tSlot = nt.TypeArguments.First() as INamedTypeSymbol;
                    break;

                case SymbolNames.Count:
                    symbols |= CollectionSymbols.CountField;
                    if (f.IsStatic || f.Type.SpecialType != SpecialType.System_Int32)
                        ctx.Report(fl, invSym, countField, "a non-static int field");
                    break;

                default:
                    ctx.Report(fl, Rule02InvalidField, attr, f.Name);
                    break;
            }
        }

        if ((symbols & CollectionSymbols.HashTableFiled) == CollectionSymbols.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, hashTableField);

        if ((symbols & CollectionSymbols.CountField) == CollectionSymbols.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, countField);

        var tKey = tSlot?
            .GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => !f.IsStatic && f.Type.IsStruct() && f.Name == "Key")?
            .Type as INamedTypeSymbol;

        foreach (var m in t.GetMembers().OfType<IMethodSymbol>())
        {
            var ml = m.Locations.First();

            if (m is { DeclaredAccessibility: < Accessibility.Internal, Name: SymbolNames.Hash or SymbolNames.Eq })
                ctx.Report(ml, Rule04InvalidAccessibility, $"{m.Name} field");

            var invSym = Rule03InvalidSymbolSignature;
            switch (m.Name)
            {
                case SymbolNames.Hash:
                    symbols |= CollectionSymbols.HashMethod;
                    if (!IsHashMethod(m, tKey))
                        ctx.Report(ml, invSym, hashMethod, $"the `static uint {SymbolNames.Hash}(in TKey key)`");
                    break;

                case SymbolNames.Eq:
                    symbols |= CollectionSymbols.EqMethod;
                    if (!IsEqMethod(m, tKey))
                        ctx.Report(ml, invSym, eqMethod, $"the `static bool {SymbolNames.Eq}(in TKey a, in TKey b)`");
                    break;
            }
        }

        if ((symbols & CollectionSymbols.HashMethod) == CollectionSymbols.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, hashMethod);

        if ((symbols & CollectionSymbols.EqMethod) == CollectionSymbols.None)
            ctx.Report(loc, Rule01MissingSymbol, attr, eqMethod);
    }

    private static bool IsInaccessibleHashTableField(IFieldSymbol f)
    {
        return f is
        {
            DeclaredAccessibility: < Accessibility.Internal,
            Name: SymbolNames.HashTable or SymbolNames.Count
        };
    }

    private static bool IsHashMethod(IMethodSymbol m, INamedTypeSymbol? tKey)
    {
        if (tKey == null)
            return false;

        var match = m is
        {
            IsStatic: true,
            ReturnType.SpecialType: SpecialType.System_Int32,
            RefKind: RefKind.None,
            Name: SymbolNames.Hash,
            Parameters.Length: 1
        };

        if (!match)
            return false;

        if (m.Parameters[0] is not { RefKind: RefKind.In, Type: INamedTypeSymbol { TypeKind: TypeKind.Struct } t })
            return false;

        var eqc = SymbolEqualityComparer.Default;
        return eqc.Equals(t, tKey);
    }

    private static bool IsEqMethod(IMethodSymbol m, INamedTypeSymbol? tKey)
    {
        if (tKey == null)
            return false;

        var match = m is
        {
            IsStatic: true,
            ReturnType.SpecialType: SpecialType.System_Boolean,
            RefKind: RefKind.None,
            Name: SymbolNames.Eq,
            Parameters.Length: 2
        };

        if (!match)
            return false;

        if (m.Parameters[0] is not { RefKind: RefKind.In, Type: INamedTypeSymbol { TypeKind: TypeKind.Struct } t0 })
            return false;

        if (m.Parameters[1] is not { RefKind: RefKind.In, Type: INamedTypeSymbol { TypeKind: TypeKind.Struct } t1 })
            return false;

        var eqc = SymbolEqualityComparer.Default;
        return eqc.Equals(t0, tKey) && eqc.Equals(t1, tKey);
    }

    public static bool IsValidTable(INamedTypeSymbol t, StructDeclarationSyntax sds, out INamedTypeSymbol? tSlot)
    {
        tSlot = null;

        if (!t.IsKsiHashTable() || !t.IsTopLevel() || !sds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            return false;

        var symbols = CollectionSymbols.None;
        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            if (!f.IsKsiValueTypeField())
                return false;

            var ft = (INamedTypeSymbol)f.Type;
            switch (f.Name)
            {
                case SymbolNames.HashTable when ft.IsRefListOfKsiHashTableSlot():
                    symbols |= CollectionSymbols.HashTableFiled;
                    tSlot = (INamedTypeSymbol)ft.TypeArguments[0];
                    break;
                case SymbolNames.Count:
                    symbols |= CollectionSymbols.CountField;
                    break;
                default:
                    return false;
            }
        }

        const CollectionSymbols fields = CollectionSymbols.HashTableFiled | CollectionSymbols.CountField;
        if ((symbols & fields) != fields)
            return false;

        var tKey = tSlot?
            .GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.IsKsiValueTypeField() && f.Name == SymbolNames.Key)?
            .Type as INamedTypeSymbol;

        if (tKey == null)
            return false;

        foreach (var m in t.GetMembers().OfType<IMethodSymbol>())
        {
            var isVisible = m.DeclaredAccessibility >= Accessibility.Internal;
            switch (m.Name)
            {
                case SymbolNames.Hash when isVisible && IsHashMethod(m, tKey):
                    symbols |= CollectionSymbols.HashMethod;
                    break;

                case SymbolNames.Eq when isVisible && IsEqMethod(m, tKey):
                    symbols |= CollectionSymbols.EqMethod;
                    break;

                case SymbolNames.Hash:
                case SymbolNames.Eq:
                    return false;
            }
        }

        const CollectionSymbols methods = CollectionSymbols.HashMethod | CollectionSymbols.EqMethod;
        return (symbols & methods) == methods;
    }

    public static bool IsValidSlot(ITypeSymbol t, out INamedTypeSymbol? tKey, out INamedTypeSymbol? tValue)
    {
        tKey = null;
        tValue = null;

        if (t is not INamedTypeSymbol { TypeKind: TypeKind.Struct } tSlot)
            return false;

        if (!tSlot.IsKsiHashTableSlot())
            return false;

        var fields = SlotFields.None;
        foreach (var f in t.GetMembers().OfType<IFieldSymbol>())
        {
            if (!f.IsKsiValueTypeField())
                return false;

            switch (f.Name)
            {
                case SymbolNames.State when f.Type.IsKsiHastTableSlotState():
                    fields |= SlotFields.State;
                    break;
                case SymbolNames.Key:
                    fields |= SlotFields.Key;
                    tKey = (INamedTypeSymbol)f.Type;
                    break;
                case SymbolNames.Value:
                    fields |= SlotFields.Value;
                    tValue = (INamedTypeSymbol)f.Type;
                    break;
                default:
                    return false;
            }
        }

        const SlotFields mask = SlotFields.State | SlotFields.Key;
        return (fields & mask) == mask;
    }
}