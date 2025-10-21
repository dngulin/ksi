# KsiQueryAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiQueryAttribute**
> \]

An attribute to produce the `ECS`-like query.
It creates a code-generated public method in the same type with the same name that receives
an instance of [KsiDomain](T.KsiDomainAttribute.g.md)
and a set of [KsiQueryParam](T.KsiQueryParamAttribute.g.md).
The generated method invokes the marked method for each matching entity in the given domain.

Method signature requirements: 
- Should be declared in a top-level `partial` type
- Should be a non-generic `static void` method
- All parameters should be by-ref parameters to named structures
- The first argument should be a readonly reference to
a [KsiDomain](T.KsiDomainAttribute.g.md)`Handle`
- The following parameters should be references to [KsiComponent](T.KsiComponentAttribute.g.md) types.
All component types should be unique, and at least one argument of that kind should be present
- Additionally, you can declare parameters marked with [KsiQueryParamAttribute](T.KsiQueryParamAttribute.g.md)
that are passed through from the generated method to the marked query method.
Only the [DynNoResizeAttribute](T.DynNoResizeAttribute.g.md) is inherited for these parameters

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class KsiQueryAttribute : Attribute
```
