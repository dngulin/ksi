namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = KsiAnalyzerTest<ExplicitCopyAnalyzer>;

public class ExplicitCopyAnalyzerTests
{
    [Fact]
    public async Task ExpCopy01ReceivedByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            [Ksi.ExplicitCopy]
            public struct MyStruct { public int Field; }
            
            public static class Test
            {
                public static void Method(MyStruct {|EXPCOPY01:arg|}) {}
            }
            """
        );
    }

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
                public static void Method(MyStruct {|EXPCOPY01:arg|}) {}
                
                public static void Caller() => Method({|EXPCOPY02:new MyStruct()|});
                public static void Caller(in MyStruct value) => Method({|EXPCOPY02:value|});
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
}