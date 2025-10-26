using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
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
        "Type is marked with {0} and should declare {1}"
    );

    private static readonly DiagnosticDescriptor Rule02InvalidSymbolName = Rule(02, DiagnosticSeverity.Error,
        "Invalid symbol name",
        "Type is marked with {0} and shouldn't declare the symbol {1}"
    );

    private static readonly DiagnosticDescriptor Rule03InvalidSymbolSignature = Rule(03, DiagnosticSeverity.Error,
        "Invalid symbol signature",
        "The {0} has a wrong signature. It should be {1}"
    );

    private static readonly DiagnosticDescriptor Rule04InvalidSymbolAccessibility = Rule(04, DiagnosticSeverity.Error,
        "Invalid symbol accessibility",
        "Accessibility of the {0} is to low. It should be at least {1}"
    );

    private static readonly DiagnosticDescriptor Rule05InvalidHashTableDecl = Rule(05, DiagnosticSeverity.Error,
        "Invalid KsiHashTable declaration",
        "KsiHashTable type should be a top-level partial struct"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        Rule01MissingSymbol,
        Rule02InvalidSymbolName,
        Rule03InvalidSymbolSignature,
        Rule04InvalidSymbolAccessibility,
        Rule05InvalidHashTableDecl
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(
            GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
        );
        context.EnableConcurrentExecution();
    }
}