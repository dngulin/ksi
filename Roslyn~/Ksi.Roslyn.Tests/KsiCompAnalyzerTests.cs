namespace Ksi.Roslyn.Tests;

using KsiCompAnalyzerTest = Util.KsiAnalyzerTest<KsiCompAnalyzer>;

public class KsiCompAnalyzerTests
{
    [Fact]
    public async Task Smoke()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [KsiComponent] public struct CompA { public int Data; }
            [KsiComponent] public struct CompB { public int Data; }
            
            [KsiEntity]
            public struct Entity
            {
                public CompA A;
                public CompB B;
            }
            
            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A;
                public RefList<CompB> B;
            }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain
            {
                public Archetype SoA;
                public RefList<Entity> AoS;
            }
            
            public struct ExtraData {}
            
            public static partial class TestSystem
            {
                [KsiQuery]
                private static void Tick(in Domain.KsiHandle handle, ref CompA a, in CompB b, [KsiQueryParam] in ExtraData data)
                {
                }
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp01InvalidField()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiComponent] public struct CompB { public int Data; }

            [KsiEntity]
            public struct Entity
            {
                public CompA A;
                private CompB {|KSICOMP01:B|}; // Wrong access modifier
                public int {|KSICOMP01:C|}; // Wrong type
            }
            
            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A;
                private RefList<CompB> {|KSICOMP01:B|}; // Wrong access modifier
                public RefList<int> {|KSICOMP01:C|}; // Wrong type
            }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain
            {
                private Archetype {|KSICOMP01:SoA|}; // Wrong access modifier
                public RefList<int> {|KSICOMP01:AoS|}; // Wrong type
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp02RepeatedComponent()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }

            [KsiEntity]
            public struct Entity
            {
                public CompA A1;
                public CompA {|KSICOMP02:A2|};
                public CompA {|KSICOMP02:A3|};
            }

            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A1;
                public RefList<CompA> {|KSICOMP02:A2|};
                public RefList<CompA> {|KSICOMP02:A3|};
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp03InvalidDomain()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public struct {|KSICOMP03:Domain|}
            {
                public RefList<Entity> AoS;
            }
            
            public static class Wrap
            {
                [KsiDomain]
                [ExplicitCopy, DynSized, Dealloc]
                public partial struct {|KSICOMP03:Domain|}
                {
                    public RefList<Entity> AoS;
                }
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp04LowArchetypeAccessibility()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            public class Test
            {
                [Ksi.KsiArchetype]
                private struct {|KSICOMP04:PrivStruct|} { }
                
                [Ksi.KsiArchetype]
                private protected struct {|KSICOMP04:ProvProtStruct|} { }
                
                [Ksi.KsiArchetype]
                protected struct {|KSICOMP04:ProtStruct|} { }
                
                [Ksi.KsiArchetype]
                internal struct IntStruct { }
                
                [Ksi.KsiArchetype]
                protected internal struct ProtIntStruct { }
                
                [Ksi.KsiArchetype]
                public struct PubStruct { }
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp05InvalidQueryContainingType()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain { public RefList<Entity> AoS; }
            
            public class {|KSICOMP05:TopLevel|} // Non-partial type
            {
                [KsiQuery]
                private static void Test(in Domain.KsiHandle h, ref CompA a) {}
                
                public partial class {|KSICOMP05:Inner|} // Non top-level type
                {
                    [KsiQuery]
                    private static void TestInner(in Domain.KsiHandle h, ref CompA a) {}
                }
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp06InvalidQueryMethod()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }

            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain { public RefList<Entity> AoS; }
            
            public struct ExtraData {}

            public partial class TestSystem 
            {
                [KsiQuery]
                private static void Valid(in Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private void {|KSICOMP06:NonStatic|}(in Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static int {|KSICOMP06:NonVoid|}(in Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) => 0;
                
                [KsiQuery]
                private static void {|KSICOMP06:NoHandle|}(ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void {|KSICOMP06:NonInHandle|}(Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void {|KSICOMP06:NoComponents1|}(in Domain.KsiHandle h, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void {|KSICOMP06:NoComponents2|}(in Domain.KsiHandle h) {}
                
                [KsiQuery]
                private static void {|KSICOMP06:RepComponent|}(in Domain.KsiHandle h, ref CompA a, ref CompA b) {}
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp07NonRefQueryParameter()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }

            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain { public RefList<Entity> AoS; }

            public struct ExtraData {}

            public partial class TestSystem 
            {
                [KsiQuery]
                private static void Valid(in Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void NonRef(in Domain.KsiHandle h, CompA {|KSICOMP07:a|}, [KsiQueryParam] ExtraData {|KSICOMP07:data|}) {}
            }
            """
        );
    }

    [Fact]
    public async Task KsiComp08InvalidQueryParameterType()
    {
        await KsiCompAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }

            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain { public RefList<Entity> AoS; }

            public struct ExtraData {}

            public partial class TestSystem 
            {
                [KsiQuery]
                private static void Valid(in Domain.KsiHandle h, ref CompA a, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void Valid(in Domain.KsiHandle h, ref CompA a, ref ExtraData {|KSICOMP08:b|}, [KsiQueryParam] in ExtraData data) {}
                
                [KsiQuery]
                private static void NonStruct(in Domain.KsiHandle h, ref CompA a, ref object {|KSICOMP08:b|}, [KsiQueryParam] ref object {|KSICOMP08:data|}) {}
            }
            """
        );
    }
}