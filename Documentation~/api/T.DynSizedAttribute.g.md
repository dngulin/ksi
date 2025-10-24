# DynSizedAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / DynSizedAttribute**
> \]

An attribute to indicate an [ExplicitCopy](T.ExplicitCopyAttribute.g.md) type
that contains a dynamically sized buffer.
It enables reference lifetime and aliasing diagnostics for the marked struct.

Should be added to a struct that contains fields of the `DynSized` type.

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class DynSizedAttribute : Attribute
```
