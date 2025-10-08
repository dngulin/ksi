// Disable diagnostics for redundant DynSized, Dealloc & Temp attributes
#pragma warning disable DYNSIZED03, DEALLOC03, TEMPALLOC03

namespace Ksi
{
    /// <summary>
    /// Dynamic array collection wrapping the Persistent allocator.
    /// Requires manual deallocation.
    /// </summary>
    [ExplicitCopy, DynSized, Dealloc, RefList]
    public struct RefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    /// <summary>
    /// Dynamic array collection wrapping the Temp allocator.
    /// Can be stored only on stack.
    /// </summary>
    [ExplicitCopy, DynSized, TempAlloc, RefList]
    public struct TempRefList<T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    /// <summary>
    /// Dynamic array collection wrapping the Managed allocator.
    /// Can store structures containing reference types, but it is not compatible with Burst.
    /// </summary>
    [ExplicitCopy, DynSized, RefList]
    public struct ManagedRefList<T> where T : struct
    {
        internal T[] Array;
        internal int Count;
    }
}