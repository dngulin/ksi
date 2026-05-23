# PrimitiveSizeExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / PrimitiveSizeExtensions**
> \]

Provides extension methods for [PrimitiveSize](T.PrimitiveSize.g.md).

```csharp
namespace Ksi.Serialization
{
    public static class PrimitiveSizeExtensions
}
```

Static Methods
- [\(PrimitiveSize\).InBytes\(\)](#primitivesizeinbytes) — gets the size of the primitive in bytes


## Static Methods


### \(PrimitiveSize\).InBytes\(\)

Gets the size of the primitive in bytes.

```csharp
public static int InBytes(this PrimitiveSize size)
```

Parameters
- `size` — the [PrimitiveSize](T.PrimitiveSize.g.md).

Returns the size in bytes.

> [!CAUTION]
> Possible exceptions: 
> - [ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.ArgumentOutOfRangeException?view=netstandard-2.1) — thrown when an invalid [PrimitiveSize](T.PrimitiveSize.g.md) is provided.
