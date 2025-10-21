using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ksi.Roslyn;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class KsiCompAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
    {
        return new DiagnosticDescriptor(
            id: $"KSICOMP{id:D2}",
            title: title,
            messageFormat: msg,
            category: "Ksi",
            defaultSeverity: severity,
            isEnabledByDefault: true
        );
    }

    private static readonly DiagnosticDescriptor Rule01InvalidField = Rule(01, DiagnosticSeverity.Error,
        "Invalid field of data composition type",
        "The structure is marked with the {0} that can have only public fields of {1} types"
    );

    private static readonly DiagnosticDescriptor Rule02RepeatedComponent = Rule(02, DiagnosticSeverity.Error,
        "Repeated entity component",
        "Repeated components within the Entity are not supported"
    );

    private static readonly DiagnosticDescriptor Rule03InvalidDomain = Rule(03, DiagnosticSeverity.Error,
        "Invalid [KsiDomain] declaration",
        "Structure marked with [KsiDomain] should be a partial top-level struct"
    );

    private static readonly DiagnosticDescriptor Rule04LowArchetypeAccessibility = Rule(04, DiagnosticSeverity.Error,
        "Invalid [KsiArchetype] accessibility",
        "Declaring a [KsiArchetype] struct with accessibility lower than `internal` " +
        "prevents from generation extension methods"
    );

    private static readonly DiagnosticDescriptor Rule05InvalidQueryContainingType = Rule(05, DiagnosticSeverity.Error,
        "Non top-level partial type containing [KsiQuery]",
        "Type containing [KsiQuery] methods should be a partial top-level type"
    );

    private static readonly DiagnosticDescriptor Rule06InvalidQueryMethod = Rule(06, DiagnosticSeverity.Error,
        "Invalid [KsiQuery] method signature",
        "Method marked with [KsiQuery] should be a `static void` method that " +
        "declares `KsiHandle` as the first parameter (passed by `in`) and " +
        "at least one parameter that is not marked with [KsiQueryParam]"
    );

    private static readonly DiagnosticDescriptor Rule07NonRefQueryParameter = Rule(07, DiagnosticSeverity.Error,
        "Non reference [KsiQuery] method parameter",
        "Non first [KsiQuery] method parameter should be a struct passed by reference"
    );

    private static readonly DiagnosticDescriptor Rule08InvalidQueryParameterType = Rule(08, DiagnosticSeverity.Error,
        "Invalid [KsiQuery] method parameter type",
        "Non first [KsiQuery] method parameter should be a [KsiComponent] or " +
        "should be marked with [KsiQueryParam]"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01InvalidField,
        Rule02RepeatedComponent,
        Rule03InvalidDomain,
        Rule04LowArchetypeAccessibility,
        Rule05InvalidQueryContainingType,
        Rule06InvalidQueryMethod,
        Rule07NonRefQueryParameter,
        Rule08InvalidQueryParameterType
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
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

            if (t != null && t.InAssemblyAccessibility() < Accessibility.Internal)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule04LowArchetypeAccessibility, t.Locations.First()));
        }

        if (attrs.ContainsKsiDomain())
            AnalyzeDomain(ctx, sm.GetDeclaredSymbol(sds, ct), sds);
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

            var invalidField = !f.IsPublic() ||
                               f.Type is not INamedTypeSymbol ft ||
                               !checkFieldType(ft);

            if (invalidField)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01InvalidField, l, req.Type, req.Field));
        }
    }

    private static void AnalyzeDomain(SyntaxNodeAnalysisContext ctx, INamedTypeSymbol? t, StructDeclarationSyntax sds)
    {
        const string typeReq = "[KsiDomain]";
        const string fieldReq = "[KsiArchetype] or `RefList` over [KsiEntity]";

        if (t == null)
            return;

        if (t.ContainingType != null || sds.Modifiers.All(m => !m.IsKind(SyntaxKind.PartialKeyword)))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule03InvalidDomain, sds.Identifier.GetLocation()));

        foreach (var f in t.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic))
        {
            var invalidField = !f.IsPublic() ||
                               f.Type is not INamedTypeSymbol ft ||
                               !(ft.IsKsiArchetype() || ft.IsRefListOfEntities());

            if (invalidField)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01InvalidField, f.Locations.First(), typeReq, fieldReq));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        var m = (IMethodSymbol)ctx.Symbol;
        if (!m.IsKsiQuery())
            return;

        if (!IsValidKsiQueryType(m, ctx.CancellationToken))
            ctx.ReportDiagnostic(
                Diagnostic.Create(Rule05InvalidQueryContainingType, m.ContainingType.Locations.First()));

        if (!IsValidKsiQueryMethodSignature(m))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule06InvalidQueryMethod, m.Locations.First()));

        for (var i = 1; i < m.Parameters.Length; i++)
        {
            var p = m.Parameters[i];
            if (!p.IsRef())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule07NonRefQueryParameter, p.Locations.First()));

            if (!IsValidKsiQueryParamType(p))
                ctx.ReportDiagnostic(Diagnostic.Create(Rule08InvalidQueryParameterType, p.Locations.First()));
        }
    }

    private static bool IsValidKsiQueryType(IMethodSymbol m, CancellationToken ct)
    {
        return m.ContainingType.IsTopLevel() && m.ContainingType.IsPartial(ct);
    }

    private static bool IsValidKsiQueryMethodSignature(IMethodSymbol m)
    {
        var isStaticVoid = m is { IsStatic: true, ReturnsVoid: true };
        if (!isStaticVoid)
            return false;

        var firstArgIsKsiHandle = m.Parameters.Length >= 2 &&
                                  m.Parameters[0].RefKind == RefKind.In &&
                                  m.Parameters[0].Type.IsKsiHandle();
        if (!firstArgIsKsiHandle)
            return false;

        var nonParmTypes = m.Parameters.Skip(1).Where(p => !p.IsKsiQueryParam()).Select(p => p.Type).ToImmutableArray();
        return nonParmTypes.Length != 0 && nonParmTypes.AreUnique();
    }

    private static bool IsValidKsiQueryParamType(IParameterSymbol p)
    {
        if (p.Type is not INamedTypeSymbol t)
            return false;

        return t.IsKsiComponent() || (t.IsStruct() && p.IsKsiQueryParam());
    }

    public static bool IsValidKsiQueryMethod(IMethodSymbol m, CancellationToken ct)
    {
        if (!IsValidKsiQueryType(m, ct) || !IsValidKsiQueryMethodSignature(m))
            return false;

        for (var i = 1; i < m.Parameters.Length; i++)
        {
            var p = m.Parameters[i];
            if (!p.IsRef() || !IsValidKsiQueryParamType(p))
                return false;
        }

        return true;
    }
}