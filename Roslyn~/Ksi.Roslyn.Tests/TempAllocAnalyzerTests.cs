namespace Ksi.Roslyn.Tests;

using TempAllocAnalyzerTest = Util.KsiAnalyzerTest<TempAllocAnalyzer>;

public class TempAllocAnalyzerTests
{
    [Fact]
    public async Task TempAlloc01MissingAttr()
    {
        await TempAllocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct MarkedStruct { public TempRefList<int> Field; }

            public struct {|TEMPALLOC01:NonMarkedStruct|} { public TempRefList<int> Field; }
            """
        );
    }

    [Fact]
    public async Task TempAlloc02MissingDynSized()
    {
        await TempAllocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, TempAlloc]
            public struct {|TEMPALLOC02:TestStruct|} { public TempRefList<int> Field; }
            """
        );
    }

    [Fact]
    public async Task TempAlloc03RedundantAttribute()
    {
        await TempAllocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct {|TEMPALLOC03:TestStruct|} { public int Field; }
            """
        );
    }

    [Fact]
    public async Task TempAlloc04IncompatibleAllocator()
    {
        await TempAllocAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct TestStruct { public TempRefList<int> List; }
            
            public static class TestClass
            {
                public static void Test(
                    in TempRefList<TestStruct> a,
                    in {|TEMPALLOC04:RefList<TestStruct>|} b,
                    in {|TEMPALLOC04:ManagedRefList<TestStruct>|} c
                )
                {
                    var x = TempRefList.Empty<TestStruct>();
                    {|TEMPALLOC04:var|} y = RefList.Empty<TestStruct>();
                    {|TEMPALLOC04:var|} z = ManagedRefList.Empty<TestStruct>();
                }
            }
            """
        );
    }
}