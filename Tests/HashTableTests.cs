using NUnit.Framework;

namespace Ksi.Tests
{
    internal struct Foo
    {
        public int Value;
    }

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

        public static int Hash(in int key) => key;
        public static bool Eq(in int l, in int r) => l == r;
    }

    [TestFixture]
    public class HashTableTests
    {
        [Test]
        public void Smoke()
        {
            var set = IntSet.Empty;
            set.Add(1);
            Assert.That(set.Contains(1));
            Assert.That(!set.Contains(2));

            set.Add(2);
            Assert.That(set.Contains(2));

            set.Remove(1);
            Assert.That(!set.Contains(1));

            Assert.That(set.Count() == 1);
        }
    }
}