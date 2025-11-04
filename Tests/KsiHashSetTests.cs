using NUnit.Framework;

namespace Ksi.Tests
{
    [KsiHashTableSlot]
    internal struct IntSetSlot
    {
        internal KsiHashTableSlotState State;
        internal int Key;
    }

    [KsiHashTable]
    [ExplicitCopy, DynSized, Dealloc]
    internal partial struct IntSet
    {
        internal RefList<IntSetSlot> HashTable;
        internal int Count;

        internal static int Hash(int key) => key;
        internal static bool Eq(int l, int r) => l == r;
    }

    [TestFixture]
    public class KsiHashSetTests
    {
        private static readonly System.Random Rnd = new System.Random();

        [Test]
        public void Add()
        {
            var set = IntSet.Empty;
            var n = Rnd.Next();

            var added = set.Add(n);
            Assert.That(added, Is.True);

            added = set.Add(n);
            Assert.That(added, Is.False);

            set.Dealloc();
        }

        [Test]
        public void Remove()
        {
            var set = IntSet.Empty;
            var n = Rnd.Next();

            var removed = set.Remove(n);
            Assert.That(removed, Is.False);

            set.Add(n);
            removed = set.Remove(n);
            Assert.That(removed, Is.True);

            set.Dealloc();
        }

        [Test]
        public void Contains()
        {
            var set = IntSet.Empty;
            var n = Rnd.Next();

            Assert.That(set.Contains(n), Is.False);
            set.Add(n);
            Assert.That(set.Contains(n), Is.True);
            set.Remove(n);
            Assert.That(set.Contains(n), Is.False);

            set.Dealloc();
        }

        [Test]
        public void Count()
        {
            var set = IntSet.Empty;
            Assert.That(set.Count(), Is.EqualTo(0));

            var n = Rnd.Next();

            set.Add(n);
            Assert.That(set.Count(), Is.EqualTo(1));
            set.Add(n);
            Assert.That(set.Count(), Is.EqualTo(1));

            set.Add(n + 1);
            Assert.That(set.Count(), Is.EqualTo(2));

            set.Remove(n);
            Assert.That(set.Count(), Is.EqualTo(1));
            set.Remove(n);
            Assert.That(set.Count(), Is.EqualTo(1));

            set.Remove(n + 1);
            Assert.That(set.Count(), Is.EqualTo(0));

            set.Dealloc();
        }

        [Test]
        public void Clear()
        {
            var set = IntSet.Empty;
            var n = Rnd.Next();

            set.Add(n);
            set.Add(n + 1);

            Assert.That(set.Contains(n), Is.True);
            Assert.That(set.Contains(n + 1), Is.True);
            Assert.That(set.Count(), Is.EqualTo(2));

            set.Clear();

            Assert.That(set.Contains(n), Is.False);
            Assert.That(set.Contains(n + 1), Is.False);
            Assert.That(set.Count(), Is.EqualTo(0));

            set.Dealloc();
        }
    }
}