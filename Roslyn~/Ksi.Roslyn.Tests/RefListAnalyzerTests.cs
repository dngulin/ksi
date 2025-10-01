namespace Ksi.Roslyn.Tests;

using RefListAnalyzerTest = Util.KsiAnalyzerTest<RefListAnalyzer>;

public class RefListAnalyzerTests
{
    [Fact]
    public async Task RefList01GenericItemType()
    {
        await RefListAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class Test
            {
                public static void Method<T>(ref {|REFLIST01:RefList<T>|} param) where T : unmanaged
                {
                    {|REFLIST01:RefList<T>|} a = default;
                    {|REFLIST01:var|} b = RefList.Empty<T>();
                }
            }
            
            public class Generic<T> where T : unmanaged
            {
                private ExclusiveAccess<{|REFLIST01:RefList<T>|}> _listAccess;
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

    [Fact]
    public async Task RefList03NonSpecializedCall()
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
                    {|REFLIST03:a.CopyTo<TestStruct>(ref b)|};
                    {|REFLIST03:a.CopyFrom<TestStruct>(b)|};
                    
                    {|REFLIST03:a.Dealloc<TestStruct>()|};
                    {|REFLIST03:a.Deallocated<TestStruct>()|} = default;
                    {|REFLIST03:a.Clear<TestStruct>()|};
                    {|REFLIST03:a.RemoveAt<TestStruct>(0)|};
                }
            }
            """
        );
    }
}