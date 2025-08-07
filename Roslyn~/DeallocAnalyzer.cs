using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RefListRoslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DeallocAnalyzer : DiagnosticAnalyzer
    {
        private static int _ruleId = 0;

        private static DiagnosticDescriptor Rule(string title, string msg)
        {
            return new DiagnosticDescriptor(
                id: $"DA{_ruleId++:D2}",
                title: title,
                messageFormat: msg,
                category: "Dealloc",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );
        }

        private static readonly DiagnosticDescriptor FieldRule = Rule(
            "Field of Non-Dealloc Type",
            "Structure `{0}` can be a field only of a structure marked with `DeallocApi`"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            FieldRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            if (sym.Type.TypeKind != TypeKind.Struct || sym.ContainingType.TypeKind != TypeKind.Struct)
                return;

            if (IsDeallocType(sym.ContainingType))
                return;

            if (IsDeallocType(sym.Type) || IsUnmanagedRefList(sym.Type))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
            }
        }

        private static bool IsDeallocType(ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Struct && type.GetAttributes().Contains("DeallocApiAttribute");
        }

        private static bool IsUnmanagedRefList(ITypeSymbol type)
        {
            return type.TypeKind == TypeKind.Struct && type.GetAttributes().Contains("UnmanagedRefListAttribute");
        }
    }
}