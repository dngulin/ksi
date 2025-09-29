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

        // private static readonly DiagnosticDescriptor ParameterRule = Rule(01, "Received by Value",
        //     "Receiving by value an instance of the `ExplicitCopy` type `{0}`. Consider to receive it by reference"
        // );

        private static readonly DiagnosticDescriptor Rule02ByValueArg = Rule(02,
            "Passing [ExplicitCopy] instance by value",
            "Implicit copy caused by passing a struct by value. " +
            "Consider to use the `Move` extension or changing the parameter to receive a value by reference"
        );

        private static readonly DiagnosticDescriptor FieldRule = Rule(03, "Field of non-`ExplicitCopy` Struct",
            "Declaring field of `ExplicitCopy` type `{0}` within non-`ExplicitCopy` struct. " +
            "Consider to annotate the struct with the `ExplicitCopy` attribute"
        );

        private static readonly DiagnosticDescriptor BoxingRule = Rule(04, "Boxed",
            "Boxing of `ExplicitCopy` type `{0}`"
        );

        private static readonly DiagnosticDescriptor CaptureRule = Rule(05, "Captured by Closure",
            "Capturing of `ExplicitCopy` type `{0}` by a closure"
        );

        private static readonly DiagnosticDescriptor ReturnRule = Rule(06, "Returned by Value",
            "Returning an instance of `ExplicitCopy` type `{0}` by value. " +
            "Consider to annotate the method with the `ExplicitCopyReturn` attribute "
        );

        private static readonly DiagnosticDescriptor AssignmentRule = Rule(07, "Copied by Assignment",
            "Copying an instance of `ExplicitCopy` type `{0}` by assignment"
        );

        private static readonly DiagnosticDescriptor PrivateFieldRule = Rule(08, "Private Field",
            "Declaring a private field in the `ExplicitCopy` type `{0}` prevents from providing explicit copy extensions"
        );

        private static readonly DiagnosticDescriptor GenericTypeRule = Rule(09, "Generic Type",
            "Declaring `ExplicitCopy` type `{0}` as a generic type prevents from providing explicit copy extensions"
        );

        private static readonly DiagnosticDescriptor GenericArgumentRule = Rule(10, "Generic Argument",
            "Passing an instance of the `ExplicitCopy` type `{0}` as a generic argument"
        );

        private static readonly DiagnosticDescriptor GenericTypeArgumentRule = Rule(11, "Generic Type Argument",
            "Passing the `ExplicitCopy` type `{0}` as a type argument"
        );

        private static readonly DiagnosticDescriptor GenericCopyRule = Rule(12, "Copied in Generic Context",
            "Operation produces non-explicit copy of `{0}` in generic context"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Rule02ByValueArg,
            FieldRule,
            BoxingRule,
            CaptureRule,
            ReturnRule,
            AssignmentRule,
            PrivateFieldRule,
            GenericTypeRule,
            GenericArgumentRule,
            GenericTypeArgumentRule,
            GenericCopyRule
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
            context.RegisterSymbolAction(AnalyzeReturn, SymbolKind.Method);
            context.RegisterOperationAction(AnalyzeFieldInitializer, OperationKind.FieldInitializer);
            context.RegisterOperationAction(AnalyzeVariableDeclarator, OperationKind.VariableDeclarator);
            context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
            context.RegisterOperationAction(AnalyzeTuple, OperationKind.Tuple);
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
            context.RegisterSyntaxNodeAction(AnalyzeArrayType, SyntaxKind.ArrayType);
        }

        private static void AnalyzeArgument(OperationAnalysisContext ctx)
        {
            var arg = (IArgumentOperation)ctx.Operation;
            var p = arg.Parameter;
            var t = arg.Value.Type;

            if (p == null || t == null || !t.IsExplicitCopy())
                return;

            var loc = arg.Value.Syntax.GetLocation();
            switch (p.RefKind)
            {
                case RefKind.None when !IsNotExistingValue(arg.Value):
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule02ByValueArg, loc));
                    break;
                case RefKind.Ref or RefKind.In:
                    var ot = p.OriginalDefinition.Type;
                    if (ot is ITypeParameterSymbol && !ot.IsExplicitCopy())
                        ctx.ReportDiagnostic(Diagnostic.Create(GenericArgumentRule, loc, t.Name));
                    break;
            }
        }

        private static void AnalyzeField(SymbolAnalysisContext ctx)
        {
            var sym = (IFieldSymbol)ctx.Symbol;
            if (!sym.Type.IsStructOrTypeParameter() || !sym.ContainingType.IsStruct())
                return;

            var isExplicitCopyStruct = sym.ContainingType.IsExplicitCopy();

            if (!isExplicitCopyStruct && sym.Type.IsExplicitCopy())
                ctx.ReportDiagnostic(Diagnostic.Create(FieldRule, sym.Locations.First(), sym.Type.Name));

            if (isExplicitCopyStruct && sym.DeclaredAccessibility == Accessibility.Private)
                ctx.ReportDiagnostic(
                    Diagnostic.Create(PrivateFieldRule, sym.Locations.First(), sym.ContainingType.Name));
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
                ctx.ReportDiagnostic(Diagnostic.Create(BoxingRule, op.Syntax.GetLocation(), typeFrom.Name));
            }

            var unboxing = typeFrom.IsReferenceType && typeTo.IsValueType;
            if (unboxing && typeTo.IsExplicitCopy())
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
                if (t != null && t.IsExplicitCopy())
                    ctx.ReportDiagnostic(Diagnostic.Create(CaptureRule, capture.Locations.First(), t.Name));
            }
        }

        private static void AnalyzeReturn(SymbolAnalysisContext ctx)
        {
            var method = (IMethodSymbol)ctx.Symbol;
            if (method.ReturnsRef())
                return;

            if (!method.ReturnType.IsExplicitCopy())
                return;

            if (method.IsExplicitCopyReturn())
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(ReturnRule, method.Locations.First(), method.ReturnType.Name));
        }

        private static void AnalyzeFieldInitializer(OperationAnalysisContext ctx)
        {
            var initializer = (IFieldInitializerOperation)ctx.Operation;
            var v = initializer.Value;

            if (IsNotExistingValue(v) || v.Type == null || !v.Type.IsExplicitCopy())
                return;

            ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, initializer.Syntax.GetLocation(), v.Type.Name));
        }

        private static void AnalyzeVariableDeclarator(OperationAnalysisContext ctx)
        {
            var d = (IVariableDeclaratorOperation)ctx.Operation;
            if (!d.Symbol.IsRef && d.Initializer != null)
                AnalyzeAssignment(ctx, d.Initializer.Value, d.Syntax.GetLocation());

            var t = d.Symbol.Type switch
            {
                IArrayTypeSymbol a when a.ElementType.IsExplicitCopy() => a.ElementType,
                INamedTypeSymbol n when n.IsNotSupportedGenericType(out var dyn) => dyn,
                _ => null
            };

            if (t != null)
            {
                var loc = d.GetDeclaredTypeLocation();
                ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeArgumentRule, loc, t.Name));
            }
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

            ctx.ReportDiagnostic(Diagnostic.Create(AssignmentRule, location, v.Type.Name));
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
        {
            var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
            if (sym == null)
                return;

            if (sym.IsGenericType && sym.IsExplicitCopy() && !sym.IsRefList())
                ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeRule, sym.Locations.First(), sym.Name));
        }

        private static void AnalyzeTuple(OperationAnalysisContext ctx)
        {
            var tuple = (ITupleOperation)ctx.Operation;
            foreach (var e in tuple.Elements)
            {
                if (e.Type == null)
                    continue;

                if (e.Type.IsExplicitCopy())
                    ctx.ReportDiagnostic(
                        Diagnostic.Create(GenericTypeArgumentRule, e.Syntax.GetLocation(), e.Type.Name));
            }
        }

        private static void AnalyzeInvocation(OperationAnalysisContext ctx)
        {
            var i = (IInvocationOperation)ctx.Operation;

            if (i.Instance?.Type is not INamedTypeSymbol nt || !nt.IsSpanOrReadonlySpan())
                return;

            if (!nt.TryGetGenericArg(out var gt) || gt == null || !gt.IsExplicitCopy())
                return;

            switch (i.TargetMethod.Name)
            {
                case "CopyTo":
                case "TryCopyTo":
                case "ToArray":
                case "Fill":
                    ctx.ReportDiagnostic(Diagnostic.Create(GenericCopyRule, i.Syntax.GetLocation(), gt.Name));
                    break;
            }
        }

        private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
        {
            var s = (GenericNameSyntax)ctx.Node;
            if (s.IsUnboundGenericName)
                return;

            var i = ctx.SemanticModel.GetTypeInfo(s, ctx.CancellationToken);
            if (i.Type is not INamedTypeSymbol { IsGenericType: true } t)
                return;

            if (t.IsSupportedGenericType())
                return;

            foreach (var a in t.TypeArguments)
            {
                if (a is INamedTypeSymbol na && na.IsExplicitCopy())
                    ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeArgumentRule, s.GetLocation(), na.Name));
            }
        }

        private static void AnalyzeArrayType(SyntaxNodeAnalysisContext ctx)
        {
            var a = (ArrayTypeSyntax)ctx.Node;

            var i = ctx.SemanticModel.GetTypeInfo(a.ElementType, ctx.CancellationToken);
            if (i.Type is not INamedTypeSymbol { TypeKind: TypeKind.Struct } t)
                return;

            if (t.IsExplicitCopy())
                ctx.ReportDiagnostic(Diagnostic.Create(GenericTypeArgumentRule, a.GetLocation(), t.Name));
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
                        return invocation.TargetMethod.IsExplicitCopyReturn();
                    }

                    default:
                        return false;
                }
            }
        }
    }
}