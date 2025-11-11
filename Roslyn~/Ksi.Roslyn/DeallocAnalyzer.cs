using System.Collections.Immutable;
using System.Linq;
using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DeallocAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule(int id, DiagnosticSeverity severity, string title, string msg)
        {
            return new DiagnosticDescriptor(
                id: $"DEALLOC{id:D2}",
                title: title,
                messageFormat: msg,
                category: "Ksi",
                defaultSeverity: severity,
                isEnabledByDefault: true
            );
        }

        private static readonly DiagnosticDescriptor Rule01MissingAttribute = Rule(01, DiagnosticSeverity.Error,
            "Missing [Dealloc] attribute",
            "Structure should be annotated with the [Dealloc] attribute " +
            "because it contains a [Dealloc] field of type `{0}`"
        );

        private static readonly DiagnosticDescriptor Rule02MissingDynSized = Rule(02, DiagnosticSeverity.Error,
            "Missing [DynSized] attribute",
            "Structure marked with the [Dealloc] attribute should be also marked with the [DynSized] attribute"
        );

        private static readonly DiagnosticDescriptor Rule03RedundantAttribute = Rule(03, DiagnosticSeverity.Warning,
            "Redundant [Dealloc] attribute",
            "Structure is marked with the [Dealloc] attribute but doesn't have any [Dealloc] fields"
        );

        private static readonly DiagnosticDescriptor Rule04Overwrite = Rule(04, DiagnosticSeverity.Error,
            "Overwriting [Dealloc] instance",
            "Operation overwrites a [Dealloc] instance without performing deallocation. " +
            "Consider to use the `Deallocated` extension method on assignment target."
        );

        private static readonly DiagnosticDescriptor Rule05UnusedInstance = Rule(05, DiagnosticSeverity.Error,
            "Unused [Dealloc] instance",
            "[Dealloc] instance returned by operation is not used and won't be deallocated"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule01MissingAttribute,
            Rule02MissingDynSized,
            Rule03RedundantAttribute,
            Rule04Overwrite,
            Rule05UnusedInstance
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeTypeParameter, SyntaxKind.TypeParameter);
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
            context.RegisterOperationAction(AnalyzeInvocationAssignment, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeInvocationSafety, OperationKind.Invocation);
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            var t = sym.Type;
            var ct = sym.ContainingType;

            if (!t.IsStructOrTypeParameter() || !ct.IsStruct())
                return;

            if (t.IsDeallocOrRefListOverDealloc() && !ct.IsDealloc())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01MissingAttribute, ct.Locations.First(), sym.Type.Name));
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
        {
            var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
            if (sym == null || !sym.IsDealloc())
                return;

            var hasDeallocFields = sym
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Field)
                .Cast<IFieldSymbol>()
                .Any(field => !field.IsStatic && field.Type.IsDeallocOrRefListOverDealloc());

            if (!hasDeallocFields)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule03RedundantAttribute, sym.Locations.First()));

            if (!sym.IsDynSized())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02MissingDynSized, sym.Locations.First()));
        }

        private static void AnalyzeTypeParameter(SyntaxNodeAnalysisContext ctx)
        {
            var s = (TypeParameterSyntax)ctx.Node;
            if (s.AttributeLists.Count == 0)
                return;

            var t = ctx.SemanticModel.GetDeclaredSymbol(s, ctx.CancellationToken);
            if (t == null)
                return;

            if (t.IsDealloc() && !t.IsDynSized())
                ctx.Report(s.Identifier.GetLocation(), Rule02MissingDynSized);
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx)
        {
            var assignment = (ISimpleAssignmentOperation)ctx.Operation;
            if (assignment.IsRef)
                return;

            var t = assignment.Target.Type ?? assignment.Value.Type;
            if (t == null || !t.IsValueType)
                return;

            if (!t.IsDeallocOrRefListOverDealloc())
                return;

            if (assignment.Target is IInvocationOperation i && i.TargetMethod.IsNonAllocatedResultRef())
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(Rule04Overwrite, assignment.Syntax.GetLocation()));
        }

        private static void AnalyzeInvocationAssignment(OperationAnalysisContext ctx)
        {
            var i = (IInvocationOperation)ctx.Operation;
            var m = i.TargetMethod;
            if (m.ReturnsRef())
                return;

            if (m.ReturnType.TypeKind != TypeKind.Struct || m.ReturnType is not INamedTypeSymbol t)
                return;

            if (!t.IsDeallocOrRefListOverDealloc())
                return;

            switch (i.Parent?.Kind)
            {
                case OperationKind.SimpleAssignment:
                case OperationKind.FieldInitializer:
                case OperationKind.MemberInitializer:
                case OperationKind.VariableInitializer:
                case OperationKind.Argument when i.Parent is IArgumentOperation { Parameter.RefKind: RefKind.None }:
                    break;

                default:
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule05UnusedInstance, i.Syntax.GetLocation()));
                    break;
            }
        }

        private static void AnalyzeInvocationSafety(OperationAnalysisContext ctx)
        {
            var i = (IInvocationOperation)ctx.Operation;

            if (i.Instance?.Type is not INamedTypeSymbol nt || !nt.IsSpan())
                return;

            if (!nt.TryGetGenericArg(out var gt) || gt == null || !gt.IsDealloc())
                return;

            if (i.TargetMethod.Name == "Clear")
                ctx.ReportDiagnostic(Diagnostic.Create(Rule04Overwrite, i.Syntax.GetLocation()));
        }
    }
}