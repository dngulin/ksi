# PrefixedSizeOf

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / PrefixedSizeOf**
> \]

Utility class for calculating the size of serialized data.

```csharp
namespace Ksi.Serialization
{
    public static class PrefixedSizeOf
}
```

Static Methods
- [PrefixedSizeOf.Primitive\(int\)](#prefixedsizeofprimitiveint) — calculates the serialized size of a primitive value
- [PrefixedSizeOf.RepeatedPrimitive\(int, int\)](#prefixedsizeofrepeatedprimitiveint-int) — calculates the serialized size of a repeated primitive value
- [PrefixedSizeOf.Struct\(int\)](#prefixedsizeofstructint) — calculates the serialized size of a struct
- [PrefixedSizeOf.RepeatedStruct\(int, int\)](#prefixedsizeofrepeatedstructint-int) — calculates the serialized size of a repeated struct


## Static Methods


### PrefixedSizeOf.Primitive\(int\)

Calculates the serialized size of a primitive value.

```csharp
public static int Primitive(int size)
```

Parameters
- `size` — the size of the primitive value in bytes.

Returns the total serialized size including the value qualifier.


### PrefixedSizeOf.RepeatedPrimitive\(int, int\)

Calculates the serialized size of a repeated primitive value.

```csharp
public static int RepeatedPrimitive(int itemSize, int count)
```

Parameters
- `itemSize` — the size of a single item in bytes.
- `count` — the number of items.

Returns the total serialized size.


### PrefixedSizeOf.Struct\(int\)

Calculates the serialized size of a struct.

```csharp
public static int Struct(int size)
```

Parameters
- `size` — the size of the struct content in bytes.

Returns the total serialized size including the value qualifier and length prefix.


### PrefixedSizeOf.RepeatedStruct\(int, int\)

Calculates the serialized size of a repeated struct.

```csharp
public static int RepeatedStruct(int size, int count)
```

Parameters
- `size` — the total size of all struct items in bytes.
- `count` — the number of items.

Returns the total serialized size including value qualifier, length prefix, and count prefix.
