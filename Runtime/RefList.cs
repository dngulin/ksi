#pragma warning disable DYNSIZED02

namespace Ksi
{
    [NoCopy, DynSized, RefList]
    public struct RefList<T> where T : struct
    {
        internal T[] Array;
        internal int Count;
    }

    [NoCopy, DynSized, RefList]
    public ref struct TempRefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    [NoCopy, DynSized, RefList]
    public struct NativeRefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }
}