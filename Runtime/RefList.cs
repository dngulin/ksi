using Ksi.Internal;

#pragma warning disable DYNSIZED03, DEALLOC03, TEMPALLOC03

namespace Ksi
{
    /// <summary>
    /// A dynamic array collection wrapping the <c>Persistent</c> allocator.
    /// Requires manual deallocation.
    /// </summary>
    [ExplicitCopy, DynSized, Dealloc, RefList]
    public struct RefList<[ExplicitCopy, DynSized, Dealloc] T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    /// <summary>
    /// A dynamic array collection wrapping the <c>Temp</c> allocator.
    /// Can be stored only on stack.
    /// </summary>
    [ExplicitCopy, DynSized, TempAlloc, RefList]
    public struct TempRefList<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T> where T : unmanaged
    {
        internal UnsafeArray<T> Array;
        internal int Count;
    }

    /// <summary>
    /// A dynamic array collection wrapping a managed array.
    /// Can store structures containing reference types, but it is not compatible with <c>Burst</c>.
    /// </summary>
    [ExplicitCopy, DynSized, RefList]
    public struct ManagedRefList<[ExplicitCopy, DynSized, Dealloc] T> where T : struct
    {
#pragma warning disable KSIGENERIC02
        internal T[] Array;
#pragma warning restore KSIGENERIC02
        internal int Count;
    }
}