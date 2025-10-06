using Ksi.Roslyn.Tests.Util;
using static Ksi.Roslyn.ExplicitCopyTemplates;

namespace Ksi.Roslyn.Tests;

using ExplicitCopyGeneratorTest = KsiGeneratorTest<ExplicitCopyGenerator>;

public class ExplicitCopyGeneratorTests
{
    [Fact]
    public async Task Smoke()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
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
            "MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            public static class MyStructExplicitCopy
            {
                public static void CopyFrom(this ref MyStruct self, in MyStruct other)
                {
                    self.Primitive = other.Primitive;
                    self.Collection.CopyFrom(other.Collection);
                }
                
                public static void CopyTo(this in MyStruct self, ref MyStruct other)
                {
                    other.CopyFrom(self);
                }
            
            """ +
            RefListExtensionsForDeallocItems.IndentFormat(1, "RefList", "MyStruct") +
            RefListExtensionsForDeallocItems.IndentFormat(1, "TempRefList", "MyStruct") +
            RefListExtensionsForDeallocItems.IndentFormat(1, "ManagedRefList", "MyStruct") +
            """
            }
            
            """
        );
    }

    [Fact]
    public async Task OnlyManagedRefListExtensions()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized]
            public struct MyStruct 
            {
                public int Primitive;
                public ManagedRefList<int> Collection;
            }
            """,
            "MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            public static class MyStructExplicitCopy
            {
                public static void CopyFrom(this ref MyStruct self, in MyStruct other)
                {
                    self.Primitive = other.Primitive;
                    self.Collection.CopyFrom(other.Collection);
                }
                
                public static void CopyTo(this in MyStruct self, ref MyStruct other)
                {
                    other.CopyFrom(self);
                }

            """ +
            RefListExtensions.IndentFormat(1, "ManagedRefList", "MyStruct") +
            """
            }
            
            """
        );
    }

    [Fact]
    public async Task OnlyTempRefListExtensions()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct MyStruct 
            {
                public int Primitive;
                public TempRefList<int> Collection;
            }
            """,
            "MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            public static class MyStructExplicitCopy
            {
                public static void CopyFrom(this ref MyStruct self, in MyStruct other)
                {
                    self.Primitive = other.Primitive;
                    self.Collection.CopyFrom(other.Collection);
                }
                
                public static void CopyTo(this in MyStruct self, ref MyStruct other)
                {
                    other.CopyFrom(self);
                }
            
            """ +
            RefListExtensions.IndentFormat(1, "TempRefList", "MyStruct") +
            """
            }
            
            """
        );
    }

    [Fact]
    public async Task NoRefListExtensions()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [ExplicitCopy, DynSized, TempAlloc]
            public struct MyStruct 
            {
                public string ManagedPrimitive;
                public TempRefList<int> Collection;
            }
            """,
            "MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            public static class MyStructExplicitCopy
            {
                public static void CopyFrom(this ref MyStruct self, in MyStruct other)
                {
                    self.ManagedPrimitive = other.ManagedPrimitive;
                    self.Collection.CopyFrom(other.Collection);
                }
                
                public static void CopyTo(this in MyStruct self, ref MyStruct other)
                {
                    other.CopyFrom(self);
                }
            }

            """
        );
    }

    [Fact]
    public async Task NamespaceIsUsedByExtensionsClass()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            namespace Test.Ns
            {
                [ExplicitCopy, DynSized, TempAlloc]
                public struct MyStruct 
                {
                    public string ManagedPrimitive;
                    public TempRefList<int> Collection;
                }
            }
            """,
            "MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            namespace Test.Ns
            {
                public static class MyStructExplicitCopy
                {
                    public static void CopyFrom(this ref MyStruct self, in MyStruct other)
                    {
                        self.ManagedPrimitive = other.ManagedPrimitive;
                        self.Collection.CopyFrom(other.Collection);
                    }
                    
                    public static void CopyTo(this in MyStruct self, ref MyStruct other)
                    {
                        other.CopyFrom(self);
                    }
                }
            }

            """
        );
    }

    [Fact]
    public async Task EnclosingTypeNameIsRespected()
    {
        await ExplicitCopyGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public static class EnclosingType
            {
                [ExplicitCopy, DynSized, TempAlloc]
                public struct MyStruct 
                {
                    public TempRefList<int> Collection;
                }
            }
            """,
            "EnclosingType.MyStructExplicitCopy.g.cs",
            // language=cs
            """
            using Ksi;

            public static class EnclosingType_MyStructExplicitCopy
            {
                public static void CopyFrom(this ref EnclosingType.MyStruct self, in EnclosingType.MyStruct other)
                {
                    self.Collection.CopyFrom(other.Collection);
                }
                
                public static void CopyTo(this in EnclosingType.MyStruct self, ref EnclosingType.MyStruct other)
                {
                    other.CopyFrom(self);
                }

            """ +
            RefListExtensions.IndentFormat(1, "TempRefList", "EnclosingType.MyStruct") +
            """
            }

            """
        );
    }
}