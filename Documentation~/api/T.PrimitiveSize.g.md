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

Fields
- [_8](#8) — 8-bit primitive (1 byte)
- [_16](#16) — 16-bit primitive (2 bytes)
- [_32](#32) — 32-bit primitive (4 bytes)
- [_64](#64) — 64-bit primitive (8 bytes)


## Fields


### _8

8-bit primitive (1 byte).

Value is `0`.

```csharp
_8

```


### _16

16-bit primitive (2 bytes).

Value is `1`.

```csharp
_16

```


### _32

32-bit primitive (4 bytes).

Value is `2`.

```csharp
_32

```


### _64

64-bit primitive (8 bytes).

Value is `3`.

```csharp
_64

```
