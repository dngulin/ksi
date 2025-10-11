# RefList\<T\>

Dynamic array collection wrapping the Persistent allocator.
Requires manual deallocation.

```csharp
public struct RefList<T> where T : unmanaged
```


## Static Creation Methods


### WithDefaultItems\<T\>(Int32)

Creates a list filled with `default` items.

```csharp
public static RefList<T> WithDefaultItems<T>(int count) where T : unmanaged
```

Parameters
- `count` — number of items


### WithCapacity\<T\>(Int32)

Creates a list with a given capacity.

```csharp
public static RefList<T> WithCapacity<T>(int capacity) where T : unmanaged
```

Parameters
- `capacity` — capacity of the list


### Empty\<T\>()

Creates an empty list

```csharp
public static RefList<T> Empty<T>() where T : unmanaged
```


## Extension Methods


### AsSpan\<T\>(RefList)

Represent the collection as `Span`

```csharp
public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref RefList<T> self) where T : unmanaged
```


### AsReadOnlySpan\<T\>(RefList)

Represent the collection as `ReadOnlySpan`

```csharp
public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in RefList<T> self) where T : unmanaged
```


### CopyTo\<T\>(RefList,RefList)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in RefList<T> self, ref RefList<T> other) where T : unmanaged
```

Parameters
- `self` — source list
- `other` — destination list


### CopyFrom\<T\>(RefList,RefList)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref RefList<T> self, in RefList<T> other) where T : unmanaged
```

Parameters
- `self` — destination list
- `other` — source list


### AppendDefault\<T\>(RefList,Int32)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<T>(this ref RefList<T> self, int count) where T : unmanaged
```

Parameters
- `self` — list add items
- `count` — number of items to add


### Clear\<T\>(RefList)

Removes all items from the list.

```csharp
public static void Clear<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to clear


### RemoveAt\<T\>(RefList,Int32)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<T>(this ref RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item


### RefAdd\<T\>(RefList)

Adds a `default` item to the list and returns a mutable reference to it.

```csharp
public static ref T RefAdd<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item


### Add\<T\>(RefList,T)

Adds a new item to the list.

```csharp
public static void Add<T>(this ref RefList<T> self, T item) where T : unmanaged
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### RefAt\<T\>(RefList,Int32)

Returns a mutable reference to a list item.

```csharp
public static ref T RefAt<T>([DynNoResize] this ref RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index


### RefReadonlyAt\<T\>(RefList,Int32)

Returns a readonly reference to a list item.

```csharp
public static ref readonly T RefReadonlyAt<T>(this in RefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index


### Count\<T\>(RefList)

Returns item count in the given list

```csharp
public static int Count<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get item count

Returns item count in the given list


### Capacity\<T\>(RefList)

Returns capacity of the given list.

```csharp
public static int Capacity<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get capacity

Returns capacity of the given list (zero if the buffer is deallocated)


### RefIterReversed\<T\>(RefList)

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static RefListIteratorReversed<T> RefIterReversed<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefReadonlyIterReversed\<T\>(RefList)

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static RefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefIter\<T\>(RefList)

Creates a mutable by-ref iterator for the list.

```csharp
public static RefListIterator<T> RefIter<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### RefReadonlyIter\<T\>(RefList)

Creates a readonly by-ref iterator for the list.

```csharp
public static RefListReadOnlyIterator<T> RefReadonlyIter<T>(this in RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### ToStringAscii(RefList)

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in RefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### ToStringUtf8(RefList)

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in RefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### AppendAsciiString(RefList,String)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref RefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### AppendUtf8String(RefList,String)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref RefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### Deallocated\<T\>(RefList)

Deallocate the list and returns it.

```csharp
public static ref RefList<T> Deallocated<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to deallocate

Returns the list as an assignable reference


### Dealloc\<T\>(RefList)

Deallocate the list.
After deallocating the structure becomes zeroed.

```csharp
public static void Dealloc<T>(this ref RefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to deallocate
