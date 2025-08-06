using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefListRoslyn
{
    [Generator(LanguageNames.CSharp)]
    public class PublicApiGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initCtx)
        {
            var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) =>
                {
                    if (!(node is StructDeclarationSyntax structDecl))
                        return false;

                    if (structDecl.TypeParameterList?.Parameters.Count != 1)
                        return false;

                    if (structDecl.ConstraintClauses.Count != 1)
                        return false;

                    if (structDecl.ConstraintClauses[0].Constraints.Count != 1)
                        return false;

                    var c = structDecl.ConstraintClauses[0].Constraints[0];

                    if (!(c.IsKind(SyntaxKind.StructConstraint) || IsUnmanagedConstraint(c)))
                        return false;

                    return HasAttribute(structDecl.AttributeLists, "RefListApi");
                },
                transform: (ctx, _) =>
                {
                    var t = (TypeDeclarationSyntax)ctx.Node;
                    var identifier = t.Identifier.ToString();
                    var constraint = t.ConstraintClauses[0].Constraints[0].IsKind(SyntaxKind.StructConstraint)
                        ? "struct"
                        : "unmanaged";

                    return (identifier, constraint);
                }
            );

            var collected = query.Collect();

            initCtx.RegisterSourceOutput(collected, (ctx, entries) =>
            {
                foreach (var (t, c) in entries)
                {
                    ctx.AddSource($"{t}.g.cs", string.Format(Templates.StaticApi, t, c));
                    ctx.AddSource($"{t}Impl.g.cs", string.Format(Templates.Extensions, t, c));
                    ctx.AddSource($"{t}Iterators.g.cs", string.Format(Templates.Iterators, t, c));
                }
            });
        }

        private static bool HasAttribute(SyntaxList<AttributeListSyntax> attrLists, string attributeName)
        {
            foreach (var attrList in attrLists)
            foreach (var attr in attrList.Attributes)
            {
                if (attr.Name.ToString() == attributeName)
                    return true;
            }

            return false;
        }

        private static bool IsUnmanagedConstraint(TypeParameterConstraintSyntax c)
        {
            return c.IsKind(SyntaxKind.TypeConstraint) && c is TypeConstraintSyntax tcs && tcs.Type.IsUnmanaged;
        }
    }
}