# TempRefList\<T\>

Dynamic array collection wrapping the Temp allocator.
Can be stored only on stack.

```csharp
public struct TempRefList<T> where T : unmanaged
```


## Static Creation Methods


### TempRefList.Empty\<T\>()

Creates an empty list

```csharp
public static TempRefList<T> Empty<T>() where T : unmanaged
```


### TempRefList.WithCapacity\<T\>(int)

Creates a list with a given capacity.

```csharp
public static TempRefList<T> WithCapacity<T>(int capacity) where T : unmanaged
```

Parameters
- `capacity` — capacity of the list


### TempRefList.WithDefaultItems\<T\>(int)

Creates a list filled with `default` items.

```csharp
public static TempRefList<T> WithDefaultItems<T>(int count) where T : unmanaged
```

Parameters
- `count` — number of items


## Extension Methods


### (in TempRefList\<T\>).AsReadOnlySpan()

Represent the collection as `ReadOnlySpan`

```csharp
public static unsafe ReadOnlySpan<T> AsReadOnlySpan<T>(this in TempRefList<T> self) where T : unmanaged
```


### (in TempRefList\<T\>).Capacity()

Returns capacity of the given list.

```csharp
public static int Capacity<T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get capacity

Returns capacity of the given list (zero if the buffer is deallocated)


### (in TempRefList\<T\>).CopyTo(ref TempRefList\<T\>)

Copies all items to another list.
All items existing before copying are removed.

```csharp
public static void CopyTo<T>(this in TempRefList<T> self, ref TempRefList<T> other) where T : unmanaged
```

Parameters
- `self` — source list
- `other` — destination list


### (in TempRefList\<T\>).Count()

Returns item count in the given list

```csharp
public static int Count<T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to get item count

Returns item count in the given list


### (in TempRefList\<T\>).RefReadonlyAt(int)

Returns a readonly reference to a list item.

```csharp
public static ref readonly T RefReadonlyAt<T>(this in TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a readonly reference to a list item at the given index


### (in TempRefList\<T\>).RefReadonlyIter()

Creates a readonly by-ref iterator for the list.

```csharp
public static TempRefListReadOnlyIterator<T> RefReadonlyIter<T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (in TempRefList\<T\>).RefReadonlyIterReversed()

Creates a readonly reversed by-ref iterator for the list.

```csharp
public static TempRefListReadOnlyIteratorReversed<T> RefReadonlyIterReversed<T>(this in TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (in TempRefList\<byte\>).ToStringAscii()

Creates a string interpreting list contents as ASCII bytes.

```csharp
public static string ToStringAscii(this in TempRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### (in TempRefList\<byte\>).ToStringUtf8()

Creates a string interpreting list contents as UTF-8 bytes.

```csharp
public static string ToStringUtf8(this in TempRefList<byte> self)
```

Parameters
- `self` — list containing string bytes

Returns the string created from bytes


### (ref TempRefList\<T\>).Add(T)

Adds a new item to the list.

```csharp
public static void Add<T>(this ref TempRefList<T> self, T item) where T : unmanaged
```

Parameters
- `self` — list to add an item
- `item` — item to add to the list


### (ref TempRefList\<T\>).AppendDefault(int)

Adds a specified number of `default` items.

```csharp
public static void AppendDefault<T>(this ref TempRefList<T> self, int count) where T : unmanaged
```

Parameters
- `self` — list add items
- `count` — number of items to add


### (ref TempRefList\<T\>).AsSpan()

Represent the collection as `Span`

```csharp
public static unsafe Span<T> AsSpan<T>([DynNoResize] this ref TempRefList<T> self) where T : unmanaged
```


### (ref TempRefList\<T\>).Clear()

Removes all items from the list.

```csharp
public static void Clear<T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to clear


### (ref TempRefList\<T\>).CopyFrom(in TempRefList\<T\>)

Copies all items from another list.
All items existing before copying are removed.

```csharp
public static void CopyFrom<T>(this ref TempRefList<T> self, in TempRefList<T> other) where T : unmanaged
```

Parameters
- `self` — destination list
- `other` — source list


### (ref TempRefList\<T\>).RefAdd()

Adds a `default` item to the list and returns a mutable reference to it.

```csharp
public static ref T RefAdd<T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to add an item

Returns a mutable reference to the created item


### (ref TempRefList\<T\>).RefAt(int)

Returns a mutable reference to a list item.

```csharp
public static ref T RefAt<T>([DynNoResize] this ref TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to get an item reference
- `index` — required item index

Returns a mutable reference to a list item at the given index


### (ref TempRefList\<T\>).RefIter()

Creates a mutable by-ref iterator for the list.

```csharp
public static TempRefListIterator<T> RefIter<T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (ref TempRefList\<T\>).RefIterReversed()

Creates a mutable reversed by-ref iterator for the list.

```csharp
public static TempRefListIteratorReversed<T> RefIterReversed<T>(this ref TempRefList<T> self) where T : unmanaged
```

Parameters
- `self` — list to iterate

Returns the iterator to use in the foreach loop


### (ref TempRefList\<T\>).RemoveAt(int)

Removes an item from the list at the given index.

```csharp
public static void RemoveAt<T>(this ref TempRefList<T> self, int index) where T : unmanaged
```

Parameters
- `self` — list to remove the item
- `index` — an index to remove the item


### (ref TempRefList\<byte\>).AppendAsciiString(string)

Appends a given string to the list as ASCII bytes.

```csharp
public static void AppendAsciiString(this ref TempRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append


### (ref TempRefList\<byte\>).AppendUtf8String(string)

Appends a given string to the list as UTF-8 bytes.

```csharp
public static void AppendUtf8String(this ref TempRefList<byte> self, string value)
```

Parameters
- `self` — list to append bytes
- `value` — string value to append
