namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = KsiAnalyzerTest<ExplicitCopyAnalyzer>;

public class ExplicitCopyAnalyzerTests
{
    [Fact]
    public async Task ExpCopy02CopiedByPassingByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void Method(in MyStruct refParam, MyStruct valParam)
                {
                    ReceiveByValue({|EXPCOPY02:refParam|});
                    ReceiveByValue({|EXPCOPY02:valParam|});
                    
                    var local = new MyStruct();
                    ReceiveByValue({|EXPCOPY02:local|});
                    
                    ref var localRef = ref local;
                    ReceiveByValue({|EXPCOPY02:localRef|});
                    
                    ReceiveByValue(new MyStruct());
                    ReceiveByValue(default);
                    ReceiveByValue(CreateMyStruct());
                }
                
                private static void ReceiveByValue(MyStruct arg) {}
                
                [Ksi.ExplicitCopyReturn]
                private static MyStruct CreateMyStruct() => default;
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy03UsedAsFieldOfNonExplicitCopy()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            [Ksi.ExplicitCopy]
            public struct MarkedStruct { public MyStruct Field; }
            
            public struct NonMarkedStruct { public MyStruct {|EXPCOPY03:Field|}; }
            """
        );
    }

    [Fact]
    public async Task ExpCopy04CopiedByBoxing()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void Box(object arg) {}
                
                public static void Method(in MyStruct value)
                {
                    Box({|EXPCOPY04:new MyStruct()|});
                    Box({|EXPCOPY04:value|});
                    var x = {|EXPCOPY04:(object)value|};
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy05CopiedByClosureCapture()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void Method(in MyStruct value) {}
            
                public static void Caller()
                {
                    var {|EXPCOPY05:x|} = new MyStruct();
                    System.Action f = () => { Method(x); };
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy06ReturnedByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }

            public static class Test
            {
                public static MyStruct {|EXPCOPY06:NonMarkedReturn|}() => default;
                
                [Ksi.ExplicitCopyReturn]
                public static MyStruct MarkedReturn() => default;
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy07CopiedByAssignment()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }

            public static class Test
            {
                public static void Method(ref MyStruct value)
                {
                    var {|EXPCOPY07:a = value|};
                    var b = new MyStruct();
                    {|EXPCOPY07:value = b|};
                    value = new MyStruct();
                    value = default;
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy08HasPrivateField()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct
            {
                public int A;
                private int {|EXPCOPY08:B|};
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy09DeclaredAsAGenericType()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct {|EXPCOPY09:MyStruct|}<T>
            {
                public T Field;
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy10PassedAsGenericMethodArgument()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void GenericMethod<T>(in T value) {}
                public static void SafeGenericMethod<[Ksi.ExplicitCopy] T>(in T value) {}
            
                public static void Caller()
                {
                    var a = new MyStruct();
                    GenericMethod({|EXPCOPY10:a|});
                    SafeGenericMethod(a);
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy11UsedAsGenericTypeArgument()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            public struct Generic<T> { public T Field; }
            
            [Ksi.ExplicitCopy]
            public struct Test
            {
                public {|EXPCOPY11:Generic<MyStruct>|} Field;
                
                public static void Method(in {|EXPCOPY11:Generic<MyStruct>|} arg)
                {
                    {|EXPCOPY11:Generic<MyStruct>|} a = default;
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy12UsedAsGenericTypeArgument()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            using System;
            
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }

            public static class Test
            {
                public static void Method(Span<MyStruct> rw, ReadOnlySpan<MyStruct> ro)
                {
                    {|EXPCOPY12:ro.CopyTo(rw)|};
                    {|EXPCOPY12:ro.TryCopyTo(rw)|};
                    
                    var span = new Span<MyStruct>();
                    {|EXPCOPY12:rw.CopyTo(span)|};
                    {|EXPCOPY12:rw.TryCopyTo(span)|};
                    
                    {|EXPCOPY12:ro.ToArray()|};
                    {|EXPCOPY12:rw.ToArray()|};
                }
            }
            """
        );
    }
}