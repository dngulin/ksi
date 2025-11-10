using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn
{
    [Generator(LanguageNames.CSharp)]
    public class RefListGenerator : IIncrementalGenerator
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
                    ctx.AddSource($"{t}.g.cs", FillTemplate(RefListTemplates.StaticApi, t, c));
                    ctx.AddSource($"{t}.Api.g.cs", FillTemplate(RefListTemplates.InstanceApi, t, c));
                    ctx.AddSource($"{t}.Iterators.g.cs", FillTemplate(RefListTemplates.Iterators, t, c));
                    ctx.AddSource($"{t}.StringExt.g.cs", FillTemplate(RefListTemplates.StringExt, t, c));

                    if (t == SymbolNames.RefList)
                        ctx.AddSource($"{t}.Dealloc.g.cs", FillTemplate(RefListTemplates.Dealloc, t, c));
                }
            });
        }

        private static string FillTemplate(string template, string typeName, string constraint)
        {
            var traitsAll = typeName == SymbolNames.TempRefList
                ? "ExplicitCopy, DynSized, Dealloc, TempAlloc"
                : "ExplicitCopy, DynSized, Dealloc";
            var traitsExceptDealloc = typeName == SymbolNames.TempRefList
                ? "ExplicitCopy, DynSized, TempAlloc"
                : "ExplicitCopy, DynSized";

            return template
                .ToStringBuilder()
                .Replace("|TRefList|", typeName)
                .Replace("|constraint|", constraint)
                .Replace("|TraitsAll|", traitsAll)
                .Replace("|TraitsExceptDealloc|", traitsExceptDealloc)
                .ToString();
        }
    }
}