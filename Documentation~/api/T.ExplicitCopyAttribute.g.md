# ExplicitCopyAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / ExplicitCopyAttribute**
> \]

Attribute that forbids structure implicit copying and provides explicit copy extension methods.
Can be applied only to POD types without any methods and private fields.
Should be added to a struct that contains fields of `ExplicitCopy` type.
Can be also applied to a generic type parameter to make it compatible with `ExplicitCopy` types.

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
public sealed class ExplicitCopyAttribute : Attribute
```
