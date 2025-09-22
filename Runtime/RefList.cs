#pragma warning disable DEALLOC03, DYNSIZED02

namespace Ksi
{
    [ExplicitCopy, DynSized, Dealloc, RefList]
    public struct RefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    [ExplicitCopy, DynSized, RefList]
    public ref struct TempRefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    [ExplicitCopy, DynSized, RefList]
    public struct ManagedRefList<T> where T : struct
    {
        internal T[] Array;
        internal int Count;
    }
}