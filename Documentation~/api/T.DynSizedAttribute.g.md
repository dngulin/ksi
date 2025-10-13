# DynSizedAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / DynSizedAttribute**
> \]

Attribute to indicate an [ExplicitCopy](T.ExplicitCopyAttribute.g.md) type
that contains a dynamically sized buffer.
Should be added to a struct that contains fields of the `DynSized` type.
Enables reference lifetime and aliasing diagnostics.

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class DynSizedAttribute : Attribute
```
