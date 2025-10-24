using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ksi.Roslyn.DocGen;

public static class KsiCompilation
{
    public static Compilation Create()
    {
        var sources = Directory
            .EnumerateFiles("Runtime", "*.cs", SearchOption.AllDirectories)
            .Append("Roslyn~/Ksi/UnityApiStub.cs")
            .Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), null, p));

        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(Span<>).Assembly.Location)
        };

        var opts = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            allowUnsafe: true,
            optimizationLevel: OptimizationLevel.Release
        );

        CSharpGeneratorDriver.Create(new RefListGenerator())
            .RunGeneratorsAndUpdateCompilation(
                CSharpCompilation.Create("Ksi.dll", sources, refs, opts),
                out var compilation,
                out _
            );

        return compilation;
    }
}