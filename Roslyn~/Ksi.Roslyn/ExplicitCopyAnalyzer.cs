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
    public class ExplicitCopyAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule(int id, string title, string msg)
        {
            return new DiagnosticDescriptor(
                id: $"EXPCOPY{id:D2}",
                title: title,
                messageFormat: msg,
                category: "ExplicitCopy",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );
        }

        private static readonly DiagnosticDescriptor Rule01MissingAttr = Rule(01,
            "Missing [ExplicitCopy] attribute",
            "Structure should be annotated with the [ExplicitCopy] attribute " +
            "because it contains an [ExplicitCopy] field of type `{0}`"
        );

        private static readonly DiagnosticDescriptor Rule02ArgumentCopy = Rule(02,
            "Passing [ExplicitCopy] instance by value",
            "Implicit copy caused by passing a struct by value. " +
            "Consider to use the `Move` extension or changing the parameter to receive a value by reference"
        );

        private static readonly DiagnosticDescriptor Rule03ReturningCopy = Rule(03,
            "Returning a copy of the [ExplicitCopy] instance",
            "Implicit copy caused by a return operation"
        );

        private static readonly DiagnosticDescriptor Rule04AssignmentCopy = Rule(04,
            "Assignment copy of the [ExplicitCopy] instance",
            "Implicit copy caused by a assignment"
        );

        private static readonly DiagnosticDescriptor Rule05DefensiveCopy = Rule(05,
            "Defensive copy of the [ExplicitCopy] instance",
            "Implicit copy caused by non-readonly method invocation of a readonly instance"
        );

        private static readonly DiagnosticDescriptor Rule06ClosureCapture = Rule(06,
            "Capturing the [ExplicitCopy] instance by closure",
            "Implicit copy caused by closure capturing"
        );

        private static readonly DiagnosticDescriptor Rule07Boxing = Rule(07,
            "Boxing/unboxing the [ExplicitCopy] instance",
            "Boxing/unboxing is not allowed for [ExplicitCopy] types"
        );

        private static readonly DiagnosticDescriptor Rule08PrivateField = Rule(08,
            "Private field declaration in the [ExplicitCopy] type",
            "Declaring a private field prevents from providing explicit copy extensions"
        );

        private static readonly DiagnosticDescriptor Rule12SpanCopy = Rule(12,
            "Using Span copying API with [ExplicitCopy] items",
            "Span operation is not valid for [ExplicitCopy] types"
        );

        private static readonly DiagnosticDescriptor Rule13LowAccessibility = Rule(13,
            "Declaring [ExplicitCopy] struct with low accessibility",
            "Declaring [ExplicitCopy] struct with accessibility lower than `internal` " +
            "prevents from providing explicit copy extensions"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule01MissingAttr,
            Rule02ArgumentCopy,
            Rule03ReturningCopy,
            Rule04AssignmentCopy,
            Rule05DefensiveCopy,
            Rule06ClosureCapture,
            Rule07Boxing,
            Rule08PrivateField,
            Rule12SpanCopy,
            Rule13LowAccessibility
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();

            context.RegisterOperationAction(AnalyzeArgument, OperationKind.Argument);
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterOperationAction(AnalyzeBoxing, OperationKind.Conversion);
            context.RegisterSyntaxNodeAction(
                AnalyzeCaptures,
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.LocalFunctionStatement
            );
            context.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
            context.RegisterOperationAction(AnalyzeFieldInitializer, OperationKind.FieldInitializer);
            context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        }

        private static void AnalyzeArgument(OperationAnalysisContext ctx)
        {
            var arg = (IArgumentOperation)ctx.Operation;
            var p = arg.Parameter;
            var t = arg.Value.Type;

            if (p == null || t == null || !t.IsExplicitCopy())
                return;

            if (p.RefKind == RefKind.None && !IsNotExistingValue(arg.Value))
                ctx.ReportDiagnostic(Diagnostic.Create(Rule02ArgumentCopy, arg.Value.Syntax.GetLocation()));
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var f = (IFieldSymbol)ctx.Symbol;
            var t = f.Type;
            var ct = f.ContainingType;

            if (!f.Type.IsStructOrTypeParameter() || !ct.IsStruct())
                return;

            var isExplicitCopyStruct = ct.IsExplicitCopy();

            if (!isExplicitCopyStruct && t.IsExplicitCopy())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule01MissingAttr, ct.Locations.First(), t.Name));

            if (isExplicitCopyStruct && f.IsPrivate())
                ctx.ReportDiagnostic(
                    Diagnostic.Create(Rule08PrivateField, f.Locations.First(), ct.Name));
        }

        private static void AnalyzeBoxing(OperationAnalysisContext ctx)
        {
            var op = (IConversionOperation)ctx.Operation;
            var typeFrom = op.Operand.Type;
            var typeTo = op.Type;

            if (typeFrom == null || typeTo == null)
                return;

            var boxing = typeFrom.IsValueType && typeTo.IsReferenceType;
            if (boxing && typeFrom.IsExplicitCopy())
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Rule07Boxing, op.Syntax.GetLocation()));
            }

            var unboxing = typeFrom.IsReferenceType && typeTo.IsValueType;
            if (unboxing && typeTo.IsExplicitCopy())
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Rule07Boxing, op.Syntax.GetLocation()));
            }
        }

        private static void AnalyzeCaptures(SyntaxNodeAnalysisContext ctx)
        {
            var dataFlowAnalysis = ctx.SemanticModel.AnalyzeDataFlow(ctx.Node);
            var capturedVariables = dataFlowAnalysis.Captured;

            foreach (var capture in capturedVariables)
            {
                var t = GetCaptureSymbolType(capture);
                if (t != null && t.IsExplicitCopy())
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule06ClosureCapture, capture.Locations.First()));
            }
        }

        private static void AnalyzeReturn(OperationAnalysisContext ctx)
        {
            var r = (IReturnOperation)ctx.Operation;
            if (r is not { ReturnedValue: { Type: { IsReferenceType: false } t } v } || !t.IsExplicitCopy())
                return;

            if (r.ReturnsByRef(ctx.CancellationToken))
                return;

            if (IsNotExistingValue(v) || IsLocalValue(v))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(Rule03ReturningCopy, r.Syntax.GetLocation()));
        }

        private static void AnalyzeFieldInitializer(OperationAnalysisContext ctx)
        {
            var initializer = (IFieldInitializerOperation)ctx.Operation;
            var v = initializer.Value;

            if (IsNotExistingValue(v) || v.Type == null || !v.Type.IsExplicitCopy())
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(Rule04AssignmentCopy, initializer.Syntax.GetLocation()));
        }

        private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
        {
            var d = (IVariableDeclaratorOperation)ctx.Operation;
            if (!d.Symbol.IsRef && d.Initializer != null)
                AnalyzeAssignment(ctx, d.Initializer.Value, d.Syntax.GetLocation());
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx)
        {
            var assignment = (ISimpleAssignmentOperation)ctx.Operation;
            if (!assignment.IsRef)
                AnalyzeAssignment(ctx, assignment.Value, assignment.Syntax.GetLocation());
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx, IOperation v, Location location)
        {
            if (IsNotExistingValue(v) || v.Type == null || !v.Type.IsExplicitCopy())
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(Rule04AssignmentCopy, location));
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
        {
            var t = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
            if (t == null)
                return;

            if (t.IsExplicitCopy() && t.InAssemblyAccessibility() < Accessibility.Internal)
                ctx.ReportDiagnostic(Diagnostic.Create(Rule13LowAccessibility, t.Locations.First()));
        }

        private static void AnalyzeInvocation(OperationAnalysisContext ctx)
        {
            var i = (IInvocationOperation)ctx.Operation;
            AnalyzeDefensiveCopy(ctx, i);
            AnalyzeSpanApiCall(ctx, i);
        }

        private static void AnalyzeDefensiveCopy(OperationAnalysisContext ctx, IInvocationOperation op)
        {
            if (op.TargetMethod.IsReadOnly || op.Instance?.Type == null || !op.Instance.Type.IsExplicitCopy())
                return;

            if (op.Instance.IsReadonlyRef())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule05DefensiveCopy, op.Syntax.GetLocation()));
        }

        private static void AnalyzeSpanApiCall(OperationAnalysisContext ctx, IInvocationOperation op)
        {
            if (op.Instance?.Type is not INamedTypeSymbol nt || !nt.IsSpanOrReadonlySpan())
                return;

            if (!nt.TryGetGenericArg(out var gt) || gt == null || !gt.IsExplicitCopy())
                return;

            switch (op.TargetMethod.Name)
            {
                case "CopyTo":
                case "TryCopyTo":
                case "ToArray":
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule12SpanCopy, op.Syntax.GetLocation(), gt.Name));
                    break;
            }
        }

        private static ITypeSymbol? GetCaptureSymbolType(ISymbol symbol)
        {
            return symbol.Kind switch
            {
                SymbolKind.Local => ((ILocalSymbol)symbol).Type,
                SymbolKind.Parameter => ((IParameterSymbol)symbol).Type,
                _ => null
            };
        }

        private static bool IsNotExistingValue(IOperation op)
        {
            return op.Unwrapped().Kind switch
            {
                OperationKind.DefaultValue => true,
                OperationKind.ObjectCreation => true,
                OperationKind.Invocation => op is IInvocationOperation { TargetMethod.RefKind: RefKind.None },
                OperationKind.PropertyReference => op is IPropertyReferenceOperation { Property.RefKind: RefKind.None },
                _ => false
            };
        }

        private static bool IsLocalValue(IOperation op)
        {
            switch (op)
            {
                case ILocalReferenceOperation { Local.RefKind: RefKind.None }:
                case IParameterReferenceOperation { Parameter.RefKind: RefKind.None }:
                    return true;

                default:
                    return false;
            }
        }
    }
}