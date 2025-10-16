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
}