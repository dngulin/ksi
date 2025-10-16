using Microsoft.CodeAnalysis;

namespace Ksi.Roslyn;

[Generator(LanguageNames.CSharp)]
public partial class KsiCompGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initCtx)
    {
        GenerateKsiArchetypeExtensions(initCtx);
        GenerateKsiHandle(initCtx);
    }
}