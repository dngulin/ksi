using System;

namespace DnDev
{
    [NoCopy]
    public struct RefList<T> where T : struct
    {
        internal T[] Buffer;
        internal int Count;

        internal RefList(T[] buffer, int count)
        {
            Buffer = buffer;
            Count = count;
        }
    }

    [NoCopy]
    public unsafe ref struct TempRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;

        internal TempRefList(T* buffer, int capacity, int count)
        {
            Buffer = buffer;
            Capacity = capacity;
            Count = count;
        }
    }

    [NoCopy]
    public unsafe ref struct NativeRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;

        internal NativeRefList(T* buffer, int capacity, int count)
        {
            Buffer = buffer;
            Capacity = capacity;
            Count = count;
        }
    }

    public static class RefList
    {
        [NoCopyReturn]
        public static RefList<T> Empty<T>() where T : struct
        {
            return new RefList<T>(null, 0);
        }

        [NoCopyReturn]
        public static RefList<T> WithCapacity<T>(int capacity) where T : struct
        {
            return new RefList<T>(new T[capacity], 0);
        }

        [NoCopyReturn]
        public static RefList<T> WithDefaultItems<T>(int count) where T : struct
        {
            return new RefList<T>(new T[count], count);
        }

        [NoCopyReturn]
        public static RefList<T> Move<T>(ref RefList<T> other) where T : struct
        {
            var list = new RefList<T>(other.Buffer, other.Count);
            other = default;
            return list;
        }

        [NoCopyReturn]
        public static RefList<T> Copy<T>(in RefList<T> other) where T : struct
        {
            if (other.Capacity() == 0)
                return default;

            var items = new T[other.Capacity()];
            Array.Copy(other.Buffer, items, other.Count);
            return new RefList<T>(other.Buffer, other.Count);
        }
    }

    public static class RefListImpl
    {
        public static int Capacity<T>(this in RefList<T> list) where T : struct => list.GetBufferSize();

        public static int Count<T>(this in RefList<T> list) where T : struct => list.Count;


        public static ref readonly T RefReadonlyAt<T>(this in RefList<T> list, int index) where T : struct => ref list.Buffer[index];

        public static ref T RefAt<T>(this ref RefList<T> list, int index) where T : struct => ref list.Buffer[index];


        public static void Add<T>(this ref RefList<T> list, in T item) where T : struct
        {
            list.EnsureCanAdd();
            list.IndexBufferMut(list.Count++) = item;
        }

        public static ref T RefAdd<T>(this ref RefList<T> list) where T : struct
        {
            list.EnsureCanAdd();
            return ref list.IndexBufferMut(list.Count++);
        }

        private static void EnsureCanAdd<T>(this ref RefList<T> list) where T : struct
        {
            if (list.Count < list.Capacity())
                return;

            var newSize = Math.Max(list.Capacity() * 2, 1);
            list.SetBufferSize(newSize);
        }

        public static void RemoveAt<T>(this ref RefList<T> list, int index) where T : struct
        {
            if (index < 0 || index >= list.Count)
            {
                throw new IndexOutOfRangeException();
            }

            list.Count--;
            list.CopyBufferRange(index + 1, index, list.Count - index);
            list.IndexBufferMut(list.Count) = default;
        }

        public static void Clear<T>(this ref RefList<T> list) where T : struct
        {
            if (list.Count == 0)
                return;

            list.ClearBuffer();
            list.Count = 0;
        }

        public static void AppendDefault<T>(this ref RefList<T> list, int count) where T : struct
        {
            var newCount = list.Count + count;

            if (list.Capacity() < newCount)
                list.SetBufferSize(newCount);
        }
    }
}