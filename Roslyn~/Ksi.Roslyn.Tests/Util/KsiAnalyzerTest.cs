using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Ksi.Roslyn.Tests.Util;

public class KsiAnalyzerTest<T> : CSharpAnalyzerTest<T, DefaultVerifier> where T : DiagnosticAnalyzer, new()
{
    protected override IEnumerable<Type> GetSourceGenerators() => [
        typeof(ExplicitCopyGenerator),
        typeof(DeallocGenerator),
        typeof(KsiCompGenerator)
    ];

    public static async Task RunAsync([StringSyntax("c#-test")] string code)
    {
        var test = new KsiAnalyzerTest<T>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState = { AdditionalReferences = { typeof(RefList<>).Assembly } },
            TestCode = code,
            TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck
        };

        await test.RunAsync();
    }
}