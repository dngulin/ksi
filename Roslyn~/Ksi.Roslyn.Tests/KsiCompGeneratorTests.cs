using Ksi.Roslyn.Tests.Util;
using static Ksi.Roslyn.KsiCompTemplates;

namespace Ksi.Roslyn.Tests;

using KsiCompGeneratorTest = KsiGeneratorTest<KsiCompGenerator>;

public class KsiCompGeneratorTests
{
    [Fact]
    public async Task KsiHandleIsProduced()
    {
        const string sectionEnumItems =
            """
            Section1 = 1,
            Section2 = 2
            """;

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
            string.Format(KsiHandle, "Domain", sectionEnumItems.WithIndent(2)) + '\n'
        );
    }
}