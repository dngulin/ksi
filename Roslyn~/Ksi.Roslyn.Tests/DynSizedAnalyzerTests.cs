namespace Ksi.Roslyn.Tests;

using DynSizedAnalyzerTest = Util.KsiAnalyzerTest<DynSizedAnalyzer>;

public class DynSizedAnalyzerTests
{
    [Fact]
    public async Task ExpCopy01MissingAttr()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct MarkedStruct { public RefList<int> Field; }

            public struct {|DYNSIZED01:NonMarkedStruct|} { public RefList<int> Field; }
            """
        );
    }
}