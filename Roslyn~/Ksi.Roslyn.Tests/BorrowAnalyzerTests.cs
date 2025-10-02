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
                    
                    ref var a = ref list.RefAt(0);
                    
                    // Resizing collection within item reference lifetime
                    {|BORROW03:list|}.RefAdd() = 42;
                    
                    // Mutating collection without resizing it within item reference lifetime
                    list.RefAt(1) = 42;
                    
                    a = 0;
                    
                    // Mutating collection outside of any item reference lifetime
                    list.Clear();
                }
                
            }
            """
        );
    }
}