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

        private static readonly DiagnosticDescriptor FieldRule = Rule(02, DiagnosticSeverity.Error,
            "Field of Non-Dealloc Type",
            "Structure `{0}` can be a field only of a structure marked with `Dealloc`"
        );

        private static readonly DiagnosticDescriptor DynSizedRule = Rule(01, DiagnosticSeverity.Error,
            "DynSized Attribute Required",
            "Missing `DynSized` attribute for a struct `{0}` marked with `Dealloc` attribute"
        );

        private static readonly DiagnosticDescriptor RedundantRule = Rule(03, DiagnosticSeverity.Warning,
            "Redundant Dealloc Attribute",
            "Structure `{0}` is marked with `Dealloc` attribute but doesn't have any fields to deallocate"
        );

        private static readonly DiagnosticDescriptor OverwriteRule = Rule(04, DiagnosticSeverity.Error,
            "Dealloc Instance Overwrite",
            "Operation overwrites a Dealloc type instance without calling Dealloc"
        );

        private static readonly DiagnosticDescriptor NotAssignedValueRule = Rule(05, DiagnosticSeverity.Error,
            "Not Assigned Value",
            "Dealloc instance is not assigned"
        );

        private static readonly DiagnosticDescriptor GenericArgumentRule = Rule(06, DiagnosticSeverity.Error,
            "Generic Argument",
            "Passing an instance of the `Dealloc` type `{0}` as a generic argument"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            FieldRule,
            DynSizedRule,
            RedundantRule,
            OverwriteRule,
            NotAssignedValueRule,
            GenericArgumentRule
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
            context.RegisterOperationAction(AnalyzeInvocationAssignment, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeInvocationSafety, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            if (!sym.Type.IsStructOrTypeParameter() || !sym.ContainingType.IsStruct())
                return;

            if (sym.Type.IsDeallocOrRefListOverDealloc() && !sym.ContainingType.IsDealloc())
                ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
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
                ctx.ReportDiagnostic(Diagnostic.Create(RedundantRule, sym.Locations.First(), sym.Name));

            if (!sym.IsDynSized())
                ctx.ReportDiagnostic(Diagnostic.Create(DynSizedRule, sym.Locations.First(), sym.Name));
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

            ctx.ReportDiagnostic(Diagnostic.Create(OverwriteRule, assignment.Syntax.GetLocation()));
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
                    break;

                default:
                    ctx.ReportDiagnostic(Diagnostic.Create(NotAssignedValueRule, i.Syntax.GetLocation()));
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
                ctx.ReportDiagnostic(Diagnostic.Create(OverwriteRule, i.Syntax.GetLocation()));
        }

        private static void AnalyzeArgument(OperationAnalysisContext ctx)
        {
            var arg = (IArgumentOperation)ctx.Operation;
            var p = arg.Parameter;
            var t = arg.Value.Type;

            if (p == null || t == null || !t.IsDeallocOrRefListOverDealloc())
                return;

            if (p.RefKind is not (RefKind.Ref or RefKind.In))
                return;

            var ot = p.OriginalDefinition.Type;
            if (ot is not ITypeParameterSymbol)
                return;

            // Handled by ExplicitCopy analyzer or already compatible
            if (!ot.IsExplicitCopy() || ot.IsDealloc())
                return;

            var loc = arg.Value.Syntax.GetLocation();
            ctx.ReportDiagnostic(Diagnostic.Create(GenericArgumentRule, loc, t.Name));
        }
    }
}