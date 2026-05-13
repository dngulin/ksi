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

Fields
- [SignedInt](#signedint) — a signed integer value
- [UnsignedInt](#unsignedint) — an unsigned integer value
- [FloatPoint](#floatpoint) — a floating-point value


## Fields


### SignedInt

A signed integer value.

Value is `0`.

```csharp
SignedInt

```


### UnsignedInt

An unsigned integer value.

Value is `1`.

```csharp
UnsignedInt

```


### FloatPoint

A floating-point value.

Value is `2`.

```csharp
FloatPoint

```
