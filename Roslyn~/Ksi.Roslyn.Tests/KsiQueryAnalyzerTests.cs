namespace Ksi.Roslyn.Tests;

using KsiQueryAnalyzerTest = Util.KsiAnalyzerTest<KsiQueryAnalyzer>;

public class KsiQueryAnalyzerTests
{
    [Fact]
    public async Task Smoke()
    {
        await KsiQueryAnalyzerTest.RunAsync(
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
            public struct Domain
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
        await KsiQueryAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiComponent] public struct CompB { public int Data; }

            [KsiEntity]
            public struct Entity
            {
                public CompA A;
                private CompB {|KSIQUERY01:B|}; // Wrong access modifier
                public int {|KSIQUERY01:C|}; // Wrong type
            }
            
            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A;
                private RefList<CompB> {|KSIQUERY01:B|}; // Wrong access modifier
                public RefList<int> {|KSIQUERY01:C|}; // Wrong type
            }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Domain
            {
                private Archetype {|KSIQUERY01:SoA|}; // Wrong access modifier
                public RefList<int> {|KSIQUERY01:AoS|}; // Wrong type
            }
            """
        );
    }

    [Fact]
    public async Task KsiQuery02RepeatedComponent()
    {
        await KsiQueryAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }

            [KsiEntity]
            public struct Entity
            {
                public CompA A1;
                public CompA {|KSIQUERY02:A2|};
                public CompA {|KSIQUERY02:A3|};
            }

            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A1;
                public RefList<CompA> {|KSIQUERY02:A2|};
                public RefList<CompA> {|KSIQUERY02:A3|};
            }
            """
        );
    }
}