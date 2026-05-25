# ValueQualifier

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / ValueQualifier**
> \]

Describes a value in the Ksi binary format.
It is serialized as a single byte and prefixes any serialized value.

Serialization layouts:
- `Primitive`: `Qualifier, Value`
- `RepeatedPrimitive`: `Qualifier, [Length, Value, Value, ...]`
- `Struct`: `Qualifier, [Length, Value]`
- `RepeatedStruct`: `Qualifier, [Length, ItemCount, Qualifier, [LenPrefix, Value], Qualifier, [LenPrefix, Value], ...]`

```csharp
namespace Ksi.Serialization
{
    public struct ValueQualifier
}
```

Static Creation Methods
- [ValueQualifier.Primitive\(PrimitiveKind, PrimitiveSize\)](#valuequalifierprimitiveprimitivekind-primitivesize) — creates a [ValueQualifier](T.ValueQualifier.g.md) for a primitive value
- [ValueQualifier.RepeatedPrimitive\(PrimitiveKind, PrimitiveSize, LenPrefixSize\)](#valuequalifierrepeatedprimitiveprimitivekind-primitivesize-lenprefixsize) — creates a [ValueQualifier](T.ValueQualifier.g.md) for a repeated primitive value
- [ValueQualifier.RepeatedStruct\(LenPrefixSize, LenPrefixSize\)](#valuequalifierrepeatedstructlenprefixsize-lenprefixsize) — creates a [ValueQualifier](T.ValueQualifier.g.md) for a repeated struct value
- [ValueQualifier.Struct\(LenPrefixSize\)](#valuequalifierstructlenprefixsize) — creates a [ValueQualifier](T.ValueQualifier.g.md) for a struct value
- [ValueQualifier.Unpack\(byte\)](#valuequalifierunpackbyte) — unpacks a [ValueQualifier](T.ValueQualifier.g.md) from a packed [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)

Constants
- [PackedSize](#packedsize) — size of the value qualifier in the packed format (1 byte)

Fields
- [Kind](#kind) — the kind of the value
- [LenPrefixSize](#lenprefixsize) — the size of the length prefix for the value
- [PrimitiveKind](#primitivekind) — the kind of the primitive value
- [PrimitiveSize](#primitivesize) — the size of the primitive value

Static Methods
- [ValueQualifier.GetLenPrefix\(uint\)](#valuequalifiergetlenprefixuint) — gets the required `LenPrefixSize` to store the specified length

Extension Methods
- [\(in ValueQualifier\).ItemCountPrefixSize\(\)](#in-valuequalifieritemcountprefixsize) — gets the [LenPrefixSize](T.LenPrefixSize.g.md) for the item count in a `RepeatedStruct`
- [\(in ValueQualifier\).Packed\(\)](#in-valuequalifierpacked) — packs a [ValueQualifier](T.ValueQualifier.g.md) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)


## Static Creation Methods


### ValueQualifier.Primitive\(PrimitiveKind, PrimitiveSize\)

Creates a [ValueQualifier](T.ValueQualifier.g.md) for a primitive value.

```csharp
public static ValueQualifier Primitive(PrimitiveKind pk, PrimitiveSize ps)
```

Parameters
- `pk` — the kind of the primitive value.
- `ps` — the size of the primitive value.

Returns a new [ValueQualifier](T.ValueQualifier.g.md).


### ValueQualifier.RepeatedPrimitive\(PrimitiveKind, PrimitiveSize, LenPrefixSize\)

Creates a [ValueQualifier](T.ValueQualifier.g.md) for a repeated primitive value.

```csharp
public static ValueQualifier RepeatedPrimitive(PrimitiveKind pk, PrimitiveSize ps, LenPrefixSize lps)
```

Parameters
- `pk` — the kind of the primitive value.
- `ps` — the size of the primitive value.
- `lps` — the size of the length prefix.

Returns a new [ValueQualifier](T.ValueQualifier.g.md).


### ValueQualifier.RepeatedStruct\(LenPrefixSize, LenPrefixSize\)

Creates a [ValueQualifier](T.ValueQualifier.g.md) for a repeated struct value.

```csharp
public static ValueQualifier RepeatedStruct(LenPrefixSize lps, LenPrefixSize cps)
```

Parameters
- `lps` — the size of the length prefix.
- `cps` — the size of the count prefix.

Returns a new [ValueQualifier](T.ValueQualifier.g.md).


### ValueQualifier.Struct\(LenPrefixSize\)

Creates a [ValueQualifier](T.ValueQualifier.g.md) for a struct value.

```csharp
public static ValueQualifier Struct(LenPrefixSize lps)
```

Parameters
- `lps` — the size of the length prefix.

Returns a new [ValueQualifier](T.ValueQualifier.g.md).


### ValueQualifier.Unpack\(byte\)

Unpacks a [ValueQualifier](T.ValueQualifier.g.md) from a packed [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1).

```csharp
public static ValueQualifier Unpack(byte packed)
```

Parameters
- `packed` — the packed byte.

Returns the unpacked [ValueQualifier](T.ValueQualifier.g.md).


## Constants


### PackedSize

Size of the value qualifier in the packed format (1 byte).

```csharp
public const int PackedSize = sizeof(byte)
```


## Fields


### Kind

The kind of the value.

```csharp
public ValueKind Kind
```

Type: [ValueKind](T.ValueKind.g.md)


### LenPrefixSize

The size of the length prefix for the value.
Is used only for len-prefixed values: `RepeatedPrimitive`, `Struct`, `RepeatedStruct`.

```csharp
public LenPrefixSize LenPrefixSize
```

Type: [LenPrefixSize](T.LenPrefixSize.g.md)


### PrimitiveKind

The kind of the primitive value.
Is used only for primitive-based values: `Primitive`, `RepeatedPrimitive`.

```csharp
public PrimitiveKind PrimitiveKind
```

Type: [PrimitiveKind](T.PrimitiveKind.g.md)


### PrimitiveSize

The size of the primitive value.
Is used only for primitive-based value values: `Primitive`, `RepeatedPrimitive`.

For `RepeatedStruct` that field is reinterpreted as `ItemCountPrefixSize`.

```csharp
public PrimitiveSize PrimitiveSize
```

Type: [PrimitiveSize](T.PrimitiveSize.g.md)


## Static Methods


### ValueQualifier.GetLenPrefix\(uint\)

Gets the required `LenPrefixSize` to store the specified length.

```csharp
public static LenPrefixSize GetLenPrefix(uint len)
```

Parameters
- `len` — the length value.

Returns the smallest `LenPrefixSize` that can accommodate the length.


## Extension Methods


### \(in ValueQualifier\).ItemCountPrefixSize\(\)

Gets the [LenPrefixSize](T.LenPrefixSize.g.md) for the item count in a `RepeatedStruct`.

```csharp
public static LenPrefixSize ItemCountPrefixSize(this in ValueQualifier self)
```

Parameters
- `self` — the [ValueQualifier](T.ValueQualifier.g.md).

Returns the [LenPrefixSize](T.LenPrefixSize.g.md) used for item count.


### \(in ValueQualifier\).Packed\(\)

Packs a [ValueQualifier](T.ValueQualifier.g.md) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1).

```csharp
public static byte Packed(this in ValueQualifier unpacked)
```

Parameters
- `unpacked` — the [ValueQualifier](T.ValueQualifier.g.md) to pack.

Returns the packed byte.
