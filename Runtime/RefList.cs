using System;

namespace DnDev
{
    [NoCopy, ManagedRefListApi]
    public struct RefList<T> where T : struct
    {
        internal T[] Buffer;
        internal int Count;

        internal RefList(in RefList<T> other)
        {
            Buffer = other.Buffer;
            Count = other.Count;
        }
    }

    [NoCopy, UnmanagedRefListApi]
    public unsafe ref struct TempRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;

        internal TempRefList(in TempRefList<T> other)
        {
            Buffer = other.Buffer;
            Capacity = other.Capacity;
            Count = other.Count;
        }
    }

    [NoCopy, UnmanagedRefListApi]
    public unsafe ref struct NativeRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;

        internal NativeRefList(in TempRefList<T> other)
        {
            Buffer = other.Buffer;
            Capacity = other.Capacity;
            Count = other.Count;
        }
    }

    public static class RefList
    {
        [NoCopyReturn]
        public static RefList<T> Empty<T>() where T : struct => default;

        [NoCopyReturn]
        public static RefList<T> WithCapacity<T>(int capacity) where T : struct
        {
            var list = Empty<T>();
            list.SetBufferSize(capacity);
            return list;
        }

        [NoCopyReturn]
        public static RefList<T> WithDefaultItems<T>(int count) where T : struct
        {
            var list = WithCapacity<T>(count);
            list.Count = count;
            return list;
        }

        [NoCopyReturn]
        public static RefList<T> Move<T>(ref RefList<T> other) where T : struct
        {
            var list = new RefList<T>(other);
            other = default;
            return list;
        }

        [NoCopyReturn]
        public static RefList<T> Copy<T>(in RefList<T> other) where T : struct
        {
            if (other.Count == 0)
                return default;

            var list = WithCapacity<T>(other.Count);
            list.CopyBufferItemsFrom(other);

            return list;
        }
    }

    public static class RefListImpl
    {
        public static int Capacity<T>(this in RefList<T> list) where T : struct => list.GetBufferSize();

        public static int Count<T>(this in RefList<T> list) where T : struct => list.Count;

        public static void Dealloc<T>(this ref RefList<T> list) where T : struct => list.SetBufferSize(0);


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
            list.CopyWithinBuffer(index + 1, index, list.Count - index);
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