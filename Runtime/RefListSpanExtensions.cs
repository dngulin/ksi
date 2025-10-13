using System;

#pragma warning disable REFLIST01

namespace Ksi
{
    /// <summary>
    /// Extension methods to represent <c>RefList</c> collection as <see cref="Span{T}"/> or <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class RefListSpanExtensions
    {
        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the list.</returns>
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        [RefListIterator] public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in TempRefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the list.</returns>
        [RefListIterator] public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in ManagedRefList<T> self)
            where T : struct => new ReadOnlySpan<T>(self.Array, 0, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> wrapping the list.</returns>
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        [RefListIterator] public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> wrapping the list.</returns>
        [RefListIterator] public static Span<T> AsSpan<T>([DynNoResize] this ref ManagedRefList<T> self)
            where T : struct => new Span<T>(self.Array, 0, self.Count);
    }
}