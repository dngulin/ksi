namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = Util.KsiAnalyzerTest<ExplicitCopyAnalyzer>;

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
    public async Task ExpCopy02ArgumentCopy()
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
    public async Task ExpCopy04AssignmentCopy()
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
    public async Task ExpCopy06ClosureCapture()
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
    public async Task ExpCopy07Boxing()
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
    public async Task ExpCopy11TypeArgument()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
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
                public {|EXPCOPY11:Generic<MyStruct>|} Field;
                
                public static void Method(in {|EXPCOPY11:Generic<MyStruct>|} arg)
                {
                    {|EXPCOPY11:Generic<MyStruct>|} a = default;
                    {|EXPCOPY11:var|} b = new {|EXPCOPY11:Generic<MyStruct>|}();
                    {|EXPCOPY11:var|} c = new {|EXPCOPY11:MyStruct[10]|};
                }
                
                public static void ValidGenerics(
                    in RefList<MyStruct> refList,
                    in TempRefList<MyStruct> tempRefList,
                    in ManagedRefList<MyStruct> managedRefList,
                    Span<MyStruct> rwSpan,
                    ReadOnlySpan<MyStruct> roSpan,
                    ExclusiveAccess<MyStruct> exAcc,
                    MutableAccessScope<MyStruct> rwScope,
                    ReadOnlyAccessScope<MyStruct> roScope
                )
                {
                }
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy12SpanCopy()
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

    [Fact]
    public async Task ExpCopy13LowAccessibility()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            public class Test
            {
                [Ksi.ExplicitCopy]
                private struct {|EXPCOPY13:PrivStruct|} { public int Field; }
                
                [Ksi.ExplicitCopy]
                private protected struct {|EXPCOPY13:ProvProtStruct|} { public int Field; }
                
                [Ksi.ExplicitCopy]
                protected struct {|EXPCOPY13:ProtStruct|} { public int Field; }
                
                [Ksi.ExplicitCopy]
                internal struct IntStruct { public int Field; }
                
                [Ksi.ExplicitCopy]
                protected internal struct ProtIntStruct { public int Field; }
                
                [Ksi.ExplicitCopy]
                public struct PubStruct { public int Field; }
            }
            """
        );
    }
}