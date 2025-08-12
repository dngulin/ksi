namespace DnDev
{
    [NoCopy, RefListApi]
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

    [NoCopy, RefListApi, UnmanagedRefList]
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

    [NoCopy, RefListApi, UnmanagedRefList]
    public unsafe struct NativeRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;

        internal NativeRefList(in NativeRefList<T> other)
        {
            Buffer = other.Buffer;
            Capacity = other.Capacity;
            Count = other.Count;
        }
    }
}