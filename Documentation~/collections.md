# Collections

> \[ [Getting Started](getting-started.md)
> \| [Traits](traits.md)
> \| **Collections**
> \| [Referencing](borrow-checker-at-home.md)
> \| [ECS](ecs.md)
> \| [Serialization](serialization.md)
> \| [API](api/index.g.md)
> \]

There are three dynamic collections provided by the framework:
[RefList\<T\>](api/T.RefList-1.g.md), [TempRefList\<T\>](api/T.TempRefList-1.g.md)
and [ManagedRefList\<T\>](api/T.ManagedRefList-1.g.md).
All of them share the same public API but use different allocators
and inherit different sets of [trait attributes](traits.md).

| Collection          | Item Constraint | Allocator    | Trait Attributes                        | Burst |
|---------------------|-----------------|--------------|-----------------------------------------|-------|
| `RefList<T>`        | `unmanaged`     | `Persistent` | `ExplicitCopy`, `DynSized`, `Dealloc`   | Yes   |
| `TempRefList<T>`    | `unmanaged`     | `Temp`       | `ExplicitCopy`, `DynSized`, `TempAlloc` | Yes   |
| `ManagedRefList<T>` | `struct`        | `Managed`    | `ExplicitCopy`, `DynSized`              | No    |

As a general term, all these types are referred to as `TRefList<T>`.

The `TRefList<T>` types are the only [ExplicitCopy](api/T.ExplicitCopyAttribute.g.md) types that can be generic.

## Default Value Safety

All collections are safe to use in a `default` (zeroed) state.
It simply indicates that the collection is empty and its internal buffer is not allocated.

After [moving](api/T.KsiExtensions.g.md#ref-tmove) or [deallocating itself](api/T.RefList-1.g.md#ref-reflisttdealloc),
the collection returns to a zeroed state and can be reused.

## Data Access Control

The ѯ-Framework collections API uses extension methods to provide separate methods for _mutable_ and
_readonly_ data access.
This is possible because extension methods can be defined separately for `this ref` and `this in` parameters:

```csharp
// This method is available if you have ANY REFERENCE to the data
public static ref readonly T RefReadonlyAt<T>(this in RefList<T> self, int index)

// This method is available only if you have a MUTABLE REFERENCE to the data
public static ref T RefAt<T>(this ref RefList<T> self, int index)
```

The example above shows that you can get a mutable reference to a collection item
if you have a mutable reference to the collection itself.
However, you can get a readonly reference to an item even if you have a mutable reference to the collection.

In practice, this means:

> [!IMPORTANT]
> By composing your data with ѯ-Framework collections,
> you can control data access through reference mutability.

See the full list of extension methods in the API reference:
- [RefList\<T\>](api/T.RefList-1.g.md)
- [TempRefList\<T\>](api/T.TempRefList-1.g.md)
- [ManagedRefList\<T\>](api/T.ManagedRefList-1.g.md)

See usage examples in the [RefList\<T\>](../Tests/RefListTests.cs)
and [ManagedRefList\<T\>](../Tests/ManagedRefListTests.cs) unit tests.

## HashSet and HashMap

ѯ-Framework also provides a way to wrap a `TRefList<T>` into a struct that provides a `HashSet` or `HashMap` API.
This requires defining a data layout and using the [KsiHashTable](api/T.KsiHashTableAttribute.g.md)
and [KsiHashTableSlot](api/T.KsiHashTableSlotAttribute.g.md) attributes.

In short, you define the data structure, and the ѯ-Framework generates the extension-method-based API.

First, you need to declare your hash table slot type.
It should be a [KsiHashTableSlot](api/T.KsiHashTableSlotAttribute.g.md) structure with the following fields:
- `State` — a [KsiHashTableSlotState](api/T.KsiHashTableSlotState.g.md) field used internally by the collection code.
- `Key` — a value type field representing the key stored in the collection.
- `Value` (optional) — a value type field representing the value stored in the collection. This field is required for `HashMap` collections.

Mark your slot type with `[KsiHashTableSlot]`, and analyzers will hint at what is missing.
Example:
```csharp
[KsiHashTableSlot]
internal struct IntSetSlot
{
    internal KsiHashTableSlotState State;
    internal int Key;
}

[KsiHashTableSlot]
internal struct IntToIntMapSlot
{
    internal KsiHashTableSlotState State;
    internal int Key;
    internal int Value;
}
```

Next, you need to declare the collection itself.
It should be a top-level partial struct marked with [KsiHashTable](api/T.KsiHashTableAttribute.g.md) containing the following fields:
- `HashTable` — a `TRefList<TSlot>` field representing the hash table itself.
- `Count` — an `int` field that stores the number of items in the collection.

You also need to declare key hashing and equality comparison methods:
- `static int Hash(TKey key)` — a method to get a key's hash code.
- `static bool Eq(TKey l, TKey r)` — a method to compare two keys for equality.

For both methods, `TKey` can also be passed by readonly reference.

Similarly to the slot, mark the collection with `[KsiHashTable]`
and follow the analyzer hints. Example:
```csharp
[KsiHashTableSlot]
[ExplicitCopy, Dealloc, DynSized]
public struct IntSet
{
    internal RefList<IntSetSlot> HashTable;
    internal int Count;
    
    internal static int Hash(int key) => key;
    internal static bool Eq(int l, int r) => l == r;
}

[KsiHashTableSlot]
[ExplicitCopy, Dealloc, DynSized]
public struct IntToIntMap
{
    internal RefList<IntToIntMapSlot> HashTable;
    internal int Count;
    
    internal static int Hash(int key) => key;
    internal static bool Eq(int l, int r) => l == r;
}
```

If everything is properly declared, you will get the following API:

`HashSet` API:
- `THashSet.Empty { get; }` — returns an empty hash set instance
- `THashSet.WithMinCapacity(int capacity)` — returns a new hash set instance with a capacity equal to or greater than the given one.
- `(in THashSet).Count()` — returns the number of keys stored in the hash set.
- `(in THashSet).Capacity()` — returns the hash set capacity.
- `(in THashSet).Contains([in ]TKey key)` — determines if the hash set contains the given key.
- `(ref THashSet).Add([in ]TKey key)` — adds a new key to the hash set.
- `(ref THashSet).Remove([in ]TKey key)` — removes a key from the hash set.
- `(ref THashSet).Rebuild(int capacity)` — reallocates the hash set with the given minimal capacity.
- `(ref THashSet).Clear()` — clears the hash set.

`HashMap` API:
- `THashMap.Empty { get; }` — returns an empty hash map instance
- `THashMap.WithMinCapacity(int capacity)` — returns a new hash map instance with a capacity equal to or greater than the given one.
- `(in THashMap).Count()` — returns the number of keys stored in the hash map.
- `(in THashMap).Capacity()` — returns the hash map capacity.
- `(in THashMap).Contains([in ]TKey key, out int index)` — determines if the hash map contains the given key.
- `(in THashMap).RefReadonlyGet([in ]TKey key)` — gets a readonly value reference stored in the hash map.
- `(in THashMap).RefReadonlyGetByIndex(int index)` — gets a readonly value reference stored in the hash map.
- `(ref THashMap).RefGet([in ]TKey key)` — gets a mutable value reference stored in the hash map.
- `(ref THashMap).RefGetByIndex(int index)` — gets a mutable value reference stored in the hash map.
- `(ref THashMap).RefSet([in ]TKey key)` — optionally inserts a new key and returns a mutable reference to the associated value.
- `(ref THashMap).Remove([in ]TKey key)` — removes a key from the hash map.
- `(ref THashMap).Rebuild(int capacity)` — reallocates the hash map with the given minimal capacity.
- `(ref THashMap).Clear()` — clears the hash map.

See usage examples in the [HashSet](../Tests/KsiHashSetTests.cs) and [HashMap](../Tests/KsiHashMapTests.cs) unit tests.

### Encapsulation

It is generally unsafe to modify the internal state of the collection without using the provided API.
To make it impossible, you can declare the collection in a separate assembly
and declare its fields with the `internal` access modifier.

> [!NOTE]
> The same technique is used for `TRefList<T>` types.

### Iterators

ѯ-Framework doesn't provide any iterators for hash tables,
but you can declare your own extension methods to access keys at a given index.

In your extension methods, do not modify or provide mutable access to these fields:
- `THashMapOrSet.HashTable`
- `THashMapOrSet.Count`
- `TSlot.State`
- `TSlot.Key`

## Diagnostics

Diagnostics related to the usage of collections:

| Diagnostic Id  | Severity | Title                                        |
|----------------|----------|----------------------------------------------|
| `KSIGENERIC03` | Error    | Jagged `TRefList<T>` types are not supported |
| `KSIHASH01`    | Error    | Missing `KsiHash` symbol                     |
| `KSIHASH02`    | Error    | Invalid `KsiHash` field                      |
| `KSIHASH03`    | Error    | Invalid `KsiHash` symbol signature           |
| `KSIHASH04`    | Error    | Invalid `KsiHash` symbol accessibility       |
| `KSIHASH05`    | Error    | Invalid `KsiHashTable` declaration           |