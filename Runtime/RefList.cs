// Disable diagnostics for redundant DynSized, Dealloc & Temp attributes
#pragma warning disable DYNSIZED03, DEALLOC03, TEMPALLOC03

namespace Ksi
{
    [ExplicitCopy, DynSized, Dealloc, RefList]
    public struct RefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    [ExplicitCopy, DynSized, TempAlloc, RefList]
    public struct TempRefList<T> where T : unmanaged
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