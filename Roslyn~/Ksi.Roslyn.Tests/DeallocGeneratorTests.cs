using Ksi.Roslyn.Tests.Util;
using static Ksi.Roslyn.DeallocTemplates;

namespace Ksi.Roslyn.Tests;

using DeallocGeneratorTest = KsiGeneratorTest<DeallocGenerator>;

public class DeallocGeneratorTests
{
    [Fact]
    public async Task Smoke()
    {
        await DeallocGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc]
            public struct MyStruct 
            {
                public int Primitive;
                public RefList<int> Collection;
            }
            """,
            "MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;

            public static class MyStruct_Dealloc
            {
                public static void Dealloc(this ref MyStruct self)
                {
                    self.Collection.Dealloc();
                }
            
            """ +
            DeallocatedExtension.IndentFormat(1, "MyStruct") +
            RefListDeallocItemsAndSelf.IndentFormat(1, "RefList", "MyStruct") +
            RefListDeallocOnlyItems.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListDeallocOnlyItems.IndentFormat(1, "ManagedRefList", "MyStruct") +
            RefListDeallocated.IndentFormat(1, "RefList", "MyStruct") +
            RefListDeallocated.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListDeallocated.IndentFormat(1, "ManagedRefList", "MyStruct") +
            RefListSpecialized.IndentFormat(1, "RefList", "MyStruct") +
            RefListSpecialized.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListSpecialized.IndentFormat(1, "ManagedRefList", "MyStruct") +
            """
            }

            """
        );
    }
}