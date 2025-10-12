# ManagedRefList\<T\>

Dynamic array collection wrapping the Managed allocator.
Can store structures containing reference types, but it is not compatible with Burst.

```csharp
public struct ManagedRefList<T> where T : struct
```

Static Creation Methods
- [ManagedRefList.Empty\<T\>()](#managedreflistemptyt) — Creates an empty list
- [ManagedRefList.WithCapacity\<T\>(int)](#managedreflistwithcapacitytint) — Creates a list with a given capacity
- [ManagedRefList.WithDefaultItems\<T\>(int)](#managedreflistwithdefaultitemstint) — Creates a list filled with `default` items

Extension Methods
- [(in ManagedRefList\<T\>).AsReadOnlySpan()](#in-managedreflisttasreadonlyspan) — Represent the collection as `ReadOnlySpan`
- [(in ManagedRefList\<T\>).Capacity()](#in-managedreflisttcapacity) — Returns capacity of the given list
- [(in ManagedRefList\<T\>).CopyTo(ref ManagedRefList\<T\>)](#in-managedreflisttcopytoref-managedreflistt) — Copies all items to another list
- [(in ManagedRefList\<T\>).Count()](#in-managedreflisttcount) — Returns item count in the given list
- [(in ManagedRefList\<T\>).RefReadonlyAt(int)](#in-managedreflisttrefreadonlyatint) — Returns a readonly reference to a list item
- [(in ManagedRefList\<T\>).RefReadonlyIter()](#in-managedreflisttrefreadonlyiter) — Creates a readonly by-ref iterator for the list
- [(in ManagedRefList\<T\>).RefReadonlyIterReversed()](#in-managedreflisttrefreadonlyiterreversed) — Creates a readonly reversed by-ref iterator for the list
- [(in ManagedRefList\<byte\>).ToStringAscii()](#in-managedreflistbytetostringascii) — Creates a string interpreting list contents as ASCII bytes
- [(in ManagedRefList\<byte\>).ToStringUtf8()](#in-managedreflistbytetostringutf8) — Creates a string interpreting list contents as UTF-8 bytes
- [(ref ManagedRefList\<T\>).Add(T)](#ref-managedreflisttaddt) — Adds a new item to the list
- [(ref ManagedRefList\<T\>).AppendDefault(int)](#ref-managedreflisttappenddefaultint) — Adds a specified number of `default` items
- [(ref ManagedRefList\<T\>).AsSpan()](#ref-managedreflisttasspan) — Represent the collection as `Span`
- [(ref ManagedRefList\<T\>).Clear()](#ref-managedreflisttclear) — Removes all items from the list
- [(ref ManagedRefList\<T\>).CopyFrom(in ManagedRefList\<T\>)](#ref-managedreflisttcopyfromin-managedreflistt) — Copies all items from another list
- [(ref ManagedRefList\<T\>).RefAdd()](#ref-managedreflisttrefadd) — Adds a `default` item to the list and returns a mutable reference to it
- [(ref ManagedRefList\<T\>).RefAt(int)](#ref-managedreflisttrefatint) — Returns a mutable reference to a list item
- [(ref ManagedRefList\<T\>).RefIter()](#ref-managedreflisttrefiter) — Creates a mutable by-ref iterator for the list
- [(ref ManagedRefList\<T\>).RefIterReversed()](#ref-managedreflisttrefiterreversed) — Creates a mutable reversed by-ref iterator for the list
- [(ref ManagedRefList\<T\>).RemoveAt(int)](#ref-managedreflisttremoveatint) — Removes an item from the list at the given index
- [(ref ManagedRefList\<byte\>).AppendAsciiString(string)](#ref-managedreflistbyteappendasciistringstring) — Appends a given string to the list as ASCII bytes
- [(ref ManagedRefList\<byte\>).AppendUtf8String(string)](#ref-managedreflistbyteappendutf8stringstring) — Appends a given string to the list as UTF-8 bytes


## Static Creation Methods


### ManagedRefList.Empty\<T\>()

Creates an empty list

```csharp
public static ManagedRefList<T> Empty<T>() where T : struct
```


### ManagedRefList.WithCapacity\<T\>(int)

Creates a list with a given capacity.

```csharp
public static ManagedRefList<T> WithCapacity<T>(int capacity) where T : struct
```

Parameters
- `capacity` — capacity of the list


### ManagedRefList.WithDefaultItems\<T\>(int)

Creates a list filled with `default` items.

```csharp
public static ManagedRefList<T> WithDefaultItems<T>(int count) where T : struct
```

Parameters
- `count` — number of items


## Extension Methods


### (in ManagedRefList\<T\>).AsReadOnlySpan()

Represent the collection as `ReadOnlySpan`

```csharp
public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in ManagedRefList<T> self) where T : struct
```


### (in ManagedRefList\<T\>).Capacity()

Returns capacity of the given list.

```csharp
public static int Capacity<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get capacity

Returns capacity of the given list (zero if the buffer is deallocated)


### (in ManagedRefList\<T\>).CopyTo(ref ManagedRefList\<T\>)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in ManagedRefList<T> self, ref ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — source list
- `other` — destination list


### (in ManagedRefList\<T\>).Count()

Returns item count in the given list

```csharp
public static int Count<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get item count

Returns item count in the given list


### (in ManagedRefList\<T\>).RefReadonlyAt(int)

Returns a readonly reference to a list item.

```csharp
public static ref readonly T RefReadonlyAt<T>(this in ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index


### (in ManagedRefList\<T\>).RefReadonlyIter()

Creates a readonly by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIterator<T> RefReadonlyIter<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (in ManagedRefList\<T\>).RefReadonlyIterReversed()

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (in ManagedRefList\<byte\>).ToStringAscii()

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### (in ManagedRefList\<byte\>).ToStringUtf8()

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### (ref ManagedRefList\<T\>).Add(T)

Adds a new item to the list.

```csharp
public static void Add<T>(this ref ManagedRefList<T> self, T item) where T : struct
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### (ref ManagedRefList\<T\>).AppendDefault(int)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<T>(this ref ManagedRefList<T> self, int count) where T : struct
```

Parameters
- `self` — list add items
- `count` — number of items to add


### (ref ManagedRefList\<T\>).AsSpan()

Represent the collection as `Span`

```csharp
public static Span<T> AsSpan<T>([DynNoResize] this ref ManagedRefList<T> self) where T : struct
```


### (ref ManagedRefList\<T\>).Clear()

Removes all items from the list.

```csharp
public static void Clear<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to clear


### (ref ManagedRefList\<T\>).CopyFrom(in ManagedRefList\<T\>)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref ManagedRefList<T> self, in ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — destination list
- `other` — source list


### (ref ManagedRefList\<T\>).RefAdd()

Adds a `default` item to the list and returns a mutable reference to it.

```csharp
public static ref T RefAdd<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item


### (ref ManagedRefList\<T\>).RefAt(int)

Returns a mutable reference to a list item.

```csharp
public static ref T RefAt<T>([DynNoResize] this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index


### (ref ManagedRefList\<T\>).RefIter()

Creates a mutable by-ref iterator for the list.

```csharp
public static ManagedRefListIterator<T> RefIter<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (ref ManagedRefList\<T\>).RefIterReversed()

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static ManagedRefListIteratorReversed<T> RefIterReversed<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (ref ManagedRefList\<T\>).RemoveAt(int)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<T>(this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item


### (ref ManagedRefList\<byte\>).AppendAsciiString(string)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### (ref ManagedRefList\<byte\>).AppendUtf8String(string)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
