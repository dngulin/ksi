namespace Ksi.Roslyn;

public static class RefListTemplates
{
    public const string StaticApi =
        // language=cs
        """
        namespace Ksi
        {
            /// <summary>
            /// |TRefList| static constructors
            /// </summary>
            public static class |TRefList|
            {
                /// <summary>
                /// Creates an empty list
                /// </summary>
                /// <returns>A new empty instance of the <see cref="|TRefList|{T}"/>.</returns>
                public static |TRefList|<T> Empty<T>() where T : |constraint| => default;

                /// <summary>
                /// Creates a list with a given capacity.
                /// </summary>
                /// <param name="capacity">Capacity of the list</param>
                /// <returns>A new instance of the <see cref="|TRefList|{T}"/> with the given capacity.</returns>
                public static |TRefList|<T> WithCapacity<T>(int capacity) where T : |constraint|
                {
                    var list = Empty<T>();
                    list.SetBufferSize(capacity);
                    return list;
                }

                /// <summary>
                /// Creates a list filled with <c>default</c> items.
                /// </summary>
                /// <param name="count">Number of items</param>
                /// <returns>A new instance of the <see cref="|TRefList|{T}"/> with the given number of <c>default</c> items.</returns>
                public static |TRefList|<T> WithDefaultItems<T>(int count) where T : |constraint|
                {
                    var list = WithCapacity<T>(count);
                    list.Count = count;
                    return list;
                }
            }
        }
        """;

    public const string InstanceApi =
        // language=cs
        """
        using System;

        namespace Ksi
        {
            /// <summary>
            /// |TRefList| API.
            /// </summary>
            public static class |TRefList|_Api
            {
                /// <summary>
                /// Returns capacity of the list.
                /// </summary>
                /// <param name="self">List to get capacity</param>
                /// <returns>Capacity of the list (zero if the buffer is deallocated).</returns>
                public static int Capacity<[|TraitsAll|] T>(this in |TRefList|<T> self) where T : |constraint| => self.GetBufferSize();

                /// <summary>
                /// Returns item count in the list.
                /// </summary>
                /// <param name="self">List to get item count</param>
                /// <returns>Item count in the list.</returns>
                public static int Count<[|TraitsAll|] T>(this in |TRefList|<T> self) where T : |constraint| => self.Count;

                /// <summary>
                /// <para>Returns a readonly reference to a list item.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to get an item reference</param>
                /// <param name="index">Required item index</param>
                /// <returns>A readonly reference to a list item at the given index.</returns>
                /// <exception cref="IndexOutOfRangeException">If the index is out of bounds</exception>
                [RefListIndexer]
                public static ref readonly T RefReadonlyAt<[|TraitsAll|] T>(this in |TRefList|<T> self, int index) where T : |constraint|
                {
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    return ref self.IndexBuffer(index);
                }

                /// <summary>
                /// <para>Returns a mutable reference to a list item.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to get an item reference</param>
                /// <param name="index">Required item index</param>
                /// <returns>A mutable reference to a list item at the given index.</returns>
                [RefListIndexer]
                public static ref T RefAt<[|TraitsAll|] T>([DynNoResize] this ref |TRefList|<T> self, int index)
                    where T : |constraint|
                {
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    return ref self.IndexBufferMut(index);
                }

                /// <summary>
                /// Adds a new item to the list.
                /// </summary>
                /// <param name="self">List to add an item</param>
                /// <param name="item">Item to add to the list</param>
                public static void Add<[|TraitsAll|] T>(this ref |TRefList|<T> self, T item) where T : |constraint|
                {
                    self.EnsureCanAdd();
                    #pragma warning disable EXPCOPY04, DEALLOC04
                    self.IndexBufferMut(self.Count++) = item;
                    #pragma warning restore EXPCOPY04, DEALLOC04
                }

                /// <summary>
                /// <para>Adds a <c>default</c> item to the list and returns a mutable reference to it.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to add an item</param>
                /// <returns>A mutable reference to the created item.</returns>
                [NonAllocatedResult, RefListIndexer]
                public static ref T RefAdd<[|TraitsAll|] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    self.EnsureCanAdd();
                    return ref self.IndexBufferMut(self.Count++);
                }

                private static void EnsureCanAdd<[|TraitsAll|] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    if (self.Count < self.Capacity())
                       return;

                    var newSize = Math.Max(self.Capacity() * 2, 1);
                    #pragma warning disable KSIGENERIC01
                    self.SetBufferSize(newSize);
                    #pragma warning restore KSIGENERIC01
                }

                /// <summary>
                /// Removes an item from the list at the given index.
                /// </summary>
                /// <param name="self">List to remove the item</param>
                /// <param name="index">An index to remove the item.</param>
                /// <exception cref="IndexOutOfRangeException">If the index is out of bounds</exception>
                public static void RemoveAt<[|TraitsExceptDealloc|] T>(this ref |TRefList|<T> self, int index) where T : |constraint|
                {
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    self.Count--;
                        
                    #pragma warning disable KSIGENERIC01
                    self.CopyWithinBuffer(index + 1, index, self.Count - index);
                    #pragma warning restore KSIGENERIC01
                    
                    self.IndexBufferMut(self.Count) = default;
                }

                /// <summary>
                /// Removes all items from the list.
                /// </summary>
                /// <param name="self">List to clear</param>
                public static void Clear<[|TraitsExceptDealloc|] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    if (self.Count == 0)
                        return;

                    self.ClearBuffer();
                    self.Count = 0;
                }

                /// <summary>
                /// Adds a specified number of <c>default</c> items.
                /// </summary>
                /// <param name="self">List add items</param>
                /// <param name="count">Number of items to add</param>
                /// <exception cref="ArgumentException">If the count is negative</exception>
                public static void AppendDefault<[|TraitsAll|] T>(this ref |TRefList|<T> self, int count) where T : |constraint|
                {
                    if (count < 0)
                        throw new ArgumentException();

                    var newCount = self.Count + count;

                    if (self.Capacity() < newCount)
                        #pragma warning disable KSIGENERIC01
                        self.SetBufferSize(newCount);
                        #pragma warning restore KSIGENERIC01

                    self.Count = newCount;
                }

                /// <summary>
                /// Copies all items from another list.
                /// All items existing before copying are removed.
                /// </summary>
                /// <param name="self">Destination list</param>
                /// <param name="other">Source list</param>
                public static void CopyFrom<T>(this ref |TRefList|<T> self, in |TRefList|<T> other) where T : |constraint|
                {
                    self.Clear();
                    self.AppendDefault(other.Count());
                    self.CopyBufferFrom(other);
                }

                /// <summary>
                /// Copies all items to another list.
                /// All items existing before copying are removed.
                /// </summary>
                /// <param name="self">Source list</param>
                /// <param name="other">Destination list</param>
                public static void CopyTo<T>(this in |TRefList|<T> self, ref |TRefList|<T> other) where T : |constraint|
                {
                    other.CopyFrom(self);
                }
            }
        }
        """;

    public const string Dealloc =
        // language=cs
        """
        using System;

        namespace Ksi
        {
            /// <summary>
            /// |TRefList| deallocation extensions.
            /// </summary>
            public static class |TRefList|_Dealloc
            {
                /// <summary>
                /// Deallocate the list.
                /// After deallocating the structure becomes zeroed.
                /// </summary>
                /// <param name="self">List to deallocate</param>
                public static void Dealloc<[ExplicitCopy] T>(this ref |TRefList|<T> self) where T : |constraint| => self.SetBufferSize(0);

                /// <summary>
                /// <para>Deallocate the list and returns it.</para>
                /// <para>Does not add any segments to <c>RefPath</c>.</para>
                /// </summary>
                /// <param name="self">List to deallocate</param>
                /// <returns>The list as an assignable reference.</returns>
                [RefPath("self", "!"), NonAllocatedResult]
                public static ref |TRefList|<T> Deallocated<[ExplicitCopy] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    self.Dealloc();
                    return ref self;
                }
            }
        }
        """;

    public const string Iterators =
        // language=cs
        """
        using System;

        namespace Ksi
        {
            /// <summary>
            /// |TRefList| iterators.
            /// </summary>
            public static class |TRefList|_IteratorExtensions
            {
                
                /// <summary>
                /// Creates a readonly by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static |TRefList|ReadOnlyIterator<T> RefReadonlyIter<[|TraitsAll|] T>(this in |TRefList|<T> self) where T : |constraint|
                {
                    return new |TRefList|ReadOnlyIterator<T>(self.AsReadOnlySpan());
                }

                /// <summary>
                /// Creates a mutable by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static |TRefList|Iterator<T> RefIter<[|TraitsAll|] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    return new |TRefList|Iterator<T>(self.AsSpan());
                }

                /// <summary>
                /// Creates a readonly reversed by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static |TRefList|ReadOnlyIteratorReversed<T> RefReadonlyIterReversed<[|TraitsAll|] T>(this in |TRefList|<T> self) where T : |constraint|
                {
                    return new |TRefList|ReadOnlyIteratorReversed<T>(self.AsReadOnlySpan());
                }

                /// <summary>
                /// Creates a mutable reversed by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static |TRefList|IteratorReversed<T> RefIterReversed<[|TraitsAll|] T>(this ref |TRefList|<T> self) where T : |constraint|
                {
                    return new |TRefList|IteratorReversed<T>(self.AsSpan());
                }
            }
            
            // Suppress missing docstrings warning for trivial iterator implementations
            #pragma warning disable CS1591

            public readonly ref struct |TRefList|Iterator<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly Span<T> _span;
                public |TRefList|Iterator(in Span<T> span) => _span = span;
                public |TRefList|Enumerator<T> GetEnumerator() => new |TRefList|Enumerator<T>(_span);
            }

            public ref struct |TRefList|Enumerator<[|TraitsAll|] T> where T : |constraint|
            {
                private Span<T> _span;
                private int _curr;

                public |TRefList|Enumerator(in Span<T> span)
                {
                    _span = span;
                    _curr = -1;
                }

                public ref T Current => ref _span[_curr];
                public bool MoveNext() => ++_curr < _span.Length;
                public void Reset() => _curr = -1;
                public void Dispose() {}
            }

            public readonly ref struct |TRefList|ReadOnlyIterator<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly ReadOnlySpan<T> _span;
                public |TRefList|ReadOnlyIterator(in ReadOnlySpan<T> span) => _span = span;
                public |TRefList|ReadOnlyEnumerator<T> GetEnumerator() => new |TRefList|ReadOnlyEnumerator<T>(_span);
            }

            public ref struct |TRefList|ReadOnlyEnumerator<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly ReadOnlySpan<T> _span;
                private int _curr;

                public |TRefList|ReadOnlyEnumerator(in ReadOnlySpan<T> span)
                {
                    _span = span;
                    _curr = -1;
                }

                public ref readonly T Current => ref _span[_curr];
                public bool MoveNext() => ++_curr < _span.Length;
                public void Reset() => _curr = -1;
                public void Dispose() {}
            }

            public readonly ref struct |TRefList|IteratorReversed<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly Span<T> _span;
                public |TRefList|IteratorReversed(in Span<T> span) => _span = span;
                public |TRefList|EnumeratorReversed<T> GetEnumerator() => new |TRefList|EnumeratorReversed<T>(_span);
            }

            public ref struct |TRefList|EnumeratorReversed<[|TraitsAll|] T> where T : |constraint|
            {
                private Span<T> _span;
                private int _curr;

                public |TRefList|EnumeratorReversed(in Span<T> span)
                {
                    _span = span;
                    _curr = span.Length;
                }

                public ref T Current => ref _span[_curr];
                public bool MoveNext() => --_curr >= 0;
                public void Reset() => _curr = -1;
                public void Dispose() {}
            }

            public readonly ref struct |TRefList|ReadOnlyIteratorReversed<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly ReadOnlySpan<T> _span;
                public |TRefList|ReadOnlyIteratorReversed(in ReadOnlySpan<T> span) => _span = span;
                public |TRefList|ReadOnlyEnumeratorReversed<T> GetEnumerator() => new |TRefList|ReadOnlyEnumeratorReversed<T>(_span);
            }

            public ref struct |TRefList|ReadOnlyEnumeratorReversed<[|TraitsAll|] T> where T : |constraint|
            {
                private readonly ReadOnlySpan<T> _span;
                private int _curr;

                public |TRefList|ReadOnlyEnumeratorReversed(in ReadOnlySpan<T> span)
                {
                    _span = span;
                    _curr = span.Length;
                }

                public ref readonly T Current => ref _span[_curr];
                public bool MoveNext() => --_curr >= 0;
                public void Reset() => _curr = _span.Length;
                public void Dispose() {}
            }
            
            #pragma warning restore CS1591
        }
        """;

    public const string StringExt =
        // language=cs
        """
        using System;
        using System.Text;

        namespace Ksi
        {
            /// <summary>
            /// |TRefList| extensions to encode and decode strings.
            /// All extensions internally use the `System.Encoding` and are not compatible with Burst.
            /// </summary>
            public static class |TRefList|_StringExtensions
            {
                /// <summary>
                /// Appends a given string to the list as UTF-8 bytes.
                /// </summary>
                /// <param name="self">List to append bytes</param>
                /// <param name="value">String value to append</param>
                public static void AppendUtf8String(this ref |TRefList|<byte> self, string value)
                {
                    if (string.IsNullOrEmpty(value))
                        return;

                    var pos = self.Count();
                    var len = Encoding.UTF8.GetByteCount(value);

                    self.AppendDefault(len);
                    Encoding.UTF8.GetBytes(value.AsSpan(), self.AsSpan().Slice(pos, len));
                }

                /// <summary>
                /// Appends a given string to the list as ASCII bytes.
                /// </summary>
                /// <param name="self">List to append bytes</param>
                /// <param name="value">String value to append</param>
                public static void AppendAsciiString(this ref |TRefList|<byte> self, string value)
                {
                    if (string.IsNullOrEmpty(value))
                        return;

                    var pos = self.Count();
                    var len = value.Length;

                    self.AppendDefault(len);
                    Encoding.ASCII.GetBytes(value.AsSpan(), self.AsSpan().Slice(pos, len));
                }

                /// <summary>
                /// Creates a string interpreting list contents as UTF-8 bytes.
                /// </summary>
                /// <param name="self">List containing string bytes</param>
                /// <returns>The string created from bytes.</returns>
                public static string ToStringUtf8(this in |TRefList|<byte> self)
                {
                    return self.Count == 0 ? "" : Encoding.UTF8.GetString(self.AsReadOnlySpan());
                }

                /// <summary>
                /// Creates a string interpreting list contents as ASCII bytes.
                /// </summary>
                /// <param name="self">List containing string bytes</param>
                /// <returns>The string created from bytes.</returns>
                public static string ToStringAscii(this in |TRefList|<byte> self)
                {
                    return self.Count == 0 ? "" : Encoding.ASCII.GetString(self.AsReadOnlySpan());
                }
            }
        }
        """;
}