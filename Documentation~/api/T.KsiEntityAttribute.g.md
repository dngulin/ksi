# KsiEntityAttribute

> \[ [Getting Started](getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiEntityAttribute**
> \]

An attribute to mark an entity type.
It should be a set of the [KsiComponent](T.KsiComponentAttribute.g.md) types.
Can be stored in the [KsiDomain](T.KsiDomainAttribute.g.md) as the `TRefList<TEntity>`
to provide the `Array of Structures` data layout.

Requirements:
- All field types should be marked with the [KsiComponentAttribute](T.KsiComponentAttribute.g.md)
- All field types should be unique

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiEntityAttribute : Attribute
```
