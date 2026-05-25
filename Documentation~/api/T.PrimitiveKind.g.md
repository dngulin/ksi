# PrimitiveKind

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / PrimitiveKind**
> \]

Represents the primitive value kind.

```csharp
namespace Ksi.Serialization
{
    public enum PrimitiveKind : byte
}
```

Values
- [SignedInt](#signedint) — a signed integer value
- [UnsignedInt](#unsignedint) — an unsigned integer value
- [FloatPoint](#floatpoint) — a floating-point value


## Values


### SignedInt

A signed integer value.

Value is `0`.


### UnsignedInt

An unsigned integer value.

Value is `1`.


### FloatPoint

A floating-point value.

Value is `2`.
