# KsiQueryAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / KsiQueryAttribute**
> \]

An attribute to produce the `ECS`-like query.
It creates a code-generated extension method for the [KsiDomain](T.KsiDomainAttribute.g.md)
structure that calls the marked method for each matching entity in the domain.

Method signature requirements: 
- Should be a non-generic `static` method
- All parameters should be by-ref parameters to named structures
- The first argument should be a readonly reference to
a [KsiDomain](T.KsiDomainAttribute.g.md)`Handle`
- The following parameters should be references to [KsiComponent](T.KsiComponentAttribute.g.md) types.
At least one argument of that kind should be present
- Optionally, after that you can declare parameters marked with [KsiQueryParamAttribute](T.KsiQueryParamAttribute.g.md)
that are passed through from the generated extension method to the marked query method.
Only the [DynNoResizeAttribute](T.DynNoResizeAttribute.g.md) is inherited for these parameters

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class KsiQueryAttribute : Attribute
```
