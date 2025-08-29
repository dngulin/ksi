namespace DnDev
{
    [NoCopy, RefListApi]
    public struct RefList<T> where T : struct
    {
        internal T[] Buffer;
        internal int Count;
    }

    [NoCopy, RefListApi, UnmanagedRefList]
    public unsafe ref struct TempRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;
    }

    [NoCopy, RefListApi, UnmanagedRefList]
    public unsafe struct NativeRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;
    }
}