namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = KsiAnalyzerTest<ExplicitCopyAnalyzer>;

public class ExplicitCopyTests
{
    [Fact]
    public async Task ExpCopy01ReceivedByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            public static class Test {
                [Ksi.ExplicitCopy]
                public struct MyStruct { public int Field; }
                
                public static void Method(MyStruct {|EXPCOPY01:arg|}) {}
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy02PassedByValue()
    {
        await ExplicitCopyAnalyzerTest.RunAsync(
            // language=cs
            """
            public static class Test {
                [Ksi.ExplicitCopy]
                public struct MyStruct { public int Field; }
                
                public static void Method(MyStruct {|EXPCOPY01:arg|}) {}
                
                public static void Caller() => Method({|EXPCOPY02:new MyStruct()|});
                public static void Caller(in MyStruct value) => Method({|EXPCOPY02:value|});
            }
            """
        );
    }

    [Fact]
    public async Task ExpCopy03FieldOfNonExplicitCopy()
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
}