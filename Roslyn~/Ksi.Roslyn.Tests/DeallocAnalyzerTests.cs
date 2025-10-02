namespace Ksi.Roslyn.Tests;

using DeallocAnalyzerTest = Util.KsiAnalyzerTest<DeallocAnalyzer>;

public class DeallocAnalyzerTests
{
    [Fact]
    public async Task Dealloc01MissingAttr()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct MarkedStruct { public RefList<int> Field; }

            public struct {|DEALLOC01:NonMarkedStruct|} { public RefList<int> Field; }
            """
        );
    }
}