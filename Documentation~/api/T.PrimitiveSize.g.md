# PrimitiveSize

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / PrimitiveSize**
> \]

Represents the size of a primitive value in bits.

```csharp
namespace Ksi.Serialization
{
    public enum PrimitiveSize : byte
}
```

Values
- [_8](#8) — 8-bit primitive (1 byte)
- [_16](#16) — 16-bit primitive (2 bytes)
- [_32](#32) — 32-bit primitive (4 bytes)
- [_64](#64) — 64-bit primitive (8 bytes)

Extension Methods
- [\(PrimitiveSize\).InBytes\(\)](#primitivesizeinbytes) — gets the size of the primitive in bytes


## Values


### _8

8-bit primitive (1 byte).

Value is `0`.


### _16

16-bit primitive (2 bytes).

Value is `1`.


### _32

32-bit primitive (4 bytes).

Value is `2`.


### _64

64-bit primitive (8 bytes).

Value is `3`.


## Extension Methods


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
