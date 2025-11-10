using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Ksi.Roslyn.Extensions;

public static class OperationExtensions
{
    public static IOperation? GetEnclosingBody(this IOperation op)
    {
        for (var p = op; p != null; p = p.Parent)
        {
            if (p is IMethodBodyOperation || p is IConstructorBodyOperation)
                return p;
        }

        return null;
    }

    public static IMethodSymbol? GetEnclosingMethod(this IOperation self, CancellationToken ct)
    {
        var model = self.SemanticModel;

        var enclosing = model?.GetEnclosingSymbol(self.Syntax.SpanStart, ct);
        if (enclosing is IMethodSymbol method)
            return method;

        return null;
    }

    public static bool IsRefListIterator(this IOperation self, out IOperation? collection)
    {
        collection = null;
        if (self is not IInvocationOperation i)
            return false;

        var m = i.TargetMethod;
        if (!m.IsExtensionMethod || !m.ReturnType.IsRefLikeType)
            return false;

        var p = m.Parameters.First();
        if (!p.IsRef() || !p.Type.IsRefList())
            return false;

        collection = i.Arguments.First().Value;
        return true;
    }

    public static Location GetDeclaredTypeLocation(this IVariableDeclaratorOperation self)
    {
        if (self.Syntax.Parent is VariableDeclarationSyntax vds)
            return vds.Type.GetLocation();

        return self.Syntax.GetLocation();
    }

    public static bool IsVar(this IVariableDeclaratorOperation d)
    {
        return d.Syntax.Parent is VariableDeclarationSyntax
        {
            Type: IdentifierNameSyntax { IsVar: true }
        };
    }

    public static bool ReturnsByRef(this IReturnOperation returnOp, CancellationToken ct)
    {
        var containing = returnOp.SemanticModel?.GetEnclosingSymbol(returnOp.Syntax.SpanStart, ct);
        switch (containing)
        {
            case IMethodSymbol { RefKind: RefKind.Ref or RefKind.RefReadOnly }:
            case IPropertySymbol { RefKind: RefKind.Ref or RefKind.RefReadOnly }:
                return true;
            default:
                return false;
        }
    }

    public static bool IsReadonlyRef(this IOperation op)
    {
        while (true)
        {
            switch (op)
            {
                case IParameterReferenceOperation { Parameter.RefKind: RefKind.In }:
                case ILocalReferenceOperation { Local.RefKind: RefKind.RefReadOnly }:
                case IInvocationOperation { TargetMethod.RefKind: RefKind.RefReadOnly }:
                case IPropertyReferenceOperation { Property.RefKind: RefKind.RefReadOnly }:
                case IFieldReferenceOperation { Field.IsReadOnly: true }:
                    return true;

                case IFieldReferenceOperation { Instance: { Type.TypeKind: TypeKind.Struct } instance }:
                    op = instance;
                    continue;

                default:
                    return false;
            }
        }
    }

    public static IOperation Unwrapped(this IOperation op)
    {
        while (true)
        {
            switch (op)
            {
                case IConversionOperation { IsImplicit: true } conv:
                    op = conv.Operand;
                    continue;

                case IParenthesizedOperation paren:
                    op = paren.Operand;
                    continue;

                default:
                    return op;
            }
        }
    }

    public static ImmutableArray<(ITypeSymbol, ITypeParameterSymbol)> GetGenericTypeSubstitutions(this IArgumentOperation a)
    {
        var at = a.Value.Type;
        var pt = a.Parameter?.OriginalDefinition.Type;

        if (at == null || pt == null)
            return ImmutableArray<(ITypeSymbol, ITypeParameterSymbol)>.Empty;

        var subs = ImmutableArray.CreateBuilder<(ITypeSymbol, ITypeParameterSymbol)>();
        CollectSubstitutions(at, pt, subs);
        return subs.ToImmutable();
    }

    private static void CollectSubstitutions(ITypeSymbol at, ITypeSymbol pt, ImmutableArray<(ITypeSymbol, ITypeParameterSymbol)>.Builder subs)
    {
        if (pt is ITypeParameterSymbol p)
        {
            subs.Add((at, p));
            return;
        }

        switch (at)
        {
            case INamedTypeSymbol nat when pt is INamedTypeSymbol npt && nat.TypeArguments.Length == npt.TypeArguments.Length:
            {
                for (var i = 0; i < nat.TypeArguments.Length; i++)
                    CollectSubstitutions(nat.TypeArguments[i], npt.TypeArguments[i], subs);
                break;
            }

            case IArrayTypeSymbol aat when pt is IArrayTypeSymbol pat:
                CollectSubstitutions(aat.ElementType, pat.ElementType, subs);
                break;
        }
    }
}