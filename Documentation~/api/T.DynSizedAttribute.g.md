# DynSizedAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| **[API](index.g.md) / DynSizedAttribute**
> \]

Attribute to indicate an [ExplicitCopy](T.ExplicitCopyAttribute.g.md) type
that contains a dynamically sized buffer that enables reference lifetime and aliasing diagnostics.

Should be added to a struct that contains fields of the `DynSized` type.

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class DynSizedAttribute : Attribute
```
