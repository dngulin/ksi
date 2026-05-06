namespace Ksi.Roslyn.Tests;

using SerdeAnalyzerTest = Util.KsiAnalyzerTest<SerdeAnalyzer>;

public class SerdeAnalyzerTests
{
    [Fact]
    public async Task Serde01MissingSerializable()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public struct {|SERDE01:TestStruct|}
            {
                [KsiSerializeField(1)]
                public int Value;
            }
            """
        );
    }

    [Fact]
    public async Task Serde02DuplicateFieldId()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiSerializable]
            public struct TestStruct
            {
                [KsiSerializeField(1)]
                public int A;

                [KsiSerializeField(1)]
                public int {|SERDE02:B|};
            }
            """
        );
    }

    [Fact]
    public async Task Serde03InvalidFieldType()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiSerializable]
            public struct TestStruct
            {
                [KsiSerializeField(1)]
                public string {|SERDE03:A|};

                [KsiSerializeField(2)]
                public object {|SERDE03:B|};
            }
            """
        );
    }

    [Fact]
    public async Task Serde04LowTypeAccessibility()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            public class Outer
            {
                {|SERDE04:[KsiSerializable]
                private struct TestStruct
                {
                    [KsiSerializeField(1)]
                    public int A;
                }|}
            }
            """
        );
    }

    [Fact]
    public async Task Serde05LowFieldAccessibility()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiSerializable]
            public struct TestStruct
            {
                [KsiSerializeField(1)]
                private int {|SERDE05:A|};
            }
            """
        );
    }

    [Fact]
    public async Task Serde06StaticField()
    {
        await SerdeAnalyzerTest.RunAsync(
            // language=cs
            """
            using Ksi;

            [KsiSerializable]
            public struct TestStruct
            {
                [KsiSerializeField(1)]
                public static int {|SERDE06:A|};
            }
            """
        );
    }
}
