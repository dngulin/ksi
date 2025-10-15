using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KsiQueryAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"KSIQUERY{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01InvalidField = Rule(01, DiagnosticSeverity.Error,
        "Invalid field",
        "The structure is marked with the {0} that can have only non-private fields of {1} types"
    );

    private static readonly DiagnosticDescriptor Rule02RepeatedComponent = Rule(01, DiagnosticSeverity.Error,
        "Repeated component",
        "Repeated components within the Entity are not supported"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01InvalidField,
        Rule02RepeatedComponent
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
        var sm = ctx.SemanticModel;
        var ct = ctx.CancellationToken;

        var sds = (StructDeclarationSyntax)ctx.Node;
        var attrs = sds.AttributeLists;

        if (attrs.ContainsKsiEntity())
        {
            var t = sm.GetDeclaredSymbol(sds, ct);
            var req = new Req("[KsiEntity]", "[KsiComponent]");
            AnalyzeEntity(ctx, t, req, static ft => ft.IsKsiComponent());
        }

        if (attrs.ContainsKsiArchetype())
        {
            var t = sm.GetDeclaredSymbol(sds, ct);
            var req = new Req("[KsiArchetype]", "`RefList` over [KsiComponent]");
            AnalyzeEntity(ctx, t, req, static ft => ft.IsRefListOfComponents());
        }

        if (attrs.ContainsKsiDomain())
            AnalyzeDomain(ctx, sm.GetDeclaredSymbol(sds, ct));
    }

    private readonly struct Req(string type, string field)
    {
        public readonly string Type = type;
        public readonly string Field = field;
    }

    private static void AnalyzeEntity(
        SyntaxNodeAnalysisContext ctx, INamedTypeSymbol? t, Req req, Func<INamedTypeSymbol, bool> checkFieldType
    )
    {
        if (t == null)
            return;

        var typesSet = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic))
        {
            var l = f.Locations.First();

            if (typesSet.Contains(f.Type))
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02RepeatedComponent, l));

            typesSet.Add(f.Type);

            var invalidField = f.DeclaredAccessibility == Accessibility.Private ||
                               f.Type is not INamedTypeSymbol ft ||
                               !checkFieldType(ft);

            if (invalidField)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01InvalidField, l, req.Type, req.Field));
        }
    }

    private static void AnalyzeDomain(SyntaxNodeAnalysisContext ctx, INamedTypeSymbol? t)
    {
        const string typeReq = "[KsiDomain]";
        const string fieldReq = "[KsiArchetype] or `RefList` over [KsiEntity]";

        if (t == null)
            return;

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic))
        {
            var invalidField = f.DeclaredAccessibility == Accessibility.Private ||
                               f.Type is not INamedTypeSymbol ft ||
                               !(ft.IsKsiArchetype() || ft.IsRefListOfEntities());

            if (invalidField)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01InvalidField, f.Locations.First(), typeReq, fieldReq));
        }
    }
}