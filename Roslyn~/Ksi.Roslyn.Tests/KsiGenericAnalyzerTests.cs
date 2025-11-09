namespace Ksi.Roslyn.Tests;

using KsiGenericAnalyzerTest = Util.KsiAnalyzerTest<KsiGenericAnalyzer>;

public class KsiGenericAnalyzerTests
{
    [Fact]
    public async Task KsiGeneric01GenericMethodArgumentTraits()
    {
        await KsiGenericAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            [ExplicitCopy] public struct ExpCopyType { public int Value; }
            [ExplicitCopy, DynSized, Dealloc] public struct DeallocType { public RefList<int> Value; }
            
            public static class GenericTest
            {
                public static void NonTrait(in int value)
                {
                    Generic(value);
                    ExpCopyGeneric(value);
                    DeallocGeneric(value);
                }
            
                public static void ExpCopy(in ExpCopyType value)
                {
                    Generic({|KSIGENERIC01:value|});
                    ExpCopyGeneric(value);
                    DeallocGeneric(value);
                }
                
                public static void Dealloc(in DeallocType value)
                {
                    Generic({|KSIGENERIC01:value|});
                    ExpCopyGeneric({|KSIGENERIC01:value|});
                    DeallocGeneric(value);
                }
                
                public static void Generic<T>(in T value) where T : unmanaged => throw null;
                public static void ExpCopyGeneric<[ExplicitCopy] T>(in T value) where T : unmanaged => throw null;
                public static void DeallocGeneric<[ExplicitCopy, Dealloc] T>(in T value) where T : unmanaged => throw null;
            }

            public static class InnerGenericTest
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
    public async Task KsiGeneric01NonSpecializedCall()
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
    public async Task KsiGeneric02JaggedRefList()
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

    [Fact]
    public async Task KsiGeneric03GenericTypeArgumentTraits()
    {
        await KsiGenericAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            using System;

            [ExplicitCopy]
            public struct MyStruct { public int Field; }
            public struct Generic<T> { public T Field; }

            [ExplicitCopy]
            public struct Test
            {
                public Generic<{|KSIGENERIC03:MyStruct|}> Field;
                
                public static void Method(in Generic<{|KSIGENERIC03:MyStruct|}> arg)
                {
                    Generic<{|KSIGENERIC03:MyStruct|}> a = default;
                    {|KSIGENERIC03:var|} b = new Generic<{|KSIGENERIC03:MyStruct|}>();
                    {|KSIGENERIC03:var|} c = new {|KSIGENERIC03:MyStruct[10]|};
                    {|KSIGENERIC03:var|} d = (new MyStruct(), 42);
                }
            }
            """
        );
    }

    [Fact]
    public async Task KsiGeneric03TempAllocContainers()
    {
        await KsiGenericAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct TestStruct { public TempRefList<int> List; }

            public static class TestClass
            {
                public static void Test(
                    in TempRefList<TestStruct> a,
                    in RefList<{|KSIGENERIC03:TestStruct|}> b,
                    in ManagedRefList<{|KSIGENERIC03:TestStruct|}> c
                )
                {
                    var x = TempRefList.Empty<TestStruct>();
                    {|KSIGENERIC03:var|} y = RefList.Empty<TestStruct>();
                    {|KSIGENERIC03:var|} z = ManagedRefList.Empty<TestStruct>();
                }
            }
            """
        );
    }
}