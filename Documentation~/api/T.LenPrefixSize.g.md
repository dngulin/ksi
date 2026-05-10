# LenPrefixSize

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / LenPrefixSize**
> \]

Represents the size of the length prefix in bytes.

```csharp
namespace Ksi.Serialization
{
    public enum LenPrefixSize : byte
}
```

Fields
- [_0](#0) — no length prefix (0 bytes)
- [_8](#8) — 8-bit length prefix (1 byte)
- [_16](#16) — 16-bit length prefix (2 bytes)
- [_32](#32) — 32-bit length prefix (4 bytes)


## Fields


### _0

No length prefix (0 bytes).

Value is `0`.

```csharp
_0

```


### _8

8-bit length prefix (1 byte).

Value is `1`.

```csharp
_8

```


### _16

16-bit length prefix (2 bytes).

Value is `2`.

```csharp
_16

```


### _32

32-bit length prefix (4 bytes).

Value is `3`.

```csharp
_32

```
