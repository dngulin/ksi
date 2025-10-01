namespace Ksi.Roslyn.Tests;

using BorrowAnalyzerTest = Util.KsiAnalyzerTest<BorrowAnalyzer>;

public class BorrowAnalyzerTests
{
    [Fact]
    public async Task Borrow01NonRefPath()
    {
        await BorrowAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct { public RefList<int> Field; }

            public static class TestClass
            {
                public static void Test(in TestStruct value)
                {
                    ref readonly var test = ref {|BORROW01:MakeRef(value)|};
                }
                
                public static ref readonly int MakeRef(in TestStruct value) => ref value.Field.RefReadonlyAt(0);
            }
            """
        );
    }
}