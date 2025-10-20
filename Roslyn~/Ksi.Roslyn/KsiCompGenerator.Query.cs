using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ksi.Roslyn;

public partial class KsiCompGenerator
{
    private enum RefMutability
    {
        Readonly,
        MutableNoResize,
        Mutable,
    }

    private class QueryInfo(IMethodSymbol m, INamedTypeSymbol domain, ImmutableArray<ParamInfo> parameters)
    {
        public readonly IMethodSymbol TargetMethod = m;
        public readonly INamedTypeSymbol Domain = domain;
        public readonly RefMutability DomainMutability = EstimateDomainMutability(parameters);
        public readonly ImmutableArray<ParamInfo> Parameters = parameters;
        public readonly List<Match> Matches = new List<Match>(16);
    }

    private class ParamInfo(IParameterSymbol p)
    {
        public readonly IParameterSymbol Symbol = p;
        public readonly bool IsQueryParam = p.IsKsiQueryParam();
        public readonly string RefKindStr = p.RefKind == RefKind.Ref ? "ref" : "in";
    }

    private class Match(string fieldName, bool isAoS, ImmutableArray<string> argNames)
    {
        public readonly string FieldName = fieldName;
        public readonly bool IsAoS = isAoS;
        public readonly ImmutableArray<string> ArgNames = argNames;
    }

    private static void GenerateKsiQuery(IncrementalGeneratorInitializationContext initCtx)
    {
        var query = initCtx.SyntaxProvider.CreateSyntaxProvider(
            predicate: (node, _) => node is MethodDeclarationSyntax mds && mds.AttributeLists.ContainsKsiQuery(),
            transform: (ctx, ct) =>
            {
                var m = ctx.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax)ctx.Node, ct);
                if (m == null || !KsiCompAnalyzer.IsValidKsiQueryMethod(m, ct))
                    return null;

                var tDomain = m.Parameters.First().Type.ContainingType;
                if (tDomain == null)
                    return null;

                var parameters = m.Parameters.Skip(1).Select(p => new ParamInfo(p)).ToImmutableArray();
                var queryInfo = new QueryInfo(m, tDomain, parameters);

                foreach (var f in tDomain.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic && f.IsPublic()))
                {
                    if (f.Type is not INamedTypeSymbol ft)
                        continue;

                    var isAoS = ft.IsRefListOfEntities();
                    if (!isAoS && !ft.IsKsiArchetype())
                        continue;


                    var fields = isAoS ? GetFieldsAoS(ft) : GetFieldsSoA(ft);
                    if (TryMatch(fields, parameters, out var args))
                        queryInfo.Matches.Add(new Match(f.Name, isAoS, args));
                }

                return queryInfo;
            }
        );

        var collected = query.Collect();

        initCtx.RegisterSourceOutput(collected, (ctx, queryInfos) =>
        {
            var sb = new StringBuilder(16 * 1024);

            foreach (var q in queryInfos)
            {
                if (q == null)
                    continue;

                var m = q.TargetMethod;
                var t = m.ContainingType;
                var tds = (TypeDeclarationSyntax)t.DeclaringSyntaxReferences.First().GetSyntax(ctx.CancellationToken);
                var tDomain = q.Domain.Name;

                var dMut = q.DomainMutability switch
                {
                    RefMutability.Mutable => "ref",
                    RefMutability.MutableNoResize => "[DynNoResize] ref",
                    RefMutability.Readonly => "in",
                    _ => throw new ArgumentOutOfRangeException()
                };
                var parameters = q.Parameters
                    .Where(p => p.IsQueryParam)
                    .Select((p, idx) => $"{p.RefKindStr} {p.Symbol.Type} {QueryParamName(p.Symbol.Name, idx)}")
                    .Prepend($"{dMut} {tDomain} domain")
                    .CommaSeparated();

                using (var file = AppendScope.Root(sb))
                {
                    file.AppendLine("using Ksi;\n");

                    using (var ns = file.OptNamespace(t.ContainingNamespace.FullyQualifiedName()))
                    using (var type = ns.Sub($"{tds.Modifiers} {tds.Keyword} {tds.Identifier}{tds.TypeParameterList}"))
                    using (var method = type.PubStat($"void {m.Name}({parameters})"))
                    {
                        for (var i = 0; i < q.Matches.Count; i++)
                        {
                            var match = q.Matches[i];
                            var f = match.FieldName;

                            var handle = i == 0
                                ? $"var handle = new {tDomain}.KsiHandle({tDomain}.KsiSection.{f}, 0);"
                                : $"\nhandle.Section = {tDomain}.KsiSection.{f};";
                            method.AppendLine(handle);

                            using var loop = method.Sub($"for (handle.Index = 0; handle.Index < domain.{f}.Count(); handle.Index++)");

                            var isMut = q.DomainMutability != RefMutability.Readonly;
                            var refKind = isMut ? "ref" : "ref readonly";
                            var indexMethod = isMut ? "RefAt" : "RefReadonlyAt";

                            loop.AppendLine(match.IsAoS
                                ? $"{refKind} var entity = ref domain.{f}.{indexMethod}(handle.Index);"
                                : $"{refKind} var archetype = ref domain.{f};");

                            var args = GetTargetMethodArgs(q, match).CommaSeparated();
                            loop.AppendLine($"{q.TargetMethod.Name}({args});");
                        }
                    }
                }

                ctx.AddSource($"{t.MetadataName}.{m.MetadataName}.KsiQuery.g.cs", sb.ToString());
            }
        });
    }

    private static string QueryParamName(string name, int idx)
    {
        if (name == "")
            return $"p{idx}";

        var replace = name is "domain" or "handle" or "entity" or "archetype" ||
                     (name.Length > 1 && name[0] == 'p' && char.IsDigit(name[1]));
        return replace ? $"p{idx}_" + name : name;
    }

    private static RefMutability EstimateDomainMutability(ImmutableArray<ParamInfo> parameters)
    {
        var result = RefMutability.Readonly;

        foreach (var pi in parameters)
        {
            if (pi.IsQueryParam)
                continue;

            var p = pi.Symbol;
            var mut = p.RefKind switch
            {
                RefKind.In => RefMutability.Readonly,
                _ => p.IsDynNoResize() || !p.Type.IsDynSized() ? RefMutability.MutableNoResize : RefMutability.Mutable
            };

            if (mut > result)
                result = mut;
        }

        return result;
    }

    private static List<string> GetTargetMethodArgs(QueryInfo q, Match m)
    {
        var args = new List<string>(q.Parameters.Length + 1) { "in handle" };

        for (var i = 0; i < q.Parameters.Length; i++)
        {
            var p = q.Parameters[i];
            var name = m.ArgNames[i];
            if (p.IsQueryParam)
            {
                args.Add($"{p.RefKindStr} {name}");
            }
            else if (m.IsAoS)
            {
                // entity = domain.{f}.{refMethod}(handle.Index));
                args.Add($"{p.RefKindStr} entity.{name}");
            }
            else
            {
                var indexMethod = p.Symbol.RefKind == RefKind.Ref ? "RefAt" : "RefReadonlyAt";
                // archetype = domain.{f};
                args.Add($"{p.RefKindStr} archetype.{name}.{indexMethod}(handle.Index)");
            }
        }

        return args;
    }

    private static ImmutableDictionary<ITypeSymbol, string> GetFieldsAoS(INamedTypeSymbol refListOfEntities)
    {
        return refListOfEntities.TypeArguments
            .First()
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => !f.IsStatic)
            .ToImmutableDictionary<IFieldSymbol, ITypeSymbol, string>(
                f => f.Type,
                f => f.Name,
                SymbolEqualityComparer.Default
            );
    }

    private static ImmutableDictionary<ITypeSymbol, string> GetFieldsSoA(INamedTypeSymbol archetype)
    {
        return archetype
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => !f.IsStatic && f.Type is INamedTypeSymbol)
            .Select(f => (f.Name, (INamedTypeSymbol)f.Type))
            .ToImmutableDictionary<(string Name, INamedTypeSymbol Type), ITypeSymbol, string>(
                f => f.Type.TypeArguments.First(),
                f => f.Name,
                SymbolEqualityComparer.Default
            );
    }

    private static bool TryMatch(
        ImmutableDictionary<ITypeSymbol, string> entityFields,
        ImmutableArray<ParamInfo> parameters,
        out ImmutableArray<string> matchArgs)
    {
        var match = parameters.Where(p => !p.IsQueryParam).All(p => entityFields.ContainsKey(p.Symbol.Type));
        if (!match)
        {
            matchArgs = ImmutableArray<string>.Empty;
            return false;
        }

        var args = new string[parameters.Length];
        var pIdx = 0;
        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            args[i] = param.IsQueryParam ?
                QueryParamName(param.Symbol.Name, pIdx++) :
                entityFields[param.Symbol.Type];
        }

        matchArgs = args.ToImmutableArray();
        return true;
    }
}