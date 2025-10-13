namespace Ksi.Roslyn;

public static class RefListTemplates
{
    public const string StaticApi =
        // language=cs
        """
        #pragma warning disable REFLIST01

        namespace Ksi
        {{
            /// <summary>
            /// {0} static constructors
            /// </summary>
            public static class {0}
            {{
                /// <summary>
                /// Creates an empty list
                /// </summary>
                /// <returns>A new empty instance of the <see cref="{0}{{T}}"/>.</returns>
                public static {0}<T> Empty<T>() where T : {1} => default;

                /// <summary>
                /// Creates a list with a given capacity.
                /// </summary>
                /// <param name="capacity">Capacity of the list</param>
                /// <returns>A new instance of the <see cref="{0}{{T}}"/> with the given capacity.</returns>
                public static {0}<T> WithCapacity<T>(int capacity) where T : {1}
                {{
                    var list = Empty<T>();
                    list.SetBufferSize(capacity);
                    return list;
                }}

                /// <summary>
                /// Creates a list filled with <c>default</c> items.
                /// </summary>
                /// <param name="count">Number of items</param>
                /// <returns>A new instance of the <see cref="{0}{{T}}"/> with the given number of <c>default</c> items.</returns>
                public static {0}<T> WithDefaultItems<T>(int count) where T : {1}
                {{
                    var list = WithCapacity<T>(count);
                    list.Count = count;
                    return list;
                }}
            }}
        }}
        """;

    public const string InstanceApi =
        // language=cs
        """
        using System;

        #pragma warning disable REFLIST01

        namespace Ksi
        {{
            /// <summary>
            /// {0} API.
            /// </summary>
            public static class {0}_Api
            {{
                /// <summary>
                /// Returns capacity of the list.
                /// </summary>
                /// <param name="self">List to get capacity</param>
                /// <returns>Capacity of the list (zero if the buffer is deallocated).</returns>
                public static int Capacity<T>(this in {0}<T> self) where T : {1} => self.GetBufferSize();

                /// <summary>
                /// Returns item count in the list.
                /// </summary>
                /// <param name="self">List to get item count</param>
                /// <returns>Item count in the list.</returns>
                public static int Count<T>(this in {0}<T> self) where T : {1} => self.Count;

                /// <summary>
                /// <para>Returns a readonly reference to a list item.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to get an item reference</param>
                /// <param name="index">Required item index</param>
                /// <returns>A readonly reference to a list item at the given index.</returns>
                /// <exception cref="IndexOutOfRangeException">If the index is out of bounds</exception>
                [RefListIndexer]
                public static ref readonly T RefReadonlyAt<T>(this in {0}<T> self, int index) where T : {1}
                {{
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    return ref self.IndexBuffer(index);
                }}

                /// <summary>
                /// <para>Returns a mutable reference to a list item.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to get an item reference</param>
                /// <param name="index">Required item index</param>
                /// <returns>A mutable reference to a list item at the given index.</returns>
                [RefListIndexer]
                public static ref T RefAt<T>([DynNoResize] this ref {0}<T> self, int index) where T : {1}
                {{
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    return ref self.IndexBufferMut(index);
                }}

                /// <summary>
                /// Adds a new item to the list.
                /// </summary>
                /// <param name="self">List to add an item</param>
                /// <param name="item">Item to add to the list</param>
                public static void Add<T>(this ref {0}<T> self, T item) where T : {1}
                {{
                    self.EnsureCanAdd();
                    self.IndexBufferMut(self.Count++) = item;
                }}

                /// <summary>
                /// <para>Adds a <c>default</c> item to the list and returns a mutable reference to it.</para>
                /// <para>Adds to <c>RefPath</c> an indexer segment <c>[n]</c>.</para>
                /// </summary>
                /// <param name="self">List to add an item</param>
                /// <returns>A mutable reference to the created item.</returns>
                [NonAllocatedResult, RefListIndexer]
                public static ref T RefAdd<T>(this ref {0}<T> self) where T : {1}
                {{
                    self.EnsureCanAdd();
                    return ref self.IndexBufferMut(self.Count++);
                }}

                private static void EnsureCanAdd<T>(this ref {0}<T> self) where T : {1}
                {{
                    if (self.Count < self.Capacity())
                       return;

                    var newSize = Math.Max(self.Capacity() * 2, 1);
                    self.SetBufferSize(newSize);
                }}

                /// <summary>
                /// Removes an item from the list at the given index.
                /// </summary>
                /// <param name="self">List to remove the item</param>
                /// <param name="index">An index to remove the item.</param>
                /// <exception cref="IndexOutOfRangeException">If the index is out of bounds</exception>
                public static void RemoveAt<T>(this ref {0}<T> self, int index) where T : {1}
                {{
                    if (index < 0 || index >= self.Count)
                        throw new IndexOutOfRangeException();

                    self.Count--;
                    self.CopyWithinBuffer(index + 1, index, self.Count - index);
                    self.IndexBufferMut(self.Count) = default;
                }}

                /// <summary>
                /// Removes all items from the list.
                /// </summary>
                /// <param name="self">List to clear</param>
                public static void Clear<T>(this ref {0}<T> self) where T : {1}
                {{
                    if (self.Count == 0)
                        return;

                    self.ClearBuffer();
                    self.Count = 0;
                }}

                /// <summary>
                /// Adds a specified number of <c>default</c> items.
                /// </summary>
                /// <param name="self">List add items</param>
                /// <param name="count">Number of items to add</param>
                /// <exception cref="ArgumentException">If the count is negative</exception>
                public static void AppendDefault<T>(this ref {0}<T> self, int count) where T : {1}
                {{
                    if (count < 0)
                        throw new ArgumentException();

                    var newCount = self.Count + count;

                    if (self.Capacity() < newCount)
                        self.SetBufferSize(newCount);

                    self.Count = newCount;
                }}

                /// <summary>
                /// Copies all items from another list.
                /// All items existing before copying are removed.
                /// </summary>
                /// <param name="self">Destination list</param>
                /// <param name="other">Source list</param>
                public static void CopyFrom<T>(this ref {0}<T> self, in {0}<T> other) where T : {1}
                {{
                    self.Clear();
                    self.AppendDefault(other.Count());
                    self.CopyBufferFrom(other);
                }}

                /// <summary>
                /// Copies all items to another list.
                /// All items existing before copying are removed.
                /// </summary>
                /// <param name="self">Source list</param>
                /// <param name="other">Destination list</param>
                public static void CopyTo<T>(this in {0}<T> self, ref {0}<T> other) where T : {1}
                {{
                    other.CopyFrom(self);
                }}
            }}
        }}
        """;

    public const string Dealloc =
        // language=cs
        """
        using System;

        #pragma warning disable REFLIST01

        namespace Ksi
        {{
            /// <summary>
            /// {0} deallocation extensions.
            /// </summary>
            public static class {0}_Dealloc
            {{
                /// <summary>
                /// Deallocate the list.
                /// After deallocating the structure becomes zeroed.
                /// </summary>
                /// <param name="self">List to deallocate</param>
                public static void Dealloc<T>(this ref {0}<T> self) where T : {1} => self.SetBufferSize(0);

                /// <summary>
                /// <para>Deallocate the list and returns it.</para>
                /// <para>Does not add any segments to <c>RefPath</c>.</para>
                /// </summary>
                /// <param name="self">List to deallocate</param>
                /// <returns>The list as an assignable reference.</returns>
                [RefPath("self", "!"), NonAllocatedResult]
                public static ref {0}<T> Deallocated<T>(this ref {0}<T> self) where T : {1}
                {{
                    self.Dealloc();
                    return ref self;
                }}
            }}
        }}
        """;

    public const string Iterators =
        // language=cs
        """
        using System;

        #pragma warning disable REFLIST01

        namespace Ksi
        {{
            /// <summary>
            /// {0} iterators.
            /// </summary>
            public static class {0}_IteratorExtensions
            {{
                
                /// <summary>
                /// Creates a readonly by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static {0}ReadOnlyIterator<T> RefReadonlyIter<T>(this in {0}<T> self) where T : {1}
                {{
                    return new {0}ReadOnlyIterator<T>(self.AsReadOnlySpan());
                }}

                /// <summary>
                /// Creates a mutable by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static {0}Iterator<T> RefIter<T>(this ref {0}<T> self) where T : {1}
                {{
                    return new {0}Iterator<T>(self.AsSpan());
                }}

                /// <summary>
                /// Creates a readonly reversed by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static {0}ReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in {0}<T> self) where T : {1}
                {{
                    return new {0}ReadOnlyIteratorReversed<T>(self.AsReadOnlySpan());
                }}

                /// <summary>
                /// Creates a mutable reversed by-ref iterator for the list.
                /// </summary>
                /// <param name="self">List to iterate</param>
                /// <returns>The iterator to use in the foreach loop.</returns>
                [RefListIterator]
                public static {0}IteratorReversed<T> RefIterReversed<T>(this ref {0}<T> self) where T : {1}
                {{
                    return new {0}IteratorReversed<T>(self.AsSpan());
                }}
            }}
            
            // Suppress missing docstrings warning for trivial iterator implementations
            #pragma warning disable CS1591

            public readonly ref struct {0}Iterator<T> where T : {1}
            {{
                private readonly Span<T> _span;
                public {0}Iterator(in Span<T> span) => _span = span;
                public {0}Enumerator<T> GetEnumerator() => new {0}Enumerator<T>(_span);
            }}

            public ref struct {0}Enumerator<T> where T : {1}
            {{
                private Span<T> _span;
                private int _curr;

                public {0}Enumerator(in Span<T> span)
                {{
                    _span = span;
                    _curr = -1;
                }}

                public ref T Current => ref _span[_curr];
                public bool MoveNext() => ++_curr < _span.Length;
                public void Reset() => _curr = -1;
                public void Dispose() {{}}
            }}

            public readonly ref struct {0}ReadOnlyIterator<T> where T : {1}
            {{
                private readonly ReadOnlySpan<T> _span;
                public {0}ReadOnlyIterator(in ReadOnlySpan<T> span) => _span = span;
                public {0}ReadOnlyEnumerator<T> GetEnumerator() => new {0}ReadOnlyEnumerator<T>(_span);
            }}

            public ref struct {0}ReadOnlyEnumerator<T> where T : {1}
            {{
                private readonly ReadOnlySpan<T> _span;
                private int _curr;

                public {0}ReadOnlyEnumerator(in ReadOnlySpan<T> span)
                {{
                    _span = span;
                    _curr = -1;
                }}

                public ref readonly T Current => ref _span[_curr];
                public bool MoveNext() => ++_curr < _span.Length;
                public void Reset() => _curr = -1;
                public void Dispose() {{}}
            }}

            public readonly ref struct {0}IteratorReversed<T> where T : {1}
            {{
                private readonly Span<T> _span;
                public {0}IteratorReversed(in Span<T> span) => _span = span;
                public {0}EnumeratorReversed<T> GetEnumerator() => new {0}EnumeratorReversed<T>(_span);
            }}

            public ref struct {0}EnumeratorReversed<T> where T : {1}
            {{
                private Span<T> _span;
                private int _curr;

                public {0}EnumeratorReversed(in Span<T> span)
                {{
                    _span = span;
                    _curr = span.Length;
                }}

                public ref T Current => ref _span[_curr];
                public bool MoveNext() => --_curr >= 0;
                public void Reset() => _curr = -1;
                public void Dispose() {{}}
            }}

            public readonly ref struct {0}ReadOnlyIteratorReversed<T> where T : {1}
            {{
                private readonly ReadOnlySpan<T> _span;
                public {0}ReadOnlyIteratorReversed(in ReadOnlySpan<T> span) => _span = span;
                public {0}ReadOnlyEnumeratorReversed<T> GetEnumerator() => new {0}ReadOnlyEnumeratorReversed<T>(_span);
            }}

            public ref struct {0}ReadOnlyEnumeratorReversed<T> where T : {1}
            {{
                private readonly ReadOnlySpan<T> _span;
                private int _curr;

                public {0}ReadOnlyEnumeratorReversed(in ReadOnlySpan<T> span)
                {{
                    _span = span;
                    _curr = span.Length;
                }}

                public ref readonly T Current => ref _span[_curr];
                public bool MoveNext() => --_curr >= 0;
                public void Reset() => _curr = _span.Length;
                public void Dispose() {{}}
            }}
            
            #pragma warning restore CS1591
        }}
        """;

    public const string StringExt =
        // language=cs
        """
        using System;
        using System.Text;

        namespace Ksi
        {{
            /// <summary>
            /// {0} extensions to encode and decode strings.
            /// All extensions internally use the `System.Encoding` and are not compatible with Burst.
            /// </summary>
            public static class {0}_StringExtensions
            {{
                /// <summary>
                /// Appends a given string to the list as UTF-8 bytes.
                /// </summary>
                /// <param name="self">List to append bytes</param>
                /// <param name="value">String value to append</param>
                public static void AppendUtf8String(this ref {0}<byte> self, string value)
                {{
                    if (string.IsNullOrEmpty(value))
                        return;

                    var pos = self.Count();
                    var len = Encoding.UTF8.GetByteCount(value);

                    self.AppendDefault(len);
                    Encoding.UTF8.GetBytes(value.AsSpan(), self.AsSpan().Slice(pos, len));
                }}

                /// <summary>
                /// Appends a given string to the list as ASCII bytes.
                /// </summary>
                /// <param name="self">List to append bytes</param>
                /// <param name="value">String value to append</param>
                public static void AppendAsciiString(this ref {0}<byte> self, string value)
                {{
                    if (string.IsNullOrEmpty(value))
                        return;

                    var pos = self.Count();
                    var len = value.Length;

                    self.AppendDefault(len);
                    Encoding.ASCII.GetBytes(value.AsSpan(), self.AsSpan().Slice(pos, len));
                }}

                /// <summary>
                /// Creates a string interpreting list contents as UTF-8 bytes.
                /// </summary>
                /// <param name="self">List containing string bytes</param>
                /// <returns>The string created from bytes.</returns>
                public static string ToStringUtf8(this in {0}<byte> self)
                {{
                    return self.Count == 0 ? "" : Encoding.UTF8.GetString(self.AsReadOnlySpan());
                }}

                /// <summary>
                /// Creates a string interpreting list contents as ASCII bytes.
                /// </summary>
                /// <param name="self">List containing string bytes</param>
                /// <returns>The string created from bytes.</returns>
                public static string ToStringAscii(this in {0}<byte> self)
                {{
                    return self.Count == 0 ? "" : Encoding.ASCII.GetString(self.AsReadOnlySpan());
                }}
            }}
        }}
        """;
}