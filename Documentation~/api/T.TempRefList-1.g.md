# TempRefList\<T\>

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / TempRefList\<T\>**
> \]

A dynamic array collection wrapping the `Temp` allocator.
Can be stored only on stack.

```csharp
[ExplicitCopy, DynSized, TempAlloc]
public struct TempRefList<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T> where T : unmanaged
```

Static Creation Methods
- [TempRefList.Empty\<T\>\(\)](#tempreflistemptyt) — creates an empty list
- [TempRefList.WithCapacity\<T\>\(int\)](#tempreflistwithcapacitytint) — creates a list with a given capacity
- [TempRefList.WithDefaultItems\<T\>\(int\)](#tempreflistwithdefaultitemstint) — creates a list filled with `default` items

Extension Methods
- [\(in TempRefList\<T\>\).AsReadOnlySpan\(\)](#in-tempreflisttasreadonlyspan) — wraps the collection with [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1)
- [\(in TempRefList\<T\>\).Capacity\(\)](#in-tempreflisttcapacity) — returns capacity of the list
- [\(in TempRefList\<T\>\).CopyTo\(ref TempRefList\<T\>\)](#in-tempreflisttcopytoref-tempreflistt) — copies all items to another list
- [\(in TempRefList\<T\>\).Count\(\)](#in-tempreflisttcount) — returns item count in the list
- [\(in TempRefList\<T\>\).RefReadonlyAt\(int\)](#in-tempreflisttrefreadonlyatint) — returns a readonly reference to a list item
- [\(in TempRefList\<T\>\).RefReadonlyIterReversed\(\)](#in-tempreflisttrefreadonlyiterreversed) — creates a readonly reversed by-ref iterator for the list
- [\(in TempRefList\<T\>\).RefReadonlyIter\(\)](#in-tempreflisttrefreadonlyiter) — creates a readonly by-ref iterator for the list
- [\(in TempRefList\<byte\>\).ToStringAscii\(\)](#in-tempreflistbytetostringascii) — creates a string interpreting list contents as ASCII bytes
- [\(in TempRefList\<byte\>\).ToStringUtf8\(\)](#in-tempreflistbytetostringutf8) — creates a string interpreting list contents as UTF-8 bytes
- [\(ref TempRefList\<T\>\).Add\(T\)](#ref-tempreflisttaddt) — adds a new item to the list
- [\(ref TempRefList\<T\>\).AppendDefault\(int\)](#ref-tempreflisttappenddefaultint) — adds a specified number of `default` items
- [\(ref TempRefList\<T\>\).AsSpan\(\)](#ref-tempreflisttasspan) — wraps the collection with [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1)
- [\(ref TempRefList\<T\>\).Clear\(\)](#ref-tempreflisttclear) — removes all items from the list
- [\(ref TempRefList\<T\>\).CopyFrom\(in TempRefList\<T\>\)](#ref-tempreflisttcopyfromin-tempreflistt) — copies all items from another list
- [\(ref TempRefList\<T\>\).RefAdd\(\)](#ref-tempreflisttrefadd) — adds a `default` item to the list and returns a mutable reference to it
- [\(ref TempRefList\<T\>\).RefAt\(int\)](#ref-tempreflisttrefatint) — returns a mutable reference to a list item
- [\(ref TempRefList\<T\>\).RefIterReversed\(\)](#ref-tempreflisttrefiterreversed) — creates a mutable reversed by-ref iterator for the list
- [\(ref TempRefList\<T\>\).RefIter\(\)](#ref-tempreflisttrefiter) — creates a mutable by-ref iterator for the list
- [\(ref TempRefList\<T\>\).RemoveAt\(int\)](#ref-tempreflisttremoveatint) — removes an item from the list at the given index
- [\(ref TempRefList\<byte\>\).AppendAsciiString\(string\)](#ref-tempreflistbyteappendasciistringstring) — appends a given string to the list as ASCII bytes
- [\(ref TempRefList\<byte\>\).AppendUtf8String\(string\)](#ref-tempreflistbyteappendutf8stringstring) — appends a given string to the list as UTF-8 bytes


## Static Creation Methods


### TempRefList.Empty\<T\>\(\)

Creates an empty list

```csharp
public static TempRefList<T> Empty<T>() where T : unmanaged
```

Returns a new empty instance of the [TempRefList\<T\>](T.TempRefList-1.g.md).


### TempRefList.WithCapacity\<T\>\(int\)

Creates a list with a given capacity.

```csharp
public static TempRefList<T> WithCapacity<T>(int capacity) where T : unmanaged
```

Parameters
- `capacity` — capacity of the list

Returns a new instance of the [TempRefList\<T\>](T.TempRefList-1.g.md) with the given capacity.


### TempRefList.WithDefaultItems\<T\>\(int\)

Creates a list filled with `default` items.

```csharp
public static TempRefList<T> WithDefaultItems<T>(int count) where T : unmanaged
```

Parameters
- `count` — number of items

Returns a new instance of the [TempRefList\<T\>](T.TempRefList-1.g.md) with the given number of `default` items.


## Extension Methods


### \(in TempRefList\<T\>\).AsReadOnlySpan\(\)

Wraps the collection with [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1).

Adds to `RefPath` a non-explicit segment `AsReadOnlySpan()`.

```csharp
public static unsafe ReadOnlySpan<T> AsReadOnlySpan<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self) where T : unmanaged
```


### \(in TempRefList\<T\>\).Capacity\(\)

Returns capacity of the list.

```csharp
public static int Capacity<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get capacity

Returns capacity of the list (zero if the buffer is deallocated).


### \(in TempRefList\<T\>\).CopyTo\(ref TempRefList\<T\>\)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in TempRefList<T> self, ref TempRefList<T> other) where T : unmanaged
```

Parameters
- `self` — source list
- `other` — destination list


### \(in TempRefList\<T\>\).Count\(\)

Returns item count in the list.

```csharp
public static int Count<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get item count

Returns item count in the list.


### \(in TempRefList\<T\>\).RefReadonlyAt\(int\)

Returns a readonly reference to a list item.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
public static ref readonly T RefReadonlyAt<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index.

> [!CAUTION]
> Possible exceptions: 
> - [IndexOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.IndexOutOfRangeException?view=netstandard-2.1) — if the index is out of bounds


### \(in TempRefList\<T\>\).RefReadonlyIterReversed\(\)

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static TempRefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in TempRefList\<T\>\).RefReadonlyIter\(\)

Creates a readonly by-ref iterator for the list.

```csharp
public static TempRefListReadOnlyIterator<T> RefReadonlyIter<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in TempRefList\<byte\>\).ToStringAscii\(\)

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in TempRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(in TempRefList\<byte\>\).ToStringUtf8\(\)

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in TempRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(ref TempRefList\<T\>\).Add\(T\)

Adds a new item to the list.

```csharp
public static void Add<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this ref TempRefList<T> self, T item) where T : unmanaged
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### \(ref TempRefList\<T\>\).AppendDefault\(int\)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this ref TempRefList<T> self, int count) where T : unmanaged
```

Parameters
- `self` — list add items
- `count` — number of items to add

> [!CAUTION]
> Possible exceptions: 
> - [ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/System.ArgumentException?view=netstandard-2.1) — if the count is negative


### \(ref TempRefList\<T\>\).AsSpan\(\)

Wraps the collection with [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1).

Adds to `RefPath` a non-explicit segment `AsSpan()`.

```csharp
public static unsafe Span<T> AsSpan<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>([DynNoResize] this ref TempRefList<T> self) where T : unmanaged
```


### \(ref TempRefList\<T\>\).Clear\(\)

Removes all items from the list.

```csharp
public static void Clear<[ExplicitCopy, DynSized, TempAlloc] T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to clear


### \(ref TempRefList\<T\>\).CopyFrom\(in TempRefList\<T\>\)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref TempRefList<T> self, in TempRefList<T> other) where T : unmanaged
```

Parameters
- `self` — destination list
- `other` — source list


### \(ref TempRefList\<T\>\).RefAdd\(\)

Adds a `default` item to the list and returns a mutable reference to it.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
[NonAllocatedResult]
public static ref T RefAdd<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item.


### \(ref TempRefList\<T\>\).RefAt\(int\)

Returns a mutable reference to a list item.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
public static ref T RefAt<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>([DynNoResize] this ref TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index.


### \(ref TempRefList\<T\>\).RefIterReversed\(\)

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static TempRefListIteratorReversed<T> RefIterReversed<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref TempRefList\<T\>\).RefIter\(\)

Creates a mutable by-ref iterator for the list.

```csharp
public static TempRefListIterator<T> RefIter<[ExplicitCopy, DynSized, Dealloc, TempAlloc] T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref TempRefList\<T\>\).RemoveAt\(int\)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<[ExplicitCopy, DynSized, TempAlloc] T>(this ref TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item.

> [!CAUTION]
> Possible exceptions: 
> - [IndexOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.IndexOutOfRangeException?view=netstandard-2.1) — if the index is out of bounds


### \(ref TempRefList\<byte\>\).AppendAsciiString\(string\)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref TempRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### \(ref TempRefList\<byte\>\).AppendUtf8String\(string\)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref TempRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
