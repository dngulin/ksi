using System;
using Ksi.Internal;

namespace Ksi
{
    /// <summary>
    /// Extension methods to represent <c>TRefList&lt;T&gt;</c> as <see cref="Span{T}"/> or <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static class RefListSpanExtensions
    {
        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the list.</returns>
        [RefListIterator]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<[ExplicitCopy, DynSized, Dealloc] T>(this in RefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        [RefListIterator]
        public static unsafe ReadOnlySpan<T> AsReadOnlySpan<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self)
            where T : unmanaged => new ReadOnlySpan<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="ReadOnlySpan{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsReadOnlySpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the list.</returns>
        [RefListIterator]
        public static ReadOnlySpan<T> AsReadOnlySpan<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self)
            where T : struct
        {
#pragma warning disable KSIGENERIC01
            return new ReadOnlySpan<T>(self.Array, 0, self.Count);
#pragma warning restore KSIGENERIC01
        }

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> wrapping the list.</returns>
        [RefListIterator]
        public static unsafe Span<T> AsSpan<[ExplicitCopy, DynSized, Dealloc] T>([DynNoResize] this ref RefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        [RefListIterator]
        public static unsafe Span<T> AsSpan<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>([DynNoResize] this ref TempRefList<T> self)
            where T : unmanaged => new Span<T>(self.Array.Buffer, self.Count);

        /// <summary>
        /// <para>Wraps the collection with <see cref="Span{T}"/>.</para>
        /// <para>Adds to <c>RefPath</c> a non-explicit segment <c>AsSpan()</c>.</para>
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> wrapping the list.</returns>
        [RefListIterator]
        public static Span<T> AsSpan<[ExplicitCopy, DynSized, Dealloc] T>([DynNoResize] this ref ManagedRefList<T> self)
            where T : struct
        {
#pragma warning disable KSIGENERIC01
            return new Span<T>(self.Array, 0, self.Count);
#pragma warning restore KSIGENERIC01
        }
    }
}