namespace Ksi.Roslyn.Tests;

using RefPathAnalyzerTest = Util.KsiAnalyzerTest<RefPathAnalyzer>;

public class RefPathAnalyzerTests
{
    [Fact]
    public async Task RefPathSmoke()
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
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct RootStruct { public DynStruct Dyn; }

            public static class RefPathExtensions
            {
                [RefPath("self", "Value")]
                public static ref int Value(this ref MyStruct self) => ref self.Value;
                
                
                [RefPath("self", "!", "Single", "Value")]
                public static ref int Value(this ref DynStruct self) => ref self.Single.Value;
                
                [RefPath("self", "!", "Single", "Value")]
                public static ref int ValueCombined(this ref DynStruct self) => ref self.Single.Value();
                
                
                [RefPath("self", "Multiple", "!", "[n]", "Value")]
                public static ref int Value(this ref DynStruct self, int idx) => ref self.Multiple.RefAt(idx).Value;
                
                [RefPath("self", "!", "[n]", "Value")]
                public static ref int Value(this ref RefList<MyStruct> self, int idx) => ref self.RefAt(idx).Value();
                
                [RefPath("self", "Multiple", "!", "[n]", "Value")]
                public static ref int ValueCombined(this ref DynStruct self, int idx) => ref self.Multiple.Value(idx);
                
                
                [RefPath]
                public static ref int Conditional(this ref DynStruct self, bool condition)
                {
                    if (condition)
                        return ref self.Single.Value;
                    
                    return ref self.Multiple.RefAt(42).Value;
                }
                
                [RefPath("self", "Dyn", "!", "Conditional()")]
                public static ref int Conditional(this ref RootStruct self, bool condition)
                {
                    return ref self.Dyn.Conditional(condition);
                }
            }
            """
        );
    }

    [Fact]
    public async Task RefPath01InvalidSignature()
    {
        await RefPathAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            using System;
            
            public struct MyStruct { public int Value; }
            
            public static class RefPathExtensions
            {
                [RefPath] public static ref int ValidRefPath(this ref MyStruct self) => throw null;
                [RefPath] public static ref int {|REFPATH01:NonExtension|}(ref MyStruct self) => throw null;
                [RefPath] public static int {|REFPATH01:NonRefOutput|}(this ref MyStruct self) => throw null;
                [RefPath] public static ref int {|REFPATH01:NonRefInput|}(this MyStruct self) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task RefPath02InvalidDeclaration()
    {
        await RefPathAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public struct MyStruct { public int Value; }
            
            [ExplicitCopy, DynSized, Dealloc]
            public struct DynStruct
            {
                public RefList<MyStruct> List;
            }

            public static class RefPathExtensions
            {
                [RefPath("self", "List", "!", "[n]", "Value")]
                public static ref int Valid(this ref DynStruct self, int idx) => throw null;
                
                [RefPath("self", null, "!", "[n]", "Value")]
                public static ref int {|REFPATH02:NullIdent|}(this ref DynStruct self, int idx) => throw null;
                
                [RefPath("self", "*List", "!", "[n]", "Value")]
                public static ref int {|REFPATH02:InvalidIdent|}(this ref DynStruct self, int idx) => throw null;
                
                [RefPath("self", "List", "!", "[n]", "!", "Value")]
                public static ref int {|REFPATH02:TwoSeparators|}(this ref DynStruct self, int idx) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task RefPath03InvalidDeclaredRoot()
    {
        await RefPathAnalyzerTest.RunAsync(
            // language=cs
            """
            public struct MyStruct { public int Value; }

            public static class RefPathExtensions
            {
                [Ksi.RefPath("self", "Value")]
                public static ref int Valid(this ref MyStruct self, ref MyStruct other) => throw null;
                
                [Ksi.RefPath("other", "Value")]
                public static ref int {|REFPATH03:Invalid|}(this ref MyStruct self, ref MyStruct other) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task RefPath04InvalidReturnExpr()
    {
        await RefPathAnalyzerTest.RunAsync(
            // language=cs
            """
            public struct MyStruct { public int Value; }

            public static class RefPathExtensions
            {
                [Ksi.RefPath]
                public static ref int Valid(this ref MyStruct self) => ref self.Value;
                
                [Ksi.RefPath]
                public static ref int Invalid(this ref MyStruct self) => ref {|REFPATH04:NonRefPath(ref self)|};
                
                public static ref int NonRefPath(ref MyStruct v) => ref v.Value;
            }
            """
        );
    }
}