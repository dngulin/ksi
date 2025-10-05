using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Ksi.Roslyn.Tests.Util;

public static class KsiGeneratorTest<T> where T : new()
{
    public static async Task RunAsync(
        [StringSyntax("c#-test")] string code,
        string fileName,
        [StringSyntax("c#-test")] string fileContents
    )
    {
        await RunAsync(code, (fileName, fileContents));
    }

    public static async Task RunAsync(
        [StringSyntax("c#-test")] string code,
        string fileName1,
        [StringSyntax("c#-test")] string fileContents1,
        string fileName2,
        [StringSyntax("c#-test")] string fileContents2
    )
    {
        await RunAsync(code, (fileName1, fileContents1), (fileName2, fileContents2));
    }

    private static async Task RunAsync(string code, params (string Name, string Contents)[] sources)
    {
        var test = new CSharpSourceGeneratorTest<T, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState = { AdditionalReferences = { typeof(RefList<>).Assembly } },
            TestCode = code
        };

        foreach (var (name, contents) in sources)
            test.TestState.GeneratedSources.Add((typeof(T), name, contents));

        await test.RunAsync();
    }
}