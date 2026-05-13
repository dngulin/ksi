# KsiSerializeFieldAttribute

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / KsiSerializeFieldAttribute**
> \]

Marks a serializable field and specifies its binary field id.

```csharp
namespace Ksi
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class KsiSerializeFieldAttribute : Attribute
}
```

Constructors
- [KsiSerializeFieldAttribute\(byte\)](#ksiserializefieldattributebyte) — initializes a new instance of the [KsiSerializeFieldAttribute](T.KsiSerializeFieldAttribute.g.md) class

Properties
- [Id](#id) — gets the binary field identifier


## Constructors


### KsiSerializeFieldAttribute\(byte\)

Initializes a new instance of the [KsiSerializeFieldAttribute](T.KsiSerializeFieldAttribute.g.md) class.

```csharp
public KsiSerializeFieldAttribute(byte id)
```

Parameters
- `id` — the binary field identifier.


## Properties


### Id

Gets the binary field identifier.

```csharp
public byte Id { get; }
```
