# KsiComponentAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiComponentAttribute**
> \]

An attribute to mark a component type.
It can be queried with the [KsiQuery](T.KsiQueryAttribute.g.md)
from the [KsiDomain](T.KsiDomainAttribute.g.md).

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiComponentAttribute : Attribute
```
