# KsiEntityAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiEntityAttribute**
> \]

An attribute to mark an entity type (set of the [KsiComponent](T.KsiComponentAttribute.g.md) types)
that should be stored in the `TRefList<T>`
within the [KsiDomain](T.KsiDomainAttribute.g.md) structure.
Use it if you need the `Array of Structures` data layout.

Requirements: 
- All field types should be marked with the [KsiComponentAttribute](T.KsiComponentAttribute.g.md)
- All field types should be unique

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiEntityAttribute : Attribute
```
