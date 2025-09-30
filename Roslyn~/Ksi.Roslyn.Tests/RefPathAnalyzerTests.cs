namespace Ksi.Roslyn.Tests;

using RefPathAnalyzerTest = Util.KsiAnalyzerTest<RefPathAnalyzer>;

public class RefPathAnalyzerTests
{
    [Fact]
    public async Task ValidDefinitions()
    {
        await RefPathAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            public struct MyStruct { public int Value; }
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct DynStruct
            {
                public MyStruct Single;
                public RefList<MyStruct> Multiple;
            }

            public static class RefPathExtensions
            {
                [RefPath("self", "Value")]
                public static ref int Value(this ref MyStruct self) => ref self.Value;
                
                [RefPath("self", "!", "Single", "Value")]
                public static ref int Value(this ref DynStruct self) => ref self.Single.Value;
                
                [RefPath("self", "Multiple", "!", "[n]", "Value")]
                public static ref int Value(this ref DynStruct self, int idx) => ref self.Multiple.RefAt(idx).Value;
            }
            """
        );
    }
}