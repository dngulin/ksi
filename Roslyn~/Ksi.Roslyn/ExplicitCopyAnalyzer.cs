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
            "Private filed declaration in the [ExplicitCopy] type",
            "Declaring a private field prevents from providing explicit copy extensions"
        );

        private static readonly DiagnosticDescriptor Rule09GenericDeclaration = Rule(09,
            "Generic [ExplicitCopy] type declaration",
            "Custom generic [ExplicitCopy] types are not allowed. Consider to use [RefList] collections instead"
        );

        private static readonly DiagnosticDescriptor Rule10GenericArgument = Rule(10,
            "Passing [ExplicitCopy] instance as a generic argument",
            "Passing an instance of the [ExplicitCopy] type as a generic argument that is not marked as [ExplicitCopy]"
        );

        private static readonly DiagnosticDescriptor Rule11TypeArgument = Rule(11,
            "Passing [ExplicitCopy] type as a type argument",
            "Passing [ExplicitCopy] type `{0}` as a type argument. Consider to use [RefList] collections instead"
        );

        private static readonly DiagnosticDescriptor Rule12SpanCopy = Rule(12,
            "Using Span copying API with [ExplicitCopy] items",
            "Span operation is not valid for [ExplicitCopy] types"
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
            Rule09GenericDeclaration,
            Rule10GenericArgument,
            Rule11TypeArgument,
            Rule12SpanCopy
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
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule02ArgumentCopy, loc));
                    break;
                case RefKind.None or RefKind.Ref or RefKind.In:
                    var ot = p.OriginalDefinition.Type;
                    if (ot is ITypeParameterSymbol && !ot.IsExplicitCopy())
                        ctx.ReportDiagnostic(Diagnostic.Create(Rule10GenericArgument, loc, t.Name));
                    break;
            }
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

            if (isExplicitCopyStruct && f.DeclaredAccessibility == Accessibility.Private)
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

            var isVar = d.Syntax is VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax
                {
                    Type: IdentifierNameSyntax { IsVar: true }
                }
            };

            if (!isVar)
                return;

            var t = d.Symbol.Type switch
            {
                IArrayTypeSymbol a when a.ElementType.IsExplicitCopy() => a.ElementType,
                INamedTypeSymbol n when n.IsNotSupportedGenericOverExplicitCopy(out var dyn) => dyn,
                _ => null
            };

            if (t != null)
            {
                var loc = d.GetDeclaredTypeLocation();
                ctx.ReportDiagnostic(Diagnostic.Create(Rule11TypeArgument, loc, t.Name));
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

            ctx.ReportDiagnostic(Diagnostic.Create(Rule04AssignmentCopy, location));
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext ctx)
        {
            var sym = ctx.SemanticModel.GetDeclaredSymbol((StructDeclarationSyntax)ctx.Node, ctx.CancellationToken);
            if (sym == null)
                return;

            if (sym.IsGenericType && sym.IsExplicitCopy() && !sym.IsRefList())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule09GenericDeclaration, sym.Locations.First(), sym.Name));
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
                        Diagnostic.Create(Rule11TypeArgument, e.Syntax.GetLocation(), e.Type.Name));
            }
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

        private static void AnalyzeGenericName(SyntaxNodeAnalysisContext ctx)
        {
            var s = (GenericNameSyntax)ctx.Node;
            if (s.IsUnboundGenericName)
                return;

            var i = ctx.SemanticModel.GetTypeInfo(s.GetTypeExpr(), ctx.CancellationToken);
            if (i.Type is not INamedTypeSymbol { IsGenericType: true } t)
                return;

            if (t.IsSupportedGenericType())
                return;

            foreach (var a in t.TypeArguments)
            {
                if (a is INamedTypeSymbol na && na.IsExplicitCopy())
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule11TypeArgument, s.GetLocation(), na.Name));
            }
        }

        private static void AnalyzeArrayType(SyntaxNodeAnalysisContext ctx)
        {
            var a = (ArrayTypeSyntax)ctx.Node;

            var i = ctx.SemanticModel.GetTypeInfo(a.ElementType, ctx.CancellationToken);
            if (i.Type is not INamedTypeSymbol { TypeKind: TypeKind.Struct } t)
                return;

            if (t.IsExplicitCopy())
                ctx.ReportDiagnostic(Diagnostic.Create(Rule11TypeArgument, a.GetLocation(), t.Name));
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
            while (true)
            {
                switch (op.Kind)
                {
                    case OperationKind.DefaultValue:
                    case OperationKind.ObjectCreation:
                        return true;

                    case OperationKind.Invocation:
                        return op is IInvocationOperation { TargetMethod.RefKind: RefKind.None };

                    case OperationKind.PropertyReference:
                        return op is IPropertyReferenceOperation { Property.RefKind: RefKind.None };

                    case OperationKind.Conversion:
                    {
                        var conv = (IConversionOperation)op;
                        op = conv.Operand;
                        continue;
                    }

                    default:
                        return false;
                }
            }
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