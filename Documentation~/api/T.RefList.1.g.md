# RefList\<T\>

Dynamic array collection wrapping the `Persistent` allocator.
Requires manual deallocation.

```csharp
public struct RefList<T> where T : unmanaged
```

Static Creation Methods
- [RefList.Empty\<T\>\(\)](#reflistemptyt) — creates an empty list
- [RefList.WithCapacity\<T\>\(int\)](#reflistwithcapacitytint) — creates a list with a given capacity
- [RefList.WithDefaultItems\<T\>\(int\)](#reflistwithdefaultitemstint) — creates a list filled with `default` items

Extension Methods
- [\(in RefList\<T\>\).AsReadOnlySpan\(\)](#in-reflisttasreadonlyspan) — represent the collection as [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1)
- [\(in RefList\<T\>\).Capacity\(\)](#in-reflisttcapacity) — returns capacity of the list
- [\(in RefList\<T\>\).CopyTo\(ref RefList\<T\>\)](#in-reflisttcopytoref-reflistt) — copies all items to another list
- [\(in RefList\<T\>\).Count\(\)](#in-reflisttcount) — returns item count in the list
- [\(in RefList\<T\>\).RefReadonlyAt\(int\)](#in-reflisttrefreadonlyatint) — returns a readonly reference to a list item
- [\(in RefList\<T\>\).RefReadonlyIterReversed\(\)](#in-reflisttrefreadonlyiterreversed) — creates a readonly reversed by-ref iterator for the list
- [\(in RefList\<T\>\).RefReadonlyIter\(\)](#in-reflisttrefreadonlyiter) — creates a readonly by-ref iterator for the list
- [\(in RefList\<byte\>\).ToStringAscii\(\)](#in-reflistbytetostringascii) — creates a string interpreting list contents as ASCII bytes
- [\(in RefList\<byte\>\).ToStringUtf8\(\)](#in-reflistbytetostringutf8) — creates a string interpreting list contents as UTF-8 bytes
- [\(ref RefList\<T\>\).Add\(T\)](#ref-reflisttaddt) — adds a new item to the list
- [\(ref RefList\<T\>\).AppendDefault\(int\)](#ref-reflisttappenddefaultint) — adds a specified number of `default` items
- [\(ref RefList\<T\>\).AsSpan\(\)](#ref-reflisttasspan) — represent the collection as [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1)
- [\(ref RefList\<T\>\).Clear\(\)](#ref-reflisttclear) — removes all items from the list
- [\(ref RefList\<T\>\).CopyFrom\(in RefList\<T\>\)](#ref-reflisttcopyfromin-reflistt) — copies all items from another list
- [\(ref RefList\<T\>\).Dealloc\(\)](#ref-reflisttdealloc) — deallocate the list
- [\(ref RefList\<T\>\).Deallocated\(\)](#ref-reflisttdeallocated) — deallocate the list and returns it
- [\(ref RefList\<T\>\).RefAdd\(\)](#ref-reflisttrefadd) — adds a `default` item to the list and returns a mutable reference to it
- [\(ref RefList\<T\>\).RefAt\(int\)](#ref-reflisttrefatint) — returns a mutable reference to a list item
- [\(ref RefList\<T\>\).RefIterReversed\(\)](#ref-reflisttrefiterreversed) — creates a mutable reversed by-ref iterator for the list
- [\(ref RefList\<T\>\).RefIter\(\)](#ref-reflisttrefiter) — creates a mutable by-ref iterator for the list
- [\(ref RefList\<T\>\).RemoveAt\(int\)](#ref-reflisttremoveatint) — removes an item from the list at the given index
- [\(ref RefList\<byte\>\).AppendAsciiString\(string\)](#ref-reflistbyteappendasciistringstring) — appends a given string to the list as ASCII bytes
- [\(ref RefList\<byte\>\).AppendUtf8String\(string\)](#ref-reflistbyteappendutf8stringstring) — appends a given string to the list as UTF-8 bytes


## Static Creation Methods


### RefList.Empty\<T\>\(\)

Creates an empty list

```csharp
public static RefList<T> Empty<T>() where T : unmanaged
```

Returns a new empty insatnce of the [RefList\<T\>](T.RefList.1.g.md).


### RefList.WithCapacity\<T\>\(int\)

Creates a list with a given capacity.

```csharp
public static RefList<T> WithCapacity<T>(int capacity) where T : unmanaged
```

Parameters
- `capacity` — capacity of the list

Returns a new insatnce of the [RefList\<T\>](T.RefList.1.g.md) with the given capacity.


### RefList.WithDefaultItems\<T\>\(int\)

Creates a list filled with `default` items.

```csharp
public static RefList<T> WithDefaultItems<T>(int count) where T : unmanaged
```

Parameters
- `count` — number of items

Returns a new insatnce of the [RefList\<T\>](T.RefList.1.g.md) with the given number of `default` items.


## Extension Methods


### \(in RefList\<T\>\).AsReadOnlySpan\(\)

Represent the collection as [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1).

```csharp
public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self) where T : unmanaged
```

Returns a [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1) wrapping the list.


### \(in RefList\<T\>\).Capacity\(\)

Returns capacity of the list.

```csharp
public static int Capacity<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get capacity

Returns capacity of the list (zero if the buffer is deallocated).


### \(in RefList\<T\>\).CopyTo\(ref RefList\<T\>\)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in RefList<T> self, ref RefList<T> other) where T : unmanaged
```

Parameters
- `self` — source list
- `other` — destination list


### \(in RefList\<T\>\).Count\(\)

Returns item count in the list.

```csharp
public static int Count<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get item count

Returns item count in the list.


### \(in RefList\<T\>\).RefReadonlyAt\(int\)

Returns a readonly reference to a list item.

```csharp
public static ref readonly T RefReadonlyAt<T>(this in RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index.

> [!CAUTION]
> Possible exceptions: 
> - `IndexOutOfRangeException` — if index is out of bounds


### \(in RefList\<T\>\).RefReadonlyIterReversed\(\)

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static RefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in RefList\<T\>\).RefReadonlyIter\(\)

Creates a readonly by-ref iterator for the list.

```csharp
public static RefListReadOnlyIterator<T> RefReadonlyIter<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in RefList\<byte\>\).ToStringAscii\(\)

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in RefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(in RefList\<byte\>\).ToStringUtf8\(\)

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in RefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(ref RefList\<T\>\).Add\(T\)

Adds a new item to the list.

```csharp
public static void Add<T>(this ref RefList<T> self, T item) where T : unmanaged
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### \(ref RefList\<T\>\).AppendDefault\(int\)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<T>(this ref RefList<T> self, int count) where T : unmanaged
```

Parameters
- `self` — list add items
- `count` — number of items to add

> [!CAUTION]
> Possible exceptions: 
> - `ArgumentException` — if count is negative


### \(ref RefList\<T\>\).AsSpan\(\)

Represent the collection as [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1).

```csharp
public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self) where T : unmanaged
```

Returns a [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1) wrapping the list.


### \(ref RefList\<T\>\).Clear\(\)

Removes all items from the list.

```csharp
public static void Clear<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to clear


### \(ref RefList\<T\>\).CopyFrom\(in RefList\<T\>\)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref RefList<T> self, in RefList<T> other) where T : unmanaged
```

Parameters
- `self` — destination list
- `other` — source list


### \(ref RefList\<T\>\).Dealloc\(\)

Deallocate the list.
After deallocating the structure becomes zeroed.

```csharp
public static void Dealloc<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to deallocate


### \(ref RefList\<T\>\).Deallocated\(\)

Deallocate the list and returns it.

```csharp
public static ref RefList<T> Deallocated<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to deallocate

Returns the list as an assignable reference.


### \(ref RefList\<T\>\).RefAdd\(\)

Adds a `default` item to the list and returns a mutable reference to it.

```csharp
public static ref T RefAdd<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item.


### \(ref RefList\<T\>\).RefAt\(int\)

Returns a mutable reference to a list item.

```csharp
public static ref T RefAt<T>([DynNoResize] this ref RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index.


### \(ref RefList\<T\>\).RefIterReversed\(\)

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static RefListIteratorReversed<T> RefIterReversed<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref RefList\<T\>\).RefIter\(\)

Creates a mutable by-ref iterator for the list.

```csharp
public static RefListIterator<T> RefIter<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref RefList\<T\>\).RemoveAt\(int\)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<T>(this ref RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item.

> [!CAUTION]
> Possible exceptions: 
> - `IndexOutOfRangeException` — if index is out of bounds


### \(ref RefList\<byte\>\).AppendAsciiString\(string\)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref RefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### \(ref RefList\<byte\>\).AppendUtf8String\(string\)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref RefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
