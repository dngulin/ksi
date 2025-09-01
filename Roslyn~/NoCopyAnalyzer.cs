using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoCopyAnalyzer : DiagnosticAnalyzer
    {
        private static int _ruleId;

        private static DiagnosticDescriptor Rule(string title, string msg)
        {
            return new DiagnosticDescriptor(
                id: $"NOCOPY{++_ruleId:D2}",
                title: title,
                messageFormat: msg,
                category: "NoCopy",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );
        }

        private static readonly DiagnosticDescriptor ParameterRule = Rule(
            "Passed by Value",
            "Type `{0}` is marked as `NoCopy` and should be received only by reference"
        );

        private static readonly DiagnosticDescriptor ArgumentRule = Rule(
            "Received by Value",
            "Type `{0}` is marked as `NoCopy` and should be passed only by reference"
        );

        private static readonly DiagnosticDescriptor FieldRule = Rule(
            "Field of Copy Type",
            "Type `{0}` is marked as `NoCopy` and can be a field only of a `NoCopy` type"
        );

        private static readonly DiagnosticDescriptor BoxingRule = Rule(
            "Boxed",
            "Type `{0}` is marked as `NoCopy` and shouldn't be boxed"
        );

        private static readonly DiagnosticDescriptor CaptureRule = Rule(
            "Captured by Closure",
            "Type `{0}` is marked as `NoCopy` and shouldn't be captured by a closure"
        );

        private static readonly DiagnosticDescriptor ReturnRule = Rule(
            "Returned by Value",
            "Type `{0}` is marked as `NoCopy` and shouldn't be returned by value"
        );

        private static readonly DiagnosticDescriptor AssignmentRule = Rule(
            "Copied by Assignment",
            "Type `{0}` is marked as `NoCopy` and shouldn't be assigned by copying other value"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            ParameterRule,
            ArgumentRule,
            FieldRule,
            BoxingRule,
            CaptureRule,
            ReturnRule,
            AssignmentRule
        );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
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
            context.RegisterSymbolAction(AnalyzeReturn, SymbolKind.Method);
            context.RegisterOperationAction(AnalyzeFieldInitializer, OperationKind.FieldInitializer);
            context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        }

        private static void AnalyzeParameter(SymbolAnalysisContext ctx)
        {
            var sym = (IParameterSymbol)ctx.Symbol;
            if (sym.RefKind != RefKind.None)
                return;

            if (!IsNoCopyType(sym.Type))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(ParameterRule, sym.Locations.First(), sym.Type.Name));
        }

        private static void AnalyzeArgument(OperationAnalysisContext ctx)
        {
            var op = (IArgumentOperation)ctx.Operation;
            if (op.Parameter != null && op.Parameter.RefKind != RefKind.None)
                return;

            var t = op.Value.Type;
            if (t == null || !IsNoCopyType(t))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(ArgumentRule, op.Value.Syntax.GetLocation(), t.Name));
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            if (sym.Type.TypeKind != TypeKind.Struct || sym.ContainingType.TypeKind != TypeKind.Struct)
                return;

            if (!IsNoCopyType(sym.Type) || IsNoCopyType(sym.ContainingType))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));
        }

        private static void AnalyzeBoxing(OperationAnalysisContext ctx)
        {
            var op = (IConversionOperation)ctx.Operation;
            var typeFrom = op.Operand.Type;
            var typeTo = op.Type;

            if (typeFrom == null || typeTo == null)
                return;

            var boxing = typeFrom.IsValueType && typeTo.IsReferenceType;
            if (boxing && IsNoCopyType(typeFrom))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(BoxingRule, op.Syntax.GetLocation(), typeFrom.Name));
            }

            var unboxing = typeFrom.IsReferenceType && typeTo.IsValueType;
            if (unboxing && IsNoCopyType(typeTo))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(BoxingRule, op.Syntax.GetLocation(), typeFrom.Name));
            }
        }

        private static void AnalyzeCaptures(SyntaxNodeAnalysisContext ctx)
        {
            var dataFlowAnalysis = ctx.SemanticModel.AnalyzeDataFlow(ctx.Node);
            var capturedVariables = dataFlowAnalysis.Captured;

            foreach (var capture in capturedVariables)
            {
                var t = GetCaptureSymbolType(capture);
                if (t != null && IsNoCopyType(t))
                    ctx.ReportDiagnostic(Diagnostic.Create(CaptureRule, capture.Locations.First(), t.Name));
            }
        }

        private static void AnalyzeReturn(SymbolAnalysisContext ctx)
        {
            var method = (IMethodSymbol)ctx.Symbol;
            if (method.ReturnsByRef || method.ReturnsByRefReadonly)
                return;

            if (!IsNoCopyType(method.ReturnType))
                return;

            if (IsNoCopyReturnMethod(method))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(ReturnRule, method.Locations.First(), method.ReturnType.Name));
        }

        private static void AnalyzeFieldInitializer(OperationAnalysisContext ctx)
        {
            var initializer = (IFieldInitializerOperation)ctx.Operation;
            var v = initializer.Value;

            if (IsNotExistingValue(v) || v.Type == null || !IsNoCopyType(v.Type))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, initializer.Syntax.GetLocation(), v.Type.Name));
        }

        private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
        {
            var declarator = (IVariableDeclaratorOperation)ctx.Operation;
            if (declarator.Symbol.IsRef)
                return;

            var initializer = declarator.Initializer;
            if (initializer == null)
                return;

            var v = initializer.Value;
            if (IsNotExistingValue(v) || v.Type == null || !IsNoCopyType(v.Type))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, declarator.Syntax.GetLocation(), v.Type.Name));
        }

        private static void AnalyzeAssignment(OperationAnalysisContext ctx)
        {
            var assignment = (ISimpleAssignmentOperation)ctx.Operation;
            var v = assignment.Value;

            if (assignment.IsRef)
                return;

            if (IsNotExistingValue(v) || v.Type == null || !IsNoCopyType(v.Type))
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, assignment.Syntax.GetLocation(), v.Type.Name));
        }

        private static bool IsNoCopyType(ITypeSymbol t)
        {
            return t.TypeKind == TypeKind.Struct && t.GetAttributes().Any(IsNoCopyAttribute);
        }

        private static bool IsNoCopyAttribute(AttributeData a)
        {
            var name = a.AttributeClass?.Name;
            if (name == null)
                return false;

            return name.EndsWith("NoCopyAttribute") || name.EndsWith("NoCopy");
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

        private static bool IsNotExistingValue(IOperation operation)
        {
            while (true)
            {
                switch (operation.Kind)
                {
                    case OperationKind.DefaultValue:
                    case OperationKind.ObjectCreation:
                        return true;

                    case OperationKind.Conversion:
                    {
                        var conversion = (IConversionOperation)operation;
                        operation = conversion.Operand;
                        continue;
                    }

                    case OperationKind.Invocation:
                    {
                        var invocation = (IInvocationOperation)operation;
                        return IsNoCopyReturnMethod(invocation.TargetMethod);
                    }

                    default:
                        return false;
                }
            }
        }

        private static bool IsNoCopyReturnMethod(IMethodSymbol method)
        {
            return method.GetAttributes().Any(IsNoCopyReturnAttribute);
        }

        private static bool IsNoCopyReturnAttribute(AttributeData a)
        {
            var name = a.AttributeClass?.Name;
            if (name == null)
                return false;

            return name.EndsWith("NoCopyReturnAttribute") || name.EndsWith("NoCopyReturn");
        }
    }
}