# KsiHashTableSlotAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiHashTableSlotAttribute**
> \]

An attribute to mark a hash table slot type.
It is required to hint a code generator for [KsiHashTable](T.KsiHashTableAttribute.g.md) types.

Marked type should be a structure that defines these fields:
- `internal KsiHashTableEntryState State` — hash table [slot state](T.KsiHashTableSlotState.g.md)
- `public TKey Key` — field to store the item key, should be a value type
- (optional) `public TValue Value` — field to store the item value, should be a value type.

If you define the `Value` field, the structure will represent the `HashMap` slot.
Otherwise, it will be a `HashSet` slot.

```csharp
public class KsiHashTableSlotAttribute : Attribute
```
