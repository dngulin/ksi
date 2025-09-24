using Ksi.Roslyn.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class RefListApiGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initCtx)
        {
            var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) =>
                {
                    if (node is not StructDeclarationSyntax structDecl)
                        return false;

                    if (structDecl.TypeParameterList?.Parameters.Count != 1)
                        return false;

                    if (structDecl.ConstraintClauses.Count != 1)
                        return false;

                    if (structDecl.ConstraintClauses[0].Constraints.Count != 1)
                        return false;

                    var c = structDecl.ConstraintClauses[0].Constraints[0];

                    if (!(c.IsKind(SyntaxKind.StructConstraint) || c.IsUnmanagedConstraint()))
                        return false;

                    return structDecl.AttributeLists.ContainsRefList();
                },
                transform: (ctx, _) =>
                {
                    var t = (StructDeclarationSyntax)ctx.Node;
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
                    ctx.AddSource($"{t}.g.cs", string.Format(RefListApiTemplates.StaticApi, t, c));
                    ctx.AddSource($"{t}Impl.g.cs", string.Format(RefListApiTemplates.InstanceApi, t, c));
                    ctx.AddSource($"{t}Iterators.g.cs", string.Format(RefListApiTemplates.Iterators, t, c));
                    ctx.AddSource($"{t}StringExt.g.cs", string.Format(RefListApiTemplates.StringExt, t));

                    if (t == "RefList")
                        ctx.AddSource($"{t}Dealloc.g.cs", string.Format(RefListApiTemplates.Dealloc, t, c));
                }
            });
        }
    }
}