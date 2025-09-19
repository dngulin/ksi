using System;

#pragma warning disable REFLIST01

namespace Ksi
{
    public static class RefListSpanExtensions
    {
        [RefListIterator] public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self)
            where T : struct => new(self.Array, 0, self.Count);
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in TempRefList<T> self)
            where T : unmanaged => new(self.Array.Buffer, self.Count);
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in NativeRefList<T> self)
            where T : unmanaged => new(self.Array.Buffer, self.Count);

        [RefListIterator] public static Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self)
            where T : struct => new(self.Array, 0, self.Count);
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self)
            where T : unmanaged => new(self.Array.Buffer, self.Count);
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref NativeRefList<T> self)
            where T : unmanaged => new(self.Array.Buffer, self.Count);
    }
}