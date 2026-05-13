# DynNoResizeAttribute

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / DynNoResizeAttribute**
> \]

An attribute to disallow resizing operations on a [DynSized](T.DynSizedAttribute.g.md) parameter.

Hints the reference lifetime analyzer that any internal buffer cannot be resized.

```csharp
namespace Ksi
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DynNoResizeAttribute : Attribute
}
```
