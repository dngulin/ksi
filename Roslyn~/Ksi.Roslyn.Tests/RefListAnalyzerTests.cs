namespace Ksi.Roslyn.Tests;

using RefListAnalyzerTest = Util.KsiAnalyzerTest<RefListAnalyzer>;

public class RefListAnalyzerTests
{
    [Fact]
    public async Task RefList01IncompatibleItemTypeTraits()
    {
        await RefListAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            [ExplicitCopy] public struct ExpCopyType {}
            [ExplicitCopy, Dealloc] public struct DeallocType {}

            public static class Test
            {
                public static void NonTrait(in RefList<int> list)
                {
                    Generic(list);
                    ExpCopyGeneric(list);
                    DeallocGeneric(list);
                }
            
                public static void ExpCopy(in RefList<ExpCopyType> list)
                {
                    Generic({|REFLIST01:list|});
                    ExpCopyGeneric(list);
                    DeallocGeneric(list);
                }
                
                public static void Dealloc(in RefList<DeallocType> list)
                {
                    Generic({|REFLIST01:list|});
                    ExpCopyGeneric({|REFLIST01:list|});
                    DeallocGeneric(list);
                }
                
                public static void Generic<T>(in RefList<T> list) where T : unmanaged => throw null;
                public static void ExpCopyGeneric<[ExplicitCopy] T>(in RefList<T> list) where T : unmanaged => throw null;
                public static void DeallocGeneric<[ExplicitCopy, Dealloc] T>(in RefList<T> list) where T : unmanaged => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task RefList01NonSpecializedCall()
    {
        await RefListAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct
            {
                public RefList<int> List;
            }

            public static class Test
            {
                public static void Method()
                {
                    var a = RefList.Empty<TestStruct>();
                    var b = RefList.Empty<TestStruct>();
                    
                    // Use generic non-specialized API:
                    {|REFLIST01:a|}.CopyTo<TestStruct>({|REFLIST01:ref b|});
                    {|REFLIST01:a|}.CopyFrom<TestStruct>({|REFLIST01:b|});
                    
                    {|REFLIST01:a|}.Dealloc<TestStruct>();
                    {|REFLIST01:a|}.Deallocated<TestStruct>() = default;
                    {|REFLIST01:a|}.Clear<TestStruct>();
                    {|REFLIST01:a|}.RemoveAt<TestStruct>(0);
                }
            }
            """
        );
    }

    [Fact]
    public async Task RefList02JaggedRefList()
    {
        await RefListAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class Test
            {
                public static void Method(ref {|REFLIST02:RefList<RefList<int>>|} param)
                {
                    {|REFLIST02:RefList<RefList<int>>|} a = default;
                    {|REFLIST02:var|} b = RefList.Empty<RefList<int>>();
                }
            }
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct
            {
                public {|REFLIST02:RefList<RefList<int>>|} Jagged;
            }

            public class TestClass
            {
                private ExclusiveAccess<{|REFLIST02:RefList<RefList<int>>|}> _listAccess;
            }
            """
        );
    }
}