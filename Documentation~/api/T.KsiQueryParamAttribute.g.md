# KsiQueryParamAttribute

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiQueryParamAttribute**
> \]

An attribute to mark a [KsiQuery](T.KsiQueryAttribute.g.md) parameter.
It is passed through from the generated method to the marked one.

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class KsiQueryParamAttribute : Attribute
```
