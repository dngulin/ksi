using System.Text;
using Ksi.Roslyn.Extensions;
using Ksi.Roslyn.Tests.Util;

namespace Ksi.Roslyn.Tests;

using KsiHashGeneratorTest = KsiGeneratorTest<KsiHashGenerator>;

public class KsiHashGeneratorTests
{
    [Fact]
    public async Task HashSetSmoke()
    {
        await KsiHashGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiHashTableSlot]
            internal struct HashSetSlot
            {
                internal KsiHashTableSlotState State;
                internal int Key;
            }

            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct HashSet
            {
                internal RefList<HashSetSlot> HashTable;
                internal int Count;
                internal static int Hash(int key) => throw null;
                internal static bool Eq(int l, int r) => throw null;
            }

            // DeallocGenerator is not loaded by this test,
            // So, the API produced by it is mocked
            public static class DeallocStub
            {
                public static ref HashSet Deallocated(ref this HashSet self) => throw null;
            }
            """,
            "HashSet.KsiHashTable.g.cs",
            new StringBuilder(8 * 1024)
                .AppendLine(
                    """
                    using Ksi;
                    using System;

                    """)
                .AppendLine(KsiHashTemplates.HashSetApi)
                .Replace("|accessibility|", "public")
                .Replace("|THashSet|", "HashSet")
                .Replace("|TKey|", "int")
                .Unwrap("[in ]", false)
                .Unwrap("[in `insertion]", false)
                .Unwrap("[.Move()]", false)
                .Unwrap("[key.Dealloc();\n                    ]", false)
                .Unwrap("[.Deallocated()`self]", true)
                .Unwrap("[.Deallocated()`slot]", false)
                .ToString()
        );
    }

    [Fact]
    public async Task HashMapSmoke()
    {
        await KsiHashGeneratorTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiHashTableSlot]
            internal struct HashMapSlot
            {
                internal KsiHashTableSlotState State;
                internal int Key;
                internal int Value;
            }

            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct HashMap
            {
                internal RefList<HashMapSlot> HashTable;
                internal int Count;
                internal static int Hash(int key) => throw null;
                internal static bool Eq(int l, int r) => throw null;
            }

            // DeallocGenerator is not loaded by this test,
            // So, the API produced by it is mocked
            public static class DeallocStub
            {
                public static ref HashMap Deallocated(ref this HashMap self) => throw null;
            }
            """,
            "HashMap.KsiHashTable.g.cs",
            new StringBuilder(8 * 1024)
                .AppendLine(
                    """
                    using Ksi;
                    using System;

                    """)
                .AppendLine(KsiHashTemplates.HashMapApi)
                .Replace("|accessibility|", "public")
                .Replace("|THashMap|", "HashMap")
                .Replace("|TKey|", "int")
                .Replace("|TValue|", "int")
                .Replace(
                    "|RefPathSuffix|",
                    """
                    "!", "[n]", "Value"
                    """
                )
                .Unwrap("[in ]", false)
                .Unwrap("[in `insertion]", false)
                .Unwrap("[.Move()`key]", false)
                .Unwrap("[.Move()`value]", false)
                .Unwrap("[key.Dealloc();\n                    ]", false)
                .Unwrap("[.Deallocated()`self]", true)
                .Unwrap("[.Deallocated()`slot]", false)
                .ToString()
        );
    }
}