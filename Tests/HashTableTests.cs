using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiHashTableSlot]
    internal struct IntSetSlot
    {
        internal KsiHashTableSlotState State;
        public int Key;
    }

    [KsiHashTable]
    [ExplicitCopy, DynSized, Dealloc]
    internal partial struct IntSet
    {
        public RefList<IntSetSlot> HashTable;
        public int Count;

        public static int Hash(int key) => key;
        public static bool Eq(int l, int r) => l == r;
    }

    [KsiHashTableSlot]
    internal struct IntToIntMapSlot
    {
        internal KsiHashTableSlotState State;
        public int Key;
        public int Value;
    }

    [KsiHashTable]
    [ExplicitCopy, DynSized, Dealloc]
    internal partial struct IntToIntMap
    {
        public RefList<IntToIntMapSlot> HashTable;
        public int Count;

        public static int Hash(int key) => key;
        public static bool Eq(int l, int r) => l == r;
    }

    [TestFixture]
    public class HashTableTests
    {
        [Test]
        public void SmokeHashSet()
        {
            var set = IntSet.Empty;
            set.Add(1);
            Assert.That(set.Contains(1));
            Assert.That(!set.Contains(2));

            set.Add(2);
            Assert.That(set.Contains(2));

            set.Remove(1);
            Assert.That(!set.Contains(1));
            Assert.That(set.Count(), Is.EqualTo(1));

            set.Dealloc();
        }

        [Test]
        public void SmokeHashMap()
        {
            var map = IntToIntMap.Empty;
            map.RefSet(1) = 2;
            Assert.That(map.RefGet(1), Is.EqualTo(2));
            Assert.That(map.Count(), Is.EqualTo(1));

            map.RefSet(2) = 3;
            Assert.That(map.RefGet(2), Is.EqualTo(3));
            Assert.That(map.Count(), Is.EqualTo(2));

            map.Remove(1);
            Assert.That(!map.Contains(1, out _));
            Assert.That(map.Count(), Is.EqualTo(1));

            map.Dealloc();
        }
    }
}