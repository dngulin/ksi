# DynNoResizeAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / DynNoResizeAttribute**
> \]

Attribute that disallows any resizing operations
on a [DynSized](T.DynSizedAttribute.g.md) type instance.

Hints the reference lifetime analyzer that any internal buffer cannot be resized.

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DynNoResizeAttribute : Attribute
```
