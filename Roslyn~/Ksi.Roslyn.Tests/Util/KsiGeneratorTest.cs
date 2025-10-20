using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Ksi.Roslyn.Tests.Util;

public static class KsiGeneratorTest<T> where T : IIncrementalGenerator, new()
{
    public static async Task RunAsync(
        [StringSyntax("c#-test")] string code,
        string fileName,
        [StringSyntax("c#-test")] string fileContents
    )
    {
        var test = new CSharpSourceGeneratorTest<T, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState =
            {
                AdditionalReferences = { typeof(RefList<>).Assembly },
                GeneratedSources = { (typeof(T), fileName, fileContents) }
            },
            TestCode = code
        };

        await test.RunAsync();
    }

    public static async Task RunAsync(
        [StringSyntax("c#-test")] string code,
        params string[] nameContentsPairs
    )
    {
        var test = new CSharpSourceGeneratorTest<T, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState =
            {
                AdditionalReferences = { typeof(RefList<>).Assembly },
            },
            TestCode = code
        };

        for (var i = 0; i < nameContentsPairs.Length; i += 2)
        {
            var fileName = nameContentsPairs[i];
            var fileContents = nameContentsPairs[i + 1];
            test.TestState.GeneratedSources.Add((typeof(T), fileName, fileContents));
        }

        await test.RunAsync();
    }
}