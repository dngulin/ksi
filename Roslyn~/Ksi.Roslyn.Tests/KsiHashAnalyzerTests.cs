namespace Ksi.Roslyn.Tests;

using KsiHashAnalyzerTest = Util.KsiAnalyzerTest<KsiHashAnalyzer>;

public class KsiHashAnalyzerTests
{
    [Fact]
    public async Task Rule01MissingSymbol()
    {
        await KsiHashAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            [KsiHashTableSlot] public struct {|KSIHASH01:MissingState|} { public int Key; }
            [KsiHashTableSlot] public struct {|KSIHASH01:MissingKey|} { internal KsiHashTableSlotState State; }
            
            [KsiHashTableSlot]
            public struct ValidHashSetSlot
            {
                internal KsiHashTableSlotState State;
                public int Key;
            }
            
            [KsiHashTableSlot]
            public struct ValidHashMapSlot
            {
                internal KsiHashTableSlotState State;
                public int Key;
            }
            
            [KsiHashTable]
            public partial struct {|KSIHASH01:MissingHashTable|}
            {
                internal int Count;
                
                // Method signatures are not valid because the TKey is not known 
                internal static int {|KSIHASH03:Hash|}(in int key) => throw null;
                internal static bool {|KSIHASH03:Eq|}(in int a, in int b) => throw null;
            }
            
            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct {|KSIHASH01:MissingCount|}
            {
                internal RefList<ValidHashSetSlot> HashTable;
                internal static int Hash(in int key) => throw null;
                internal static bool Eq(in int a, in int b) => throw null;
            }
            
            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct {|KSIHASH01:MissingHash|}
            {
                internal RefList<ValidHashSetSlot> HashTable;
                internal int Count;
                internal static bool Eq(in int a, in int b) => throw null;
            }
            
            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct {|KSIHASH01:MissingEq|}
            {
                internal RefList<ValidHashSetSlot> HashTable;
                internal int Count;
                internal static int Hash(in int key) => throw null;
            }
            
            [KsiHashTable]
            [ExplicitCopy, DynSized, Dealloc]
            public partial struct ValidHashSet
            {
                internal RefList<ValidHashSetSlot> HashTable;
                internal int Count;
                internal static int Hash(in int key) => throw null;
                internal static bool Eq(in int a, in int b) => throw null;
            }
            """
        );
    }

    [Fact]
    public async Task Rule02InvalidField()
    {
        await KsiHashAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiHashTableSlot]
            public struct ExtraSlotField
            {
                internal KsiHashTableSlotState State;
                public int Key;
                public int Value;
                public float {|KSIHASH02:FooBar|};
            }
            """
        );
    }

    [Fact]
    public async Task Rule03InvalidFieldSignature()
    {
        await KsiHashAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiHashTableSlot]
            public struct InvalidSlotFieldType
            {
                internal int {|KSIHASH03:State|};
                public string {|KSIHASH03:Key|};
                public object {|KSIHASH03:Value|};
            }
            
            [KsiHashTableSlot]
            public struct StaticSlotField
            {
                internal static KsiHashTableSlotState {|KSIHASH03:State|};
                public static int {|KSIHASH03:Key|};
                public static int {|KSIHASH03:Value|};
            }
            """
        );
    }

    [Fact]
    public async Task Rule04InvalidInvalidAccessibility()
    {
        await KsiHashAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;
            
            internal class PrivateSlotHolder
            {
                [KsiHashTableSlot]
                private struct {|KSIHASH04:PrivateSlot|}
                {
                    internal KsiHashTableSlotState State;
                    public int Key;
                    public int Value;
                }
            }
            
            [KsiHashTableSlot]
            internal struct PrivateSlotField
            {
                private KsiHashTableSlotState {|KSIHASH04:State|};
                private int {|KSIHASH04:Key|};
                private int {|KSIHASH04:Value|};
            }
            """
        );
    }
}