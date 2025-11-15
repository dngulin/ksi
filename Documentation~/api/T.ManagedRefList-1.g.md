# ManagedRefList\<T\>

> \[ [Getting Started](getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / ManagedRefList\<T\>**
> \]

A dynamic array collection wrapping a managed array.
Can store structures containing reference types, but it is not compatible with `Burst`.

```csharp
[ExplicitCopy, DynSized]
public struct ManagedRefList<[ExplicitCopy, DynSized, Dealloc] T> where T : struct
```

Static Creation Methods
- [ManagedRefList.Empty\<T\>\(\)](#managedreflistemptyt) — creates an empty list
- [ManagedRefList.WithCapacity\<T\>\(int\)](#managedreflistwithcapacitytint) — creates a list with a given capacity
- [ManagedRefList.WithDefaultItems\<T\>\(int\)](#managedreflistwithdefaultitemstint) — creates a list filled with `default` items

Extension Methods
- [\(in ManagedRefList\<T\>\).AsReadOnlySpan\(\)](#in-managedreflisttasreadonlyspan) — wraps the collection with [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1)
- [\(in ManagedRefList\<T\>\).Capacity\(\)](#in-managedreflisttcapacity) — returns capacity of the list
- [\(in ManagedRefList\<T\>\).CopyTo\(ref ManagedRefList\<T\>\)](#in-managedreflisttcopytoref-managedreflistt) — copies all items to another list
- [\(in ManagedRefList\<T\>\).Count\(\)](#in-managedreflisttcount) — returns item count in the list
- [\(in ManagedRefList\<T\>\).RefReadonlyAt\(int\)](#in-managedreflisttrefreadonlyatint) — returns a readonly reference to a list item
- [\(in ManagedRefList\<T\>\).RefReadonlyIterReversed\(\)](#in-managedreflisttrefreadonlyiterreversed) — creates a readonly reversed by-ref iterator for the list
- [\(in ManagedRefList\<T\>\).RefReadonlyIter\(\)](#in-managedreflisttrefreadonlyiter) — creates a readonly by-ref iterator for the list
- [\(in ManagedRefList\<byte\>\).ToStringAscii\(\)](#in-managedreflistbytetostringascii) — creates a string interpreting list contents as ASCII bytes
- [\(in ManagedRefList\<byte\>\).ToStringUtf8\(\)](#in-managedreflistbytetostringutf8) — creates a string interpreting list contents as UTF-8 bytes
- [\(ref ManagedRefList\<T\>\).Add\(T\)](#ref-managedreflisttaddt) — adds a new item to the list
- [\(ref ManagedRefList\<T\>\).AppendDefault\(int\)](#ref-managedreflisttappenddefaultint) — adds a specified number of `default` items
- [\(ref ManagedRefList\<T\>\).AsSpan\(\)](#ref-managedreflisttasspan) — wraps the collection with [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1)
- [\(ref ManagedRefList\<T\>\).Clear\(\)](#ref-managedreflisttclear) — removes all items from the list
- [\(ref ManagedRefList\<T\>\).CopyFrom\(in ManagedRefList\<T\>\)](#ref-managedreflisttcopyfromin-managedreflistt) — copies all items from another list
- [\(ref ManagedRefList\<T\>\).RefAdd\(\)](#ref-managedreflisttrefadd) — adds a `default` item to the list and returns a mutable reference to it
- [\(ref ManagedRefList\<T\>\).RefAt\(int\)](#ref-managedreflisttrefatint) — returns a mutable reference to a list item
- [\(ref ManagedRefList\<T\>\).RefIterReversed\(\)](#ref-managedreflisttrefiterreversed) — creates a mutable reversed by-ref iterator for the list
- [\(ref ManagedRefList\<T\>\).RefIter\(\)](#ref-managedreflisttrefiter) — creates a mutable by-ref iterator for the list
- [\(ref ManagedRefList\<T\>\).RemoveAt\(int\)](#ref-managedreflisttremoveatint) — removes an item from the list at the given index
- [\(ref ManagedRefList\<byte\>\).AppendAsciiString\(string\)](#ref-managedreflistbyteappendasciistringstring) — appends a given string to the list as ASCII bytes
- [\(ref ManagedRefList\<byte\>\).AppendUtf8String\(string\)](#ref-managedreflistbyteappendutf8stringstring) — appends a given string to the list as UTF-8 bytes


## Static Creation Methods


### ManagedRefList.Empty\<T\>\(\)

Creates an empty list

```csharp
public static ManagedRefList<T> Empty<T>() where T : struct
```

Returns a new empty instance of the [ManagedRefList\<T\>](T.ManagedRefList-1.g.md).


### ManagedRefList.WithCapacity\<T\>\(int\)

Creates a list with a given capacity.

```csharp
public static ManagedRefList<T> WithCapacity<T>(int capacity) where T : struct
```

Parameters
- `capacity` — capacity of the list

Returns a new instance of the [ManagedRefList\<T\>](T.ManagedRefList-1.g.md) with the given capacity.


### ManagedRefList.WithDefaultItems\<T\>\(int\)

Creates a list filled with `default` items.

```csharp
public static ManagedRefList<T> WithDefaultItems<T>(int count) where T : struct
```

Parameters
- `count` — number of items

Returns a new instance of the [ManagedRefList\<T\>](T.ManagedRefList-1.g.md) with the given number of `default` items.


## Extension Methods


### \(in ManagedRefList\<T\>\).AsReadOnlySpan\(\)

Wraps the collection with [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1).

Adds to `RefPath` a non-explicit segment `AsReadOnlySpan()`.

```csharp
public static ReadOnlySpan<T> AsReadOnlySpan<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self) where T : struct
```

Returns a [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1) wrapping the list.


### \(in ManagedRefList\<T\>\).Capacity\(\)

Returns capacity of the list.

```csharp
public static int Capacity<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get capacity

Returns capacity of the list (zero if the buffer is deallocated).


### \(in ManagedRefList\<T\>\).CopyTo\(ref ManagedRefList\<T\>\)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in ManagedRefList<T> self, ref ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — source list
- `other` — destination list


### \(in ManagedRefList\<T\>\).Count\(\)

Returns item count in the list.

```csharp
public static int Count<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get item count

Returns item count in the list.


### \(in ManagedRefList\<T\>\).RefReadonlyAt\(int\)

Returns a readonly reference to a list item.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
public static ref readonly T RefReadonlyAt<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index.

> [!CAUTION]
> Possible exceptions: 
> - [IndexOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.IndexOutOfRangeException?view=netstandard-2.1) — if the index is out of bounds


### \(in ManagedRefList\<T\>\).RefReadonlyIterReversed\(\)

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in ManagedRefList\<T\>\).RefReadonlyIter\(\)

Creates a readonly by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIterator<T> RefReadonlyIter<[ExplicitCopy, DynSized, Dealloc] T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(in ManagedRefList\<byte\>\).ToStringAscii\(\)

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(in ManagedRefList\<byte\>\).ToStringUtf8\(\)

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes.


### \(ref ManagedRefList\<T\>\).Add\(T\)

Adds a new item to the list.

```csharp
public static void Add<[ExplicitCopy, DynSized, Dealloc] T>(this ref ManagedRefList<T> self, T item) where T : struct
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### \(ref ManagedRefList\<T\>\).AppendDefault\(int\)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<[ExplicitCopy, DynSized, Dealloc] T>(this ref ManagedRefList<T> self, int count) where T : struct
```

Parameters
- `self` — list add items
- `count` — number of items to add

> [!CAUTION]
> Possible exceptions: 
> - [ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/System.ArgumentException?view=netstandard-2.1) — if the count is negative


### \(ref ManagedRefList\<T\>\).AsSpan\(\)

Wraps the collection with [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1).

Adds to `RefPath` a non-explicit segment `AsSpan()`.

```csharp
public static Span<T> AsSpan<[ExplicitCopy, DynSized, Dealloc] T>([DynNoResize] this ref ManagedRefList<T> self) where T : struct
```

Returns a [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1) wrapping the list.


### \(ref ManagedRefList\<T\>\).Clear\(\)

Removes all items from the list.

```csharp
public static void Clear<[ExplicitCopy, DynSized] T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to clear


### \(ref ManagedRefList\<T\>\).CopyFrom\(in ManagedRefList\<T\>\)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref ManagedRefList<T> self, in ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — destination list
- `other` — source list


### \(ref ManagedRefList\<T\>\).RefAdd\(\)

Adds a `default` item to the list and returns a mutable reference to it.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
[NonAllocatedResult]
public static ref T RefAdd<[ExplicitCopy, DynSized, Dealloc] T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item.


### \(ref ManagedRefList\<T\>\).RefAt\(int\)

Returns a mutable reference to a list item.

Adds to `RefPath` an indexer segment `[n]`.

```csharp
public static ref T RefAt<[ExplicitCopy, DynSized, Dealloc] T>([DynNoResize] this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index.


### \(ref ManagedRefList\<T\>\).RefIterReversed\(\)

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static ManagedRefListIteratorReversed<T> RefIterReversed<[ExplicitCopy, DynSized, Dealloc] T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref ManagedRefList\<T\>\).RefIter\(\)

Creates a mutable by-ref iterator for the list.

```csharp
public static ManagedRefListIterator<T> RefIter<[ExplicitCopy, DynSized, Dealloc] T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop.


### \(ref ManagedRefList\<T\>\).RemoveAt\(int\)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<[ExplicitCopy, DynSized] T>(this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item.

> [!CAUTION]
> Possible exceptions: 
> - [IndexOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.IndexOutOfRangeException?view=netstandard-2.1) — if the index is out of bounds


### \(ref ManagedRefList\<byte\>\).AppendAsciiString\(string\)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### \(ref ManagedRefList\<byte\>\).AppendUtf8String\(string\)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
