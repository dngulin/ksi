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

    [Fact]
    public async Task Dealloc02MissingDynSized()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, Dealloc]
            public struct {|DEALLOC02:TestStruct|} { public RefList<int> Field; }
            """
        );
    }

    [Fact]
    public async Task Dealloc03RedundantAttribute()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct {|DEALLOC03:TestStruct|} { public int Field; }
            """
        );
    }

    [Fact]
    public async Task Dealloc04Overwrite()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            public static class TestClass
            {
                public static void Test(ref RefList<int> list)
                {
                    {|DEALLOC04:list = default|};
                    list.Deallocated() = default;
                }
            }
            """
        );
    }

    [Fact]
    public async Task Dealloc05UnusedInstance()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class TestClass
            {
                public static void Test()
                {
                    {|DEALLOC05:RefList.WithCapacity<int>(42)|};
                    var list = RefList.WithCapacity<int>(42);
                }
            }
            """
        );
    }

    [Fact]
    public async Task Dealloc06GenericArgument()
    {
        await DeallocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct { public RefList<int> List; }

            public static class TestClass
            {
                public static void Incompatible<[ExplicitCopy] T>(in T value) {}
                public static void Compatible<[ExplicitCopy, Dealloc] T>(in T value) {}

                public static void Test()
                {
                    var test = new TestStruct();
                    Incompatible({|DEALLOC06:test|});
                    Compatible(test);
                }
            }
            """
        );
    }
}