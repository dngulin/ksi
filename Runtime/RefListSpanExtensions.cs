using System;

#pragma warning disable REFLIST01

namespace Ksi
{
    /// <summary>
    /// Extension methods to represent `[RefList]` collection as `Span` or `ReadOnlySpan`
    /// </summary>
    public static class RefListSpanExtensions
    {
        /// <summary>
        /// Represent the collection as `ReadOnlySpan`
        /// </summary>
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// Represent the collection as `ReadOnlySpan`
        /// </summary>
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in TempRefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// Represent the collection as `ReadOnlySpan`
        /// </summary>
        [RefListIterator] public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in ManagedRefList<T> self)
            where T : struct => new ReadOnlySpan<T>(self.Array, 0, self.Count);

        /// <summary>
        /// Represent the collection as `Span`
        /// </summary>
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// Represent the collection as `Span`
        /// </summary>
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// Represent the collection as `Span`
        /// </summary>
        [RefListIterator] public static Span<T> AsSpan<T>([DynNoResize] this ref ManagedRefList<T> self)
            where T : struct => new Span<T>(self.Array, 0, self.Count);
    }
}