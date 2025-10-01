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

    [Fact]
    public async Task ExpCopy02MissingExplicitCopy()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [DynSized, Dealloc]
            public struct {|DYNSIZED02:TestStruct|} { public RefList<int> Field; }
            """
        );
    }

    [Fact]
    public async Task ExpCopy03RedundantAttribute()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct {|DYNSIZED03:TestStruct|} { public int Field; }
            """
        );
    }

    [Fact]
    public async Task ExpCopy04NoResize()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            public static class TestClass
            {
                public static void Test([DynNoResize] ref RefList<int> list)
                {
                    list.RefAt(0) = 42;
                    {|DYNSIZED04:list|}.Clear();
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy05RedundantNoResize()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            public static class TestClass
            {
                public static void Test([Ksi.DynNoResize] ref int {|DYNSIZED05:value|}) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy06FieldOfReferenceType()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            public class TestClass
            {
                private {|DYNSIZED06:RefList<int>|} _invalid;
                private ExclusiveAccess<RefList<int>> _valid;
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy07RedundantExclusiveAccess()
    {
        await DynSizedAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public class TestClass
            {
                private {|DYNSIZED07:ExclusiveAccess<int>|} _field;
            }
            """
        );
    }
}