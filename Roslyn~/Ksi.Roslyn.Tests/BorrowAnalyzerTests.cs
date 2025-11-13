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

    [Fact]
    public async Task Borrow02AssigningRef()
    {
        await BorrowAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            using System;

            public static class TestClass
            {
                public static void Test(ref RefList<int> list)
                {
                    ref var a = ref list.RefAt(0);
                    {|BORROW02:a = ref list.RefAt(1)|};
                    
                    var span = list.AsSpan();
                    {|BORROW02:span = new Span<int>()|};
                }
            }
            """
        );
    }

    [Fact]
    public async Task Borrow03LocalRefInvalidation()
    {
        await BorrowAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class TestClass
            {
                public static void Test(ref RefList<int> list)
                {
                    foreach (ref var item in list.RefIter())
                    {
                        item++;
                        // Resizing collection within iterator
                        {|BORROW03:list|}.RemoveAt(0);
                    }
                    
                    foreach (ref var item in list.AsSpan())
                    {
                        item++;
                        // Resizing collection within Span-derived iterator
                        {|BORROW03:list|}.RemoveAt(0);
                    }
                    
                    ref var a = ref list.RefAdd();
                    
                    // Resizing collection within item reference lifetime
                    {|BORROW03:list|}.RefAdd() = 42;
                    
                    // Mutating collection without resizing it within item reference lifetime
                    list.RefAt(1) = 42;
                    
                    a = 0;
                    
                    // Mutating collection outside of any item reference lifetime
                    list.Clear();
                }
                
                public static void LoopBodyInvalidation(ref RefList<int> list, int pos)
                {
                    ref var test = ref list.RefAt(pos);
                    for (var i = list.Count() - 1; i >= 0; i++)
                    {
                        test++;
                        list.RemoveAt(i);
                    }
                }
            }
            """
        );
    }

    [Fact]
    public async Task Borrow04ArgumentAliasing()
    {
        await BorrowAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct Item { public RefList<int> List; }
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct { public RefList<Item> Items; }

            public static class TestClass
            {
                public static void Test(ref TestStruct value, int x, int y)
                {
                    SameDyn(ref {|BORROW04:value.Items.RefAt(x)|}, ref {|BORROW04:value.Items.RefAt(y)|});
                    SameDynOneReadOnly(ref {|BORROW04:value.Items.RefAt(x)|}, in value.Items.RefAt(y));
                    SameDynOneNoResize(ref {|BORROW04:value.Items.RefAt(x)|}, ref value.Items.RefAt(y));
                    
                    SameDynReadOnly(in value.Items.RefAt(x), in value.Items.RefAt(y));
                    SameDynNoResize(ref value.Items.RefAt(x), ref value.Items.RefAt(y));
                    SameNonDyn(ref value.Items.RefAt(x).List.RefAt(x), ref value.Items.RefAt(y).List.RefAt(y));
                    
                    ParentChild(ref {|BORROW04:value|}, ref {|BORROW04:value.Items.RefAt(x)|});
                    ParentReadOnlyChild(ref {|BORROW04:value|}, in value.Items.RefAt(x));
                    ReadOnlyParentChild(in value, ref {|BORROW04:value.Items.RefAt(x)|});
                    
                    ReadOnlyParentReadOnlyChild(in value, in value.Items.RefAt(x));
                    NoResizeParentNoResizeChild(ref value, ref value.Items.RefAt(x));
                    ReadOnlyParentNoDynChild(in value, ref value.Items.RefAt(x).List.RefAt(x));
                }
                
                public static void SameDyn(ref Item a, ref Item b) => throw null;
                public static void SameDynOneReadOnly(ref Item a, in Item b) => throw null;
                public static void SameDynOneNoResize(ref Item a, [DynNoResize] ref Item b) => throw null;
                
                public static void SameDynReadOnly(in Item a, in Item b) => throw null;
                public static void SameDynNoResize([DynNoResize] ref Item a, [DynNoResize] ref Item b) => throw null;
                public static void SameNonDyn(ref int a, ref int b) => throw null;
                
                public static void ParentChild(ref TestStruct a, ref Item b) => throw null;
                public static void ParentReadOnlyChild(ref TestStruct a, in Item b) => throw null;
                public static void ReadOnlyParentChild(in TestStruct a, ref Item b) => throw null;
                
                public static void ReadOnlyParentReadOnlyChild(in TestStruct a, in Item b) => throw null;
                public static void NoResizeParentNoResizeChild([DynNoResize] ref TestStruct a, [DynNoResize] ref Item b) => throw null;
                public static void ReadOnlyParentNoDynChild(in TestStruct a, ref int b) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task Borrow05RefEscapesAccessScope()
    {
        await BorrowAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct TestStruct { public RefList<int> List; }
            
            public class TestClass
            {
                private readonly ExclusiveAccess<TestStruct> _test = new ExclusiveAccess<TestStruct>();
                
                public ref int Test()
                {
                    using var test = _test.Mutable;
                    {|BORROW05:return ref test.Value.List.RefAt(0);|}
                }
            }
            """
        );
    }
}