#pragma warning disable DYNSIZED02

namespace Ksi
{
    [NoCopy, DynSized, RefList]
    public struct RefList<T> where T : struct
    {
        internal T[] Buffer;
        internal int Count;
    }

    [NoCopy, DynSized, RefList]
    public unsafe ref struct TempRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;
    }

    [NoCopy, DynSized, RefList]
    public unsafe struct NativeRefList<T> where T : unmanaged
    {
        internal T* Buffer;
        internal int Capacity;
        internal int Count;
    }
}