using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiHashTableSlot]
    internal struct IntToIntMapSlot
    {
        internal KsiHashTableSlotState State;
        internal int Key;
        internal int Value;
    }

    [KsiHashTable]
    [ExplicitCopy, DynSized, Dealloc]
    internal partial struct IntToIntMap
    {
        internal RefList<IntToIntMapSlot> HashTable;
        internal int Count;

        internal static int Hash(int key) => key;
        internal static bool Eq(int l, int r) => l == r;
    }

    [TestFixture]
    public class KsiHashMapTests
    {
        private static readonly System.Random Rnd = new System.Random();

        [Test]
        public void Set()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;

            Assert.That(map.RefSet(k), Is.EqualTo(0));

            map.RefSet(k) = v;
            Assert.That(map.RefSet(k), Is.EqualTo(v));

            map.RefSet(k) = v + 1;
            Assert.That(map.RefSet(k), Is.EqualTo(v + 1));

            map.Dealloc();
        }

        [Test]
        public void Get()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;

            map.RefSet(k) = v;
            Assert.That(map.RefGet(k), Is.EqualTo(v));
            Assert.That(map.RefReadonlyGet(k), Is.EqualTo(v));

            map.Dealloc();
        }

        [Test]
        public void Contains()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;

            Assert.That(map.Contains(k, out _), Is.False);

            map.RefSet(k) = v;
            Assert.That(map.Contains(k, out _), Is.True);

            map.Dealloc();
        }

        [Test]
        public void GetByIndex()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;

            map.RefSet(k) = v;
            Assert.That(map.Contains(k, out var idx), Is.True);
            Assert.That(map.RefGetByIndex(idx), Is.EqualTo(v));
            Assert.That(map.RefReadonlyGetByIndex(idx), Is.EqualTo(v));

            map.Dealloc();
        }

        [Test]
        public void Remove()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;

            var removed = map.Remove(k);
            Assert.That(removed, Is.False);

            map.RefSet(k) = v;
            removed = map.Remove(k);
            Assert.That(removed, Is.True);

            map.Dealloc();
        }

        [Test]
        public void Count()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());
            var map = IntToIntMap.Empty;
            Assert.That(map.Count(), Is.EqualTo(0));

            map.RefSet(k) = v;
            map.RefSet(k) = v + 1;
            Assert.That(map.Count(), Is.EqualTo(1));

            map.Remove(k);
            Assert.That(map.Count(), Is.EqualTo(0));

            map.Dealloc();
        }

        [Test]
        public void Clear()
        {
            var (k, v) = (Rnd.Next(), Rnd.Next());

            var map = IntToIntMap.Empty;
            Assert.That(map.Count(), Is.EqualTo(0));

            map.RefSet(k) = v;
            map.RefSet(k + 1) = v;
            Assert.That(map.Count(), Is.EqualTo(2));

            map.Clear();
            Assert.That(map.Count(), Is.EqualTo(0));

            map.Dealloc();
        }
    }
}