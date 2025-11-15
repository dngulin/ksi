# KsiArchetypeAttribute

> \[ [Getting Started](getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiArchetypeAttribute**
> \]

An attribute to mark a type that represents a sequence of entities.
Can be stored in the [KsiDomain](T.KsiDomainAttribute.g.md) structure
to provide the `Structure of Arrays` data layout.

Requirements:
- All field types should be `TRefList<TComponent>` types
with the [KsiComponent](T.KsiComponentAttribute.g.md) item type
- All field types should be unique

Triggers extension methods code generation to keep all inner lists with the same length:
- `(in TArchetype).Count()` — gets entity count
- `(ref TArchetype).AppendDefault(int count)` — adds a specified number of `default` components to each inner list
- `(ref TArchetype).RemoveAt(int index)` — removes an entity at the given index
- `(ref TArchetype).Clear()` — clears all inner lists

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiArchetypeAttribute : Attribute
```
