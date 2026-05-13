# KsiSerializableAttribute

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / KsiSerializableAttribute**
> \]

Marks a struct for Ksi binary serialization source generation.

```csharp
namespace Ksi
{
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiSerializableAttribute : Attribute
}
```
