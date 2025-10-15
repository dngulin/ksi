# KsiArchetypeAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / KsiArchetypeAttribute**
> \]

An attribute to mark a type that represents a list of entities
(set of the [KsiComponent](T.KsiComponentAttribute.g.md) types)
within the [KsiDomain](T.KsiDomainAttribute.g.md) structure.
Use it if you need the `Structure of Arrays` data layout.

Requirements: 
- All field types should be `RefList` collections
with the [KsiComponent](T.KsiComponentAttribute.g.md) item type
- All field types should be unique

Triggers extension methods code generation to keep all inner lists with the same length: 
- `(in T).Count()` — get entity count
- `(ref T).RemoveAt(int)` — remove an entity at the given index
- `(ref T).Add()` — add a new `default` value to each inner list
- `(ref T).Clear()` — clear all inner lists

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiArchetypeAttribute : Attribute
```
