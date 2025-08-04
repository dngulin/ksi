using System;

namespace Frog.Collections
{
    [NoCopy]
    public struct RefList<T> where T : struct
    {
        internal T[] ItemArray;
        internal int ItemCount;

        internal RefList(T[] array, int itemCount)
        {
            ItemArray = array;
            ItemCount = itemCount;
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
            var list = new RefList<T>(other.ItemArray, other.ItemCount);
            other = default;
            return list;
        }

        [NoCopyReturn]
        public static RefList<T> Copy<T>(in RefList<T> other) where T : struct
        {
            if (other.Capacity() == 0)
                return default;

            var items = new T[other.Capacity()];
            Array.Copy(other.ItemArray, items, other.ItemCount);
            return new RefList<T>(other.ItemArray, other.ItemCount);
        }
    }

    public static class RefListImpl
    {
        public static int Capacity<T>(this in RefList<T> list) where T : struct => list.ItemArray?.Length ?? 0;

        public static int Count<T>(this in RefList<T> list) where T : struct => list.ItemCount;


        public static ref readonly T RefReadonlyAt<T>(this in RefList<T> list, int index) where T : struct => ref list.ItemArray[index];

        public static ref T RefAt<T>(this ref RefList<T> list, int index) where T : struct => ref list.ItemArray[index];


        public static void Add<T>(this ref RefList<T> list, in T item) where T : struct
        {
            list.EnsureCanAdd();
            list.ItemArray[list.ItemCount++] = item;
        }

        public static ref T RefAdd<T>(this ref RefList<T> list) where T : struct
        {
            list.EnsureCanAdd();
            return ref list.ItemArray[list.ItemCount++];
        }

        private static void EnsureCanAdd<T>(this ref RefList<T> list) where T : struct
        {
            if (list.ItemCount < list.Capacity())
                return;

            var newSize = Math.Max(list.Capacity() * 2, 1);
            Array.Resize(ref list.ItemArray, newSize);
        }

        public static void RemoveAt<T>(this ref RefList<T> list, int index) where T : struct
        {
            if (index < 0 || index >= list.ItemCount)
            {
                throw new IndexOutOfRangeException();
            }

            list.ItemCount--;
            Array.Copy(list.ItemArray, index + 1, list.ItemArray, index, list.ItemCount - index);
            list.ItemArray[list.ItemCount] = default;
        }

        public static void Clear<T>(this ref RefList<T> list) where T : struct
        {
            if (list.ItemCount == 0)
                return;

            Array.Clear(list.ItemArray, 0, list.ItemCount);
            list.ItemCount = 0;
        }

        public static void AppendDefault<T>(this ref RefList<T> list, int count) where T : struct
        {
            var newCount = list.ItemCount + count;

            if (list.Capacity() < newCount)
                Array.Resize(ref list.ItemArray, newCount);

            list.ItemCount = newCount;
        }

        public static void Sort<T>(this ref RefList<T> list) where T : struct
        {
            Array.Sort(list.ItemArray, 0, list.ItemCount);
        }
    }
}