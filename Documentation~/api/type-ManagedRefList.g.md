# ManagedRefList\<T\>

Dynamic array collection wrapping the Managed allocator.
Can store structures containing reference types, but it is not compatible with Burst.

```csharp
public struct ManagedRefList<T> where T : struct
```


## Static Creation Methods


### WithDefaultItems\<T\>(Int32)

Creates a list filled with `default` items.

```csharp
public static ManagedRefList<T> WithDefaultItems<T>(int count) where T : struct
```

Parameters
- `count` — number of items


### WithCapacity\<T\>(Int32)

Creates a list with a given capacity.

```csharp
public static ManagedRefList<T> WithCapacity<T>(int capacity) where T : struct
```

Parameters
- `capacity` — capacity of the list


### Empty\<T\>()

Creates an empty list

```csharp
public static ManagedRefList<T> Empty<T>() where T : struct
```


## Extension Methods


### AsSpan\<T\>(ManagedRefList)

Represent the collection as `Span`

```csharp
public static Span<T> AsSpan<T>([DynNoResize] this ref ManagedRefList<T> self) where T : struct
```


### AsReadOnlySpan\<T\>(ManagedRefList)

Represent the collection as `ReadOnlySpan`

```csharp
public static ReadOnlySpan<T> AsReadOnlySpan<T>(this in ManagedRefList<T> self) where T : struct
```


### CopyTo\<T\>(ManagedRefList,ManagedRefList)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in ManagedRefList<T> self, ref ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — source list
- `other` — destination list


### CopyFrom\<T\>(ManagedRefList,ManagedRefList)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref ManagedRefList<T> self, in ManagedRefList<T> other) where T : struct
```

Parameters
- `self` — destination list
- `other` — source list


### AppendDefault\<T\>(ManagedRefList,Int32)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<T>(this ref ManagedRefList<T> self, int count) where T : struct
```

Parameters
- `self` — list add items
- `count` — number of items to add


### Clear\<T\>(ManagedRefList)

Removes all items from the list.

```csharp
public static void Clear<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to clear


### RemoveAt\<T\>(ManagedRefList,Int32)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<T>(this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item


### RefAdd\<T\>(ManagedRefList)

Adds a `default` item to the list and returns a mutable reference to it.

```csharp
public static ref T RefAdd<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item


### Add\<T\>(ManagedRefList,T)

Adds a new item to the list.

```csharp
public static void Add<T>(this ref ManagedRefList<T> self, T item) where T : struct
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### RefAt\<T\>(ManagedRefList,Int32)

Returns a mutable reference to a list item.

```csharp
public static ref T RefAt<T>([DynNoResize] this ref ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index


### RefReadonlyAt\<T\>(ManagedRefList,Int32)

Returns a readonly reference to a list item.

```csharp
public static ref readonly T RefReadonlyAt<T>(this in ManagedRefList<T> self, int index) where T : struct
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index


### Count\<T\>(ManagedRefList)

Returns item count in the given list

```csharp
public static int Count<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get item count

Returns item count in the given list


### Capacity\<T\>(ManagedRefList)

Returns capacity of the given list.

```csharp
public static int Capacity<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to get capacity

Returns capacity of the given list (zero if the buffer is deallocated)


### RefIterReversed\<T\>(ManagedRefList)

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static ManagedRefListIteratorReversed<T> RefIterReversed<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefReadonlyIterReversed\<T\>(ManagedRefList)

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefIter\<T\>(ManagedRefList)

Creates a mutable by-ref iterator for the list.

```csharp
public static ManagedRefListIterator<T> RefIter<T>(this ref ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefReadonlyIter\<T\>(ManagedRefList)

Creates a readonly by-ref iterator for the list.

```csharp
public static ManagedRefListReadOnlyIterator<T> RefReadonlyIter<T>(this in ManagedRefList<T> self) where T : struct
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### ToStringAscii(ManagedRefList)

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### ToStringUtf8(ManagedRefList)

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in ManagedRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### AppendAsciiString(ManagedRefList,String)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### AppendUtf8String(ManagedRefList,String)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref ManagedRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
