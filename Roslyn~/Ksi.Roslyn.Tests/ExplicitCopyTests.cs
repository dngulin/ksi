using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Ksi.Roslyn.Tests;

public class ExplicitCopyTests
{
    [Fact]
    public async Task ExpCopy01IsTriggeredByMethodArg()
    {
        var test = new CSharpAnalyzerTest<ExplicitCopyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard21,
            TestState =
            {
                AdditionalReferences = { typeof(RefList<>).Assembly }
            },
            TestCode =
                """
                using Ksi;
                
                [ExplicitCopy]
                public struct TestStruct
                {
                    public int Field;
                }
                
                public static class Test
                {
                    public static void Method(TestStruct {|EXPCOPY01:arg|}) {}
                }
                """
        };

        await test.RunAsync();
    }
}