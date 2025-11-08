namespace Ksi.Roslyn.Tests;

using KsiGenericAnalyzerTest = Util.KsiAnalyzerTest<KsiGenericAnalyzer>;

public class KsiGenericAnalyzerTests
{
    [Fact]
    public async Task RefList01IncompatibleItemTypeTraits()
    {
        await KsiGenericAnalyzerTest.RunAsync(
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
                    Generic({|KSIGENERIC01:list|});
                    ExpCopyGeneric(list);
                    DeallocGeneric(list);
                }
                
                public static void Dealloc(in RefList<DeallocType> list)
                {
                    Generic({|KSIGENERIC01:list|});
                    ExpCopyGeneric({|KSIGENERIC01:list|});
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
        await KsiGenericAnalyzerTest.RunAsync(
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
                    {|KSIGENERIC01:a|}.CopyTo<TestStruct>({|KSIGENERIC01:ref b|});
                    {|KSIGENERIC01:a|}.CopyFrom<TestStruct>({|KSIGENERIC01:b|});
                    
                    {|KSIGENERIC01:a|}.Dealloc<TestStruct>();
                    {|KSIGENERIC01:a|}.Deallocated<TestStruct>() = default;
                    {|KSIGENERIC01:a|}.Clear<TestStruct>();
                    {|KSIGENERIC01:a|}.RemoveAt<TestStruct>(0);
                }
            }
            """
        );
    }

    [Fact]
    public async Task RefList02JaggedRefList()
    {
        await KsiGenericAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class Test
            {
                public static void Method(ref {|KSIGENERIC02:RefList<RefList<int>>|} param)
                {
                    {|KSIGENERIC02:RefList<RefList<int>>|} a = default;
                    {|KSIGENERIC02:var|} b = RefList.Empty<RefList<int>>();
                }
            }
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct
            {
                public {|KSIGENERIC02:RefList<RefList<int>>|} Jagged;
            }

            public class TestClass
            {
                private ExclusiveAccess<{|KSIGENERIC02:RefList<RefList<int>>|}> _listAccess;
            }
            """
        );
    }
}