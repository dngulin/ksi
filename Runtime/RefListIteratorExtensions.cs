namespace Frog.Collections
{
    public static class RefListIteratorExtensions
    {
        public static RefListReadonlyIterator<T> RefReadonlyIter<T>(this in RefList<T> self) where T : struct
        {
            return new RefListReadonlyIterator<T>(self.ItemArray, self.ItemCount);
        }

        public static RefListIterator<T> RefIter<T>(this in RefList<T> self) where T : struct
        {
            return new RefListIterator<T>(self.ItemArray, self.ItemCount);
        }
    }

    public ref struct RefListReadonlyIterator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;

        public RefListReadonlyIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public RefListReadonlyEnumerator<T> GetEnumerator() => new RefListReadonlyEnumerator<T>(_items, _count);
    }

    public ref struct RefListIterator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;

        public RefListIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public RefListEnumerator<T> GetEnumerator() => new RefListEnumerator<T>(_items, _count);
    }

    public ref struct RefListReadonlyEnumerator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _curr;

        public RefListReadonlyEnumerator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _curr = -1;
        }

        public ref readonly T Current => ref _items[_curr];
        public bool MoveNext() => ++_curr < _count;
        public void Reset() => _curr = -1;
        public void Dispose() {}
    }

    public ref struct RefListEnumerator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _curr;

        public RefListEnumerator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _curr = -1;
        }

        public ref T Current => ref _items[_curr];
        public bool MoveNext() => ++_curr < _count;
        public void Reset() => _curr = -1;
        public void Dispose() {}
    }
}