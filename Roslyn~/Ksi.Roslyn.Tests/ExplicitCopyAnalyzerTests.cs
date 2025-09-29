namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = KsiAnalyzerTest<ExplicitCopyAnalyzer>;

public class ExplicitCopyAnalyzerTests
{

    [Fact]
    public async Task ExpCopy01MissingAttr()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }

            [Ksi.ExplicitCopy]
            public struct MarkedStruct { public MyStruct Field; }

            public struct {|EXPCOPY01:NonMarkedStruct|} { public MyStruct Field; }
            """
        );
    }

    [Fact]
    public async Task ExpCopy02CopiedByPassingByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void Method(ref MyStruct refParam, MyStruct valParam)
                {
                    var localVar = new MyStruct();
                    ref var localRef = ref localVar;
                    
                    ReceiveByValue({|EXPCOPY02:refParam|});
                    ReceiveByValue({|EXPCOPY02:valParam|});
                    ReceiveByValue({|EXPCOPY02:localVar|});
                    ReceiveByValue({|EXPCOPY02:localRef|});
                    
                    ReceiveByValue(refParam.Move());
                    ReceiveByValue(valParam.Move());
                    ReceiveByValue(localVar.Move());
                    ReceiveByValue(localRef.Move());
                    
                    ReceiveByValue(new MyStruct());
                    ReceiveByValue(default);
                    
                    ReceiveByValue(new MyStruct { Field = 7 });
                    ReceiveByValue(CreateMyStruct());
                }
                
                private static void ReceiveByValue(MyStruct arg) {}
                
                private static MyStruct CreateMyStruct() => new MyStruct { Field = 42 };
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy03ReturningCopy()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }

            public static class Test
            {
                public static MyStruct Default => default;
                
                public static MyStruct ReturnLocal() 
                {
                    var value = new MyStruct();
                    value.Field = 42;
                    return value;
                }
                
                public static MyStruct ReturnValArg(MyStruct value) 
                {
                    value.Field = 42;
                    return value;
                }
                
                public static MyStruct ReturnRefArg(ref MyStruct value) 
                {
                    value.Field = 42;
                    {|EXPCOPY03:return value;|}
                }
                
                public static MyStruct ReturnRefLocal(ref MyStruct value) 
                {
                    ref var refLocal = ref value;
                    {|EXPCOPY03:return refLocal;|}
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy04CopiedByAssignment()
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
                    var {|EXPCOPY04:a = value|};
                    
                    var b = new MyStruct();
                    {|EXPCOPY04:value = b|};
                    
                    value = new MyStruct();
                    value = default;
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy05DefensiveCopy()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct 
            {
                public int Field;
                
                public void SetValue(int value) => Field = value;
            }

            public static class Test
            {
                public static void Method(in MyStruct value)
                {
                    {|EXPCOPY05:value.SetValue(0)|};
                    
                    var local = new MyStruct();
                    local.SetValue(1);
                    
                    ref readonly var localRef = ref local;
                    {|EXPCOPY05:localRef.SetValue(2)|};
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy06CopiedByClosureCapture()
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
                    var {|EXPCOPY06:x|} = new MyStruct();
                    System.Action f = () => { Method(x); };
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy07CopiedByBoxing()
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
                    Box({|EXPCOPY07:new MyStruct()|});
                    Box({|EXPCOPY07:value|});
                    var x = {|EXPCOPY07:(object)value|};
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