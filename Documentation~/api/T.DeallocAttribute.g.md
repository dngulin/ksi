# DeallocAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / DeallocAttribute**
> \]

Attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that requires manual deallocation.
Should be added to a struct that contains fields of the `Dealloc` type.
Can be also applied to a generic type parameter to make it compatible with `Dealloc` types.

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
public sealed class DeallocAttribute : Attribute
```
