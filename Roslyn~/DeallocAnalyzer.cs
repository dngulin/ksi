using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace DnDev.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DeallocAnalyzer : DiagnosticAnalyzer
    {
        private static int _ruleId;

        private static DiagnosticDescriptor Rule(DiagnosticSeverity severity, string title, string msg)
        {
            return new DiagnosticDescriptor(
                id: $"DLC{_ruleId++:D2}",
                title: title,
                messageFormat: msg,
                category: "Dealloc",
                defaultSeverity: severity,
                isEnabledByDefault: true
            );
        }

        private static readonly DiagnosticDescriptor FieldRule = Rule(
            DiagnosticSeverity.Error,
            "Field of Non-Dealloc Type",
            "Structure `{0}` can be a field only of a structure marked with `Dealloc`"
        );

        private static readonly DiagnosticDescriptor RedundantRule = Rule(
            DiagnosticSeverity.Warning,
            "Redundant Dealloc Attribute",
            "Structure `{0}` is marked with `Dealloc` attribute but doesn't have any fields to deallocate"
        );

        private static readonly DiagnosticDescriptor AssignmentRule = Rule(
            DiagnosticSeverity.Error,
            "Dealloc Type Assignment",
            "Assigning a new value to a dealloc type can cause memory leaks and forbidden"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            FieldRule,
            RedundantRule,
            AssignmentRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            if (sym.Type.TypeKind != TypeKind.Struct || sym.ContainingType.TypeKind != TypeKind.Struct)
                return;

            if (sym.ContainingType.HasDeallocAttribute())
                return;

            if (sym.Type.HasDeallocOrRefListAttribute())
                ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
        {
            var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node);
            if (sym == null || !sym.HasDeallocAttribute())
                return;

            if (sym.GetMembers().Where(m => m.Kind == SymbolKind.Field).Cast<IFieldSymbol>().Any(field => field.Type.HasDeallocOrRefListAttribute()))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx)
        {
            var assignment = (ISimpleAssignmentOperation)ctx.Operation;
            if (assignment.IsRef)
                return;

            var t = assignment.Target.Type ?? assignment.Value.Type;
            if (t == null || !t.IsValueType)
                return;

            if (t.HasDeallocOrRefListAttribute())
                ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, assignment.Syntax.GetLocation()));
        }
    }
}