using System;
using System.IO;
using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiSerializable]
    public struct TestSerializableStruct
    {
        [KsiSerializeField(0)] public sbyte SByteField;
        [KsiSerializeField(1)] public byte ByteField;
        [KsiSerializeField(2)] public short ShortField;
        [KsiSerializeField(3)] public ushort UShortField;
        [KsiSerializeField(4)] public int IntField;
        [KsiSerializeField(5)] public uint UIntField;
        [KsiSerializeField(6)] public long LongField;
        [KsiSerializeField(7)] public ulong ULongField;
        [KsiSerializeField(8)] public float FloatField;
        [KsiSerializeField(9)] public double DoubleField;
        [KsiSerializeField(10)] public bool BoolField;
        [KsiSerializeField(11)] public char CharField;
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

            return new TestSerializableStruct
            {
                SByteField = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1),
                ByteField = (byte)random.Next(byte.MinValue, byte.MaxValue + 1),
                ShortField = (short)random.Next(short.MinValue, short.MaxValue + 1),
                UShortField = (ushort)random.Next(ushort.MinValue, ushort.MaxValue + 1),
                IntField = random.Next(),
                UIntField = (uint)random.Next(),
                LongField = longValue,
                ULongField = ulongValue,
                FloatField = (float)random.NextDouble(),
                DoubleField = random.NextDouble(),
                BoolField = random.Next(2) == 0,
                CharField = (char)random.Next(char.MinValue, char.MaxValue + 1)
            };
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

            Assert.That(deserialized.SByteField, Is.EqualTo(original.SByteField));
            Assert.That(deserialized.ByteField, Is.EqualTo(original.ByteField));
            Assert.That(deserialized.ShortField, Is.EqualTo(original.ShortField));
            Assert.That(deserialized.UShortField, Is.EqualTo(original.UShortField));
            Assert.That(deserialized.IntField, Is.EqualTo(original.IntField));
            Assert.That(deserialized.UIntField, Is.EqualTo(original.UIntField));
            Assert.That(deserialized.LongField, Is.EqualTo(original.LongField));
            Assert.That(deserialized.ULongField, Is.EqualTo(original.ULongField));
            Assert.That(deserialized.FloatField, Is.EqualTo(original.FloatField));
            Assert.That(deserialized.DoubleField, Is.EqualTo(original.DoubleField));
            Assert.That(deserialized.BoolField, Is.EqualTo(original.BoolField));
            Assert.That(deserialized.CharField, Is.EqualTo(original.CharField));
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
            span.Prepend(original);

            // Deserialize
            var deserialized = new TestSerializableStruct();
            ReadOnlySpan<byte> readSpan = buffer;
            deserialized.InitializeFrom(readSpan);

            Assert.That(deserialized.SByteField, Is.EqualTo(original.SByteField));
            Assert.That(deserialized.ByteField, Is.EqualTo(original.ByteField));
            Assert.That(deserialized.ShortField, Is.EqualTo(original.ShortField));
            Assert.That(deserialized.UShortField, Is.EqualTo(original.UShortField));
            Assert.That(deserialized.IntField, Is.EqualTo(original.IntField));
            Assert.That(deserialized.UIntField, Is.EqualTo(original.UIntField));
            Assert.That(deserialized.LongField, Is.EqualTo(original.LongField));
            Assert.That(deserialized.ULongField, Is.EqualTo(original.ULongField));
            Assert.That(deserialized.FloatField, Is.EqualTo(original.FloatField));
            Assert.That(deserialized.DoubleField, Is.EqualTo(original.DoubleField));
            Assert.That(deserialized.BoolField, Is.EqualTo(original.BoolField));
            Assert.That(deserialized.CharField, Is.EqualTo(original.CharField));
        }
    }
}
