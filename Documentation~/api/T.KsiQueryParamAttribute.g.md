# KsiQueryParamAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiQueryParamAttribute**
> \]

An attribute to mark a [KsiQuery](T.KsiQueryAttribute.g.md) parameter that
is passed through from the generated extension method to the marked query method

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class KsiQueryParamAttribute : Attribute
```
