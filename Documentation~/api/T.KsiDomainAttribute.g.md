# KsiDomainAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiDomainAttribute**
> \]

An attribute to mark a domain that can be extended with [KsiQuery](T.KsiQueryAttribute.g.md) methods.

Should be a `partial struct` that has fields only of these types: 
- `RefList` of the [KsiEntity](T.KsiEntityAttribute.g.md) type
for the `Array of Structures` data layout
- [KsiArchetype](T.KsiArchetypeAttribute.g.md) type for the `Structure of Arrays` data layout

Triggers code generation to produce
the `{DomainTypeName}.KsiSection` enum and the `{DomainTypeName}.KsiHandle` structure: 
- `{DomainTypeName}.KsiSection` — an enum that represent each field within the domain
- `{DomainTypeName}.KsiHandle` — a structure to represent a [KsiQuery](T.KsiQueryAttribute.g.md) address.
Is composed of the `{DomainTypeName}.KsiSection` and the entity index (`int`) in the section.
Should be used as the first argument for [KsiQuery](T.KsiQueryAttribute.g.md) methods

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiDomainAttribute : Attribute
```
