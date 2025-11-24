# KsiHashTableSlotState

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiHashTableSlotState**
> \]

An enum indicating a slot state for open addressing hash tables with lazy deletion.

```csharp
public enum KsiHashTableSlotState 
```

Fields
- [Empty](#empty) — slot doesn't store a value
- [Occupied](#occupied) — slot stores a value
- [Deleted](#deleted) — slot doesn't store a value because it was deleted


## Fields


### Empty

Slot doesn't store a value.

Value is `0`.

```csharp
Empty

```


### Occupied

Slot stores a value.

Value is `1`.

```csharp
Occupied

```


### Deleted

Slot doesn't store a value because it was deleted.
Slots with that state are treated as empty ones during insertions and as occupied ones during lookups.

Only trailing deleted slot sequences are set to empty after item deletion.

Value is `2`.

```csharp
Deleted

```
