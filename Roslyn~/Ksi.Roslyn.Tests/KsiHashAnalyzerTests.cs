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