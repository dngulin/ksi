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
            """
        );
    }

    [Fact]
    public async Task KsiQuery01InvalidField()
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
    public async Task KsiQuery02RepeatedComponent()
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
    public async Task KsiQuery03NonPartialKsiDomain()
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
    public async Task KsiComp04LowAccessibility()
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
}