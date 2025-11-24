# ExplicitCopyAttribute

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / ExplicitCopyAttribute**
> \]

A trait attribute that forbids structure implicit copying.

Should be added to a struct that contains fields of `ExplicitCopy` type.

Can be also applied to a generic type parameter to make it compatible with `ExplicitCopy` types.

Attribute triggers code generation for explicit copy extension methods:
- `(in TExpCopy).CopyTo(ref TExpCopy other)` — copies the current struct to another one
- `(ref TExpCopy).CopyFrom(in TExpCopy other)` — copies another struct to the current one
- `(in TRefList<TExpCopy>).CopyTo(ref TRefList<TExpCopy> other)` — copies all items
of the current list to another one
- `(ref TRefList<TExpCopy>).CopyFrom(in TRefList<TExpCopy> other)` — copies all items
of another struct to the current one

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
public sealed class ExplicitCopyAttribute : Attribute
```
