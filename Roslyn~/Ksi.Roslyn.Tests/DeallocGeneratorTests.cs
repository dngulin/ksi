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

            /// <summary>
            /// Deallocation extensions for MyStruct
            /// </summary>
            public static class MyStruct_Dealloc
            {
                /// <summary>
                /// Deallocate all owned resources by the structure.
                /// </summary>
                /// <param name="self">structure to deallocate</param>
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

    [Fact]
    public async Task OnlyManagedRefListExtensions()
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
                public ManagedRefList<int> Collection2;
            }
            """,
            "MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;

            /// <summary>
            /// Deallocation extensions for MyStruct
            /// </summary>
            public static class MyStruct_Dealloc
            {
                /// <summary>
                /// Deallocate all owned resources by the structure.
                /// </summary>
                /// <param name="self">structure to deallocate</param>
                public static void Dealloc(this ref MyStruct self)
                {
                    self.Collection.Dealloc();
                }

            """ +
            DeallocatedExtension.IndentFormat(1, "MyStruct") +
            RefListDeallocOnlyItems.IndentFormat(1, "ManagedRefList", "MyStruct") +
            RefListDeallocated.IndentFormat(1, "ManagedRefList", "MyStruct") +
            RefListSpecialized.IndentFormat(1, "ManagedRefList", "MyStruct") +
            """
            }

            """
        );
    }

    [Fact]
    public async Task OnlyTempRefListExtensions()
    {
        await DeallocGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc, TempAlloc]
            public struct MyStruct 
            {
                public int Primitive;
                public RefList<int> Collection;
                public TempRefList<int> Collection2;
            }
            """,
            "MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;

            /// <summary>
            /// Deallocation extensions for MyStruct
            /// </summary>
            public static class MyStruct_Dealloc
            {
                /// <summary>
                /// Deallocate all owned resources by the structure.
                /// </summary>
                /// <param name="self">structure to deallocate</param>
                public static void Dealloc(this ref MyStruct self)
                {
                    self.Collection.Dealloc();
                }

            """ +
            DeallocatedExtension.IndentFormat(1, "MyStruct") +
            RefListDeallocOnlyItems.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListDeallocated.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListSpecialized.IndentFormat(1, "TempRefList", "MyStruct") +
            """
            }

            """
        );
    }

    [Fact]
    public async Task NoRefListExtensions()
    {
        await DeallocGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, Dealloc, TempAlloc]
            public struct MyStruct 
            {
                public int Primitive;
                public RefList<int> Collection;
                public TempRefList<int> Collection2;
                public ManagedRefList<int> Collection3;
            }
            """,
            "MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;

            /// <summary>
            /// Deallocation extensions for MyStruct
            /// </summary>
            public static class MyStruct_Dealloc
            {
                /// <summary>
                /// Deallocate all owned resources by the structure.
                /// </summary>
                /// <param name="self">structure to deallocate</param>
                public static void Dealloc(this ref MyStruct self)
                {
                    self.Collection.Dealloc();
                }

            """ +
            DeallocatedExtension.IndentFormat(1, "MyStruct") +
            """
            }

            """
        );
    }

    [Fact]
    public async Task NamespaceIsPickedUp()
    {
        await DeallocGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            namespace Test.Ns
            {
                [ExplicitCopy, DynSized, Dealloc, TempAlloc]
                public struct MyStruct 
                {
                    public int Primitive;
                    public RefList<int> Collection;
                    public TempRefList<int> Collection2;
                    public ManagedRefList<int> Collection3;
                }
            }
            """,
            "MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;

            namespace Test.Ns
            {
                /// <summary>
                /// Deallocation extensions for MyStruct
                /// </summary>
                public static class MyStruct_Dealloc
                {
                    /// <summary>
                    /// Deallocate all owned resources by the structure.
                    /// </summary>
                    /// <param name="self">structure to deallocate</param>
                    public static void Dealloc(this ref MyStruct self)
                    {
                        self.Collection.Dealloc();
                    }

            """ +
            DeallocatedExtension.IndentFormat(2, "MyStruct") +
            """
                }
            }

            """
        );
    }

    [Fact]
    public async Task EnclosingTypeNameIsPickedUp()
    {
        await DeallocGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class EnclosingType
            {
                [ExplicitCopy, DynSized, Dealloc, TempAlloc]
                public struct MyStruct 
                {
                    public int Primitive;
                    public RefList<int> Collection;
                    public TempRefList<int> Collection2;
                    public ManagedRefList<int> Collection3;
                }
            }
            """,
            "EnclosingType.MyStruct.Dealloc.g.cs",
            // language=cs
            """
            using Ksi;
            
            /// <summary>
            /// Deallocation extensions for EnclosingType.MyStruct
            /// </summary>
            public static class EnclosingType_MyStruct_Dealloc
            {
                /// <summary>
                /// Deallocate all owned resources by the structure.
                /// </summary>
                /// <param name="self">structure to deallocate</param>
                public static void Dealloc(this ref EnclosingType.MyStruct self)
                {
                    self.Collection.Dealloc();
                }

            """ +
            DeallocatedExtension.IndentFormat(1, "EnclosingType.MyStruct") +
            """
            }

            """
        );
    }
}