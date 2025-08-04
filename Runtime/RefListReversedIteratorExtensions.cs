namespace DnDev
{
    public static class RefListReversedIteratorExtensions
    {
        public static RefListReversedReadonlyIterator<T> RefReadonlyIterReversed<T>(this in RefList<T> self) where T : struct
        {
            return new RefListReversedReadonlyIterator<T>(self.Buffer, self.Count);
        }

        public static RefListReversedIterator<T> RefIterReversed<T>(this in RefList<T> self) where T : struct
        {
            return new RefListReversedIterator<T>(self.Buffer, self.Count);
        }
    }

    public ref struct RefListReversedReadonlyIterator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;

        public RefListReversedReadonlyIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public RefListReversedReadonlyEnumerator<T> GetEnumerator() => new RefListReversedReadonlyEnumerator<T>(_items, _count);
    }

    public ref struct RefListReversedIterator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;

        public RefListReversedIterator(T[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public RefListReversedEnumerator<T> GetEnumerator() => new RefListReversedEnumerator<T>(_items, _count);
    }

    public ref struct RefListReversedReadonlyEnumerator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _curr;

        public RefListReversedReadonlyEnumerator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _curr = count;
        }

        public ref readonly T Current => ref _items[_curr];
        public bool MoveNext() => --_curr >= 0;
        public void Reset() => _curr = _count;
        public void Dispose() {}
    }

    public ref struct RefListReversedEnumerator<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int _count;
        private int _curr;

        public RefListReversedEnumerator(T[] items, int count)
        {
            _items = items;
            _count = count;
            _curr = count;
        }

        public ref readonly T Current => ref _items[_curr];
        public bool MoveNext() => --_curr >= 0;
        public void Reset() => _curr = _count;
        public void Dispose() {}
    }
}