using Ksi.Roslyn.Tests.Util;
using static Ksi.Roslyn.KsiCompTemplates;

namespace Ksi.Roslyn.Tests;

using KsiCompGeneratorTest = KsiGeneratorTest<KsiCompGenerator>;

public class KsiCompGeneratorTests
{
    [Fact]
    public async Task KsiHandleIsProduced()
    {
        await KsiCompGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [KsiComponent] public struct CompA { public int Data; }
            [KsiEntity] public struct Entity { public CompA A; }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct Domain
            {
                public RefList<Entity> Section1;
                public RefList<Entity> Section2;
            }
            """,
            "Domain.KsiHandle.g.cs",
            string.Format(
                KsiHandle,
                "public",
                "Domain",
                """
                Section1 = 1,
                Section2 = 2
                """.WithNewLineIndent(2)
            ) + '\n'
        );
    }

    [Fact]
    public async Task ArchetypeExtensionsAreProduced()
    {
        await KsiCompGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] public struct CompA { public int Data; }
            [KsiComponent] public struct CompB { public int Data; }

            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            public struct Archetype
            {
                public RefList<CompA> A;
                public RefList<CompB> B;
            }
            """,
            "Archetype.KsiArchetypeExtensions.g.cs",
            """
            using Ksi;
            
            public static class Archetype_KsiArchetypeExtensions
            {
            
            """ +
            string.Format(
                ArchetypeExtensions,
                "Archetype",
                "return self.A.Count();",
                """
                self.A.RefAdd();
                self.B.RefAdd();
                """.WithNewLineIndent(1),
                """
                self.A.RemoveAt(index);
                self.B.RemoveAt(index);
                """.WithNewLineIndent(1),
                """
                self.A.Clear();
                self.B.Clear();
                """.WithNewLineIndent(1)
            ).Indented(1) +
            """
           
            }
            
            """
        );
    }

    [Fact]
    public async Task KsiQueryIsProduced()
    {
        await KsiCompGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiComponent] internal struct CompA { public int Data; }
            [KsiEntity] internal struct Entity { public CompA A; }
            
            [KsiArchetype]
            [ExplicitCopy, DynSized, Dealloc]
            internal struct Archetype { public RefList<CompA> A; }
            
            [KsiDomain]
            [ExplicitCopy, DynSized, Dealloc]
            internal partial struct Domain
            {
                public Archetype SoA;
                public RefList<Entity> AoS;
            }
            
            internal static partial class TestSystem
            {
                [KsiQuery]
                private static void Tick(in Domain.KsiHandle h, ref CompA a) {}
            }
            """,
            "Archetype.KsiArchetypeExtensions.g.cs",
            """
            using Ksi;

            internal static class Archetype_KsiArchetypeExtensions
            {

            """ +
            string.Format(
                ArchetypeExtensions,
                "Archetype",
                "return self.A.Count();",
                "self.A.RefAdd();",
                "self.A.RemoveAt(index);",
                "self.A.Clear();"
            ).Indented(1) + '\n' +
            """
            }

            """,
            "Domain.KsiHandle.g.cs",
            string.Format(
                KsiHandle,
                "internal",
                "Domain",
                """
                SoA = 1,
                AoS = 2
                """.WithNewLineIndent(2)
            ) + '\n',
            "TestSystem.Tick.KsiQuery.g.cs",
            // language=cs
            """
            using Ksi;
            
            internal static partial class TestSystem
            {
                public static void Tick([DynNoResize] ref Domain domain)
                {
                    var handle = new Domain.KsiHandle(Domain.KsiSection.SoA, 0);
                    for (handle.Index = 0; handle.Index < domain.SoA.Count(); handle.Index++)
                    {
                        ref var archetype = ref domain.SoA;
                        Tick(in handle, ref archetype.A.RefAt(handle.Index));
                    }
                    
                    handle.Section = Domain.KsiSection.AoS;
                    for (handle.Index = 0; handle.Index < domain.AoS.Count(); handle.Index++)
                    {
                        ref var entity = ref domain.AoS.RefAt(handle.Index);
                        Tick(in handle, ref entity.A);
                    }
                }
            }
            
            """
        );
    }
}