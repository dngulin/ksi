# ValueKind

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / ValueKind**
> \]

Represents the kind of value being serialized.

```csharp
namespace Ksi.Serialization
{
    public enum ValueKind : byte
}
```

Values
- [Primitive](#primitive) — a primitive value
- [RepeatedPrimitive](#repeatedprimitive) — a repeated primitive value: `TRefList<TPrimitive>`
- [Struct](#struct) — a struct value
- [RepeatedStruct](#repeatedstruct) — a repeated struct value: `TRefList<TStruct>`


## Values


### Primitive

A primitive value.

Value is `0`.


### RepeatedPrimitive

A repeated primitive value: `TRefList<TPrimitive>`.

Value is `1`.


### Struct

A struct value.

Value is `2`.


### RepeatedStruct

A repeated struct value: `TRefList<TStruct>`.

Value is `3`.
