using System;

#pragma warning disable REFLIST01

namespace Ksi
{
    public static class RefListSpanExtensions
    {
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in TempRefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);
        [RefListIterator] public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in ManagedRefList<T> self)
            where T : struct => new ReadOnlySpan<T>(self.Array, 0, self.Count);

        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);
        [RefListIterator] public static Span<T> AsSpan<T>([DynNoResize] this ref ManagedRefList<T> self)
            where T : struct => new Span<T>(self.Array, 0, self.Count);
    }
}