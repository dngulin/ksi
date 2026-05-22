using System;
using System.IO;
using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiSerializable]
    [ExplicitCopy, DynSized]
    public struct TestSerializableStruct
    {
        [KsiSerializeField(0)] public sbyte SByte;
        [KsiSerializeField(1)] public byte Byte;
        [KsiSerializeField(2)] public short Short;
        [KsiSerializeField(3)] public ushort UShort;
        [KsiSerializeField(4)] public int Int;
        [KsiSerializeField(5)] public uint UInt;
        [KsiSerializeField(6)] public long Long;
        [KsiSerializeField(7)] public ulong ULong;
        [KsiSerializeField(8)] public float Float;
        [KsiSerializeField(9)] public double Double;
        [KsiSerializeField(10)] public bool Bool;
        [KsiSerializeField(11)] public char Char;
        [KsiSerializeField(12)] public SampleEnum Enum;
        [KsiSerializeField(14)] public InnerStruct Struct;
        [KsiSerializeField(13)] public ManagedRefList<SampleEnum> RepEnum;
        [KsiSerializeField(15)] public ManagedRefList<InnerStruct> RepStruct;
    }

    public enum SampleEnum : ushort
    {
        A, B, C, D
    }

    [KsiSerializable]
    public struct InnerStruct
    {
        [KsiSerializeField(0)] public int Value;
    }

    [TestFixture]
    public class SerializationTests
    {
        private static TestSerializableStruct CreateRandomStruct(Random random)
        {
            Span<byte> span = stackalloc byte[sizeof(ulong)];

            random.NextBytes(span);
            var longValue = BitConverter.ToInt64(span);

            random.NextBytes(span);
            var ulongValue = BitConverter.ToUInt64(span);

            var result = new TestSerializableStruct
            {
                SByte = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1),
                Byte = (byte)random.Next(byte.MinValue, byte.MaxValue + 1),
                Short = (short)random.Next(short.MinValue, short.MaxValue + 1),
                UShort = (ushort)random.Next(ushort.MinValue, ushort.MaxValue + 1),
                Int = random.Next(),
                UInt = (uint)random.Next(),
                Long = longValue,
                ULong = ulongValue,
                Float = (float)random.NextDouble(),
                Double = random.NextDouble(),
                Bool = random.Next(2) == 0,
                Char = (char)random.Next(char.MinValue, char.MaxValue + 1),
                Enum = (SampleEnum)random.Next(0, 4),
                Struct = new InnerStruct
                {
                    Value = random.Next()
                },
            };

            var iterations = random.Next(0, 100);
            for (var i = 0; i < iterations; i++)
                result.RepEnum.Add((SampleEnum)random.Next(0, 4));

            iterations = random.Next(0, 100);
            for (var i = 0; i < iterations; i++)
                result.RepStruct.RefAdd().Value = random.Next();

            return result;
        }

        [Test]
        public void SerializeAndDeserializeFromStream()
        {
            var random = new Random(42);
            var original = CreateRandomStruct(random);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Serialize
            writer.Write(original);

            ms.Position = 0;
            using var reader = new BinaryReader(ms);

            // Deserialize
            var deserialized = new TestSerializableStruct();
            deserialized.InitializeFrom(reader);

            Assert.That(deserialized.SByte, Is.EqualTo(original.SByte));
            Assert.That(deserialized.Byte, Is.EqualTo(original.Byte));
            Assert.That(deserialized.Short, Is.EqualTo(original.Short));
            Assert.That(deserialized.UShort, Is.EqualTo(original.UShort));
            Assert.That(deserialized.Int, Is.EqualTo(original.Int));
            Assert.That(deserialized.UInt, Is.EqualTo(original.UInt));
            Assert.That(deserialized.Long, Is.EqualTo(original.Long));
            Assert.That(deserialized.ULong, Is.EqualTo(original.ULong));
            Assert.That(deserialized.Float, Is.EqualTo(original.Float));
            Assert.That(deserialized.Double, Is.EqualTo(original.Double));
            Assert.That(deserialized.Bool, Is.EqualTo(original.Bool));
            Assert.That(deserialized.Char, Is.EqualTo(original.Char));
            Assert.That(deserialized.Enum, Is.EqualTo(original.Enum));
            Assert.That(deserialized.Struct.Value, Is.EqualTo(original.Struct.Value));
            Assert.That(deserialized.RepEnum.Count(), Is.EqualTo(original.RepEnum.Count()));
            Assert.That(deserialized.RepStruct.Count(), Is.EqualTo(original.RepStruct.Count()));

            for (var i = 0; i < deserialized.RepEnum.Count(); i++)
                Assert.That(deserialized.RepEnum.RefReadonlyAt(i), Is.EqualTo(original.RepEnum.RefReadonlyAt(i)));

            for (var i = 0; i < deserialized.RepStruct.Count(); i++)
            {
                var oVal = deserialized.RepStruct.RefReadonlyAt(i).Value;
                var dVal = original.RepStruct.RefReadonlyAt(i).Value;
                Assert.That(oVal, Is.EqualTo(dVal));
            }
        }

        [Test]
        public void SerializeAndDeserializeFromBuffer()
        {
            var random = new Random(24);
            var original = CreateRandomStruct(random);

            var size = original.GetSerializedSize();
            var buffer = new byte[size];
            Span<byte> span = buffer;

            // Serialize
            span.Prepend(original, true);

            // Deserialize
            var deserialized = new TestSerializableStruct();
            ReadOnlySpan<byte> readSpan = buffer;
            deserialized.InitializeFrom(ref readSpan);

            Assert.That(deserialized.SByte, Is.EqualTo(original.SByte));
            Assert.That(deserialized.Byte, Is.EqualTo(original.Byte));
            Assert.That(deserialized.Short, Is.EqualTo(original.Short));
            Assert.That(deserialized.UShort, Is.EqualTo(original.UShort));
            Assert.That(deserialized.Int, Is.EqualTo(original.Int));
            Assert.That(deserialized.UInt, Is.EqualTo(original.UInt));
            Assert.That(deserialized.Long, Is.EqualTo(original.Long));
            Assert.That(deserialized.ULong, Is.EqualTo(original.ULong));
            Assert.That(deserialized.Float, Is.EqualTo(original.Float));
            Assert.That(deserialized.Double, Is.EqualTo(original.Double));
            Assert.That(deserialized.Bool, Is.EqualTo(original.Bool));
            Assert.That(deserialized.Char, Is.EqualTo(original.Char));
            Assert.That(deserialized.Enum, Is.EqualTo(original.Enum));
            Assert.That(deserialized.Struct.Value, Is.EqualTo(original.Struct.Value));
            Assert.That(deserialized.RepEnum.Count(), Is.EqualTo(original.RepEnum.Count()));
            Assert.That(deserialized.RepStruct.Count(), Is.EqualTo(original.RepStruct.Count()));

            for (var i = 0; i < deserialized.RepEnum.Count(); i++)
                Assert.That(deserialized.RepEnum.RefReadonlyAt(i), Is.EqualTo(original.RepEnum.RefReadonlyAt(i)));

            for (var i = 0; i < deserialized.RepStruct.Count(); i++)
            {
                var oVal = deserialized.RepStruct.RefReadonlyAt(i).Value;
                var dVal = original.RepStruct.RefReadonlyAt(i).Value;
                Assert.That(oVal, Is.EqualTo(dVal));
            }
        }
    }
}
