# ValueKind

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / ValueKind**
> \]

Represents the kind of value being serialized.

```csharp
namespace Ksi.Serialization
{
    public enum ValueKind : byte
}
```

Fields
- [Primitive](#primitive) — a primitive value
- [RepeatedPrimitive](#repeatedprimitive) — a repeated primitive value: `TRefList<TPrimitive>`
- [Struct](#struct) — a struct value
- [RepeatedStruct](#repeatedstruct) — a repeated struct value: `TRefList<TStruct>`


## Fields


### Primitive

A primitive value.

Value is `0`.

```csharp
Primitive

```


### RepeatedPrimitive

A repeated primitive value: `TRefList<TPrimitive>`.

Value is `1`.

```csharp
RepeatedPrimitive

```


### Struct

A struct value.

Value is `2`.

```csharp
Struct

```


### RepeatedStruct

A repeated struct value: `TRefList<TStruct>`.

Value is `3`.

```csharp
RepeatedStruct

```
