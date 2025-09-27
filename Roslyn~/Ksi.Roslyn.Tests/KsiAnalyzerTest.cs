using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Ksi.Roslyn.Tests;

public static class KsiAnalyzerTest<T> where T : DiagnosticAnalyzer, new()
{
    public static async Task RunAsync([StringSyntax("c#-test")] string code)
    {
        var test = new CSharpAnalyzerTest<T, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState = { AdditionalReferences = { typeof(RefList<>).Assembly } },
            TestCode = code,
        };

        await test.RunAsync();
    }
}