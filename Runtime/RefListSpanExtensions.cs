using System;

#pragma warning disable REFLIST01

namespace Ksi
{
    public static class RefListSpanExtensions
    {
        [RefListIterator]
        public static ReadOnlySpan<T> AsReadonlySpan<T>(this in RefList<T> self) where T : struct
        {
            return new ReadOnlySpan<T>(self.Buffer, 0, self.Count);
        }

        [RefListIterator]
        public static unsafe ReadOnlySpan<T> AsReadonlySpan<T>(this in TempRefList<T> self) where T : unmanaged
        {
            return new ReadOnlySpan<T>(self.Buffer, self.Count);
        }

        [RefListIterator]
        public static unsafe ReadOnlySpan<T> AsReadonlySpan<T>(this in NativeRefList<T> self) where T : unmanaged
        {
            return new ReadOnlySpan<T>(self.Buffer, self.Count);
        }

        [RefListIterator]
        public static Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self) where T : struct
        {
            return new Span<T>(self.Buffer, 0, self.Count);
        }

        [RefListIterator]
        public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self) where T : unmanaged
        {
            return new Span<T>(self.Buffer, self.Count);
        }

        [RefListIterator]
        public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref NativeRefList<T> self) where T : unmanaged
        {
            return new Span<T>(self.Buffer, self.Count);
        }
    }
}