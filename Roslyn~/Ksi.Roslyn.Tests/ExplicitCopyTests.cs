namespace Ksi.Roslyn.Tests;

using ExplicitCopyAnalyzerTest = KsiAnalyzerTest<ExplicitCopyAnalyzer>;

public class ExplicitCopyTests
{
    [Fact]
    public async Task ExpCopy01()
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
    public async Task ExpCopy02()
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
}