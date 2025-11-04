# KsiHashTableAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiHashTableAttribute**
> \]

An attribute to mark a hashtable-based collection.
It can be either `HashSet` or `HashMap` with API provided by Roslyn code generator.
The generated implementation is based on the open addressing hash table with linear single-step probing
and lazy deletion.

Marked type should be a structure that defines:
- `internal TRefList<TSlot> HashTable` — inner hash table,
where `TSlot` should be marked with [KsiHashTableSlotAttribute](T.KsiHashTableSlotAttribute.g.md).
Kind of the slot defines the collection kind (`HashSet` or `HashMap`)
- `internal int Count` — count of occupied slots in the hash table
- `internal int HashCode([in ]TKey key)` — computes hash code for the key defined in the `TSlot`
- `internal int AreEqual([in ]TKey l, [in ]TKey r)` — checks keys equality

You can receive `HashCode` and `AreEqual` parameters both by `in` and by value
(except [ExplicitCopy](T.ExplicitCopyAttribute.g.md) keys, that should be passed only by `in`).

Parameter reference kind used in the `HashCode` method is inherited by generated API.
But for [ExplicitCopy](T.ExplicitCopyAttribute.g.md) keys in insertion methods
it is always a `by value` parameter to enforce moving the key into the collection.

It is recommended to define hash tables in a separate assembly to make their internal state unavailable.
Use only the generated API to modify the collection state.

`HashSet` API:
- `THashSet.Empty { get; }` — returns an empty hash set instance
- `HashSet.WithMinCapacity(int capacity)` — returns a new hash set instance with a capacity equal or greater of the given one
- `(in THashSet).Count()` — returns the number of keys stored in the hash set
- `(in THashSet).Capacity()` — returns the hash set capacity
- `(in THashSet).Contains([in ]TKey key)` — determines if the hash set contains a given key
- `(ref THashSet).Add([in ]TKey key)` — adds a new key to the hash set
- `(ref THashSet).Remove([in ]TKey key)` — removes a key from the hash set
- `(ref THashSet).Rebuild(int capacity)` — reallocates the hash set with a given minimal capacity
- `(ref THashSet).Clear()` — clears the hash set

`HashMap` API:
- `THashMap.Empty { get; }` — returns an empty map instance
- `THashMap.WithMinCapacity(int capacity)` — returns a new map instance with a capacity equal or greater of the given one
- `(in THashMap).Count()` — returns number of keys
- `(in THashMap).Capacity()` — returns the hash table size
- `(in THashMap).Contains([in ]TKey key, out int index)` — checks if the key exists in the hash table
- `(in THashMap).RefReadonlyGet([in ]TKey key)` — returns a readonly `TValue` reference
- `(in THashMap).RefReadonlyGetByIndex(int index)` — returns a readonly `TValue` reference
- `(ref THashMap).RefGet([in ]TKey key)` — returns a mutable `TValue` reference
- `(ref THashMap).RefGetByIndex(int index)` — returns a mutable `TValue` reference
- `(ref THashMap).RefSet([in ]TKey key)` — finds an entry or creates a new one and returns a mutable `TValue` reference
- `(ref THashMap).Remove([in ]TKey key)` — removes a key and returns a success flag
- `(ref THashSet).Rebuild(int capacity)` — reallocates the hash table with a given size
- `(ref THashSet).Clear()` — clears the hash table

```csharp
public class KsiHashTableAttribute : Attribute
```
