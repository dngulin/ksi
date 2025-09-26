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

    public static IOperation WithoutConversionOp(this IOperation self)
    {
        if (self is IConversionOperation conv)
            return conv.Operand;

        return self;
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
}