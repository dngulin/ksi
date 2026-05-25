# LenPrefixSize

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / LenPrefixSize**
> \]

Represents the size of the length prefix in bytes.

```csharp
namespace Ksi.Serialization
{
    public enum LenPrefixSize : byte
}
```

Values
- [_0](#0) — no length prefix (0 bytes)
- [_8](#8) — 8-bit length prefix (1 byte)
- [_16](#16) — 16-bit length prefix (2 bytes)
- [_32](#32) — 32-bit length prefix (4 bytes)

Extension Methods
- [\(LenPrefixSize\).InBytes\(\)](#lenprefixsizeinbytes) — gets the size of the length prefix in bytes


## Values


### _0

No length prefix (0 bytes).

Value is `0`.


### _8

8-bit length prefix (1 byte).

Value is `1`.


### _16

16-bit length prefix (2 bytes).

Value is `2`.


### _32

32-bit length prefix (4 bytes).

Value is `3`.


## Extension Methods


### \(LenPrefixSize\).InBytes\(\)

Gets the size of the length prefix in bytes.

```csharp
public static int InBytes(this LenPrefixSize size)
```

Parameters
- `size` — the [LenPrefixSize](T.LenPrefixSize.g.md).

Returns the size in bytes.

> [!CAUTION]
> Possible exceptions: 
> - [ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.ArgumentOutOfRangeException?view=netstandard-2.1) — thrown when an invalid [LenPrefixSize](T.LenPrefixSize.g.md) is provided.
