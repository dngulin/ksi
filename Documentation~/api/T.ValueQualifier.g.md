# ValueQualifier

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
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

Fields
- [PackedSize](#packedsize) — size of the value qualifier in the packed format (1 byte)
- [Kind](#kind) — the kind of the value
- [LenPrefixSize](#lenprefixsize) — the size of the length prefix for the value
- [PrimitiveKind](#primitivekind) — the kind of the primitive value
- [PrimitiveSize](#primitivesize) — the size of the primitive value

Static Methods
- [ValueQualifier.Primitive\(PrimitiveKind, PrimitiveSize\)](#valuequalifierprimitiveprimitivekind-primitivesize) — creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a primitive value
- [ValueQualifier.RepeatedPrimitive\(PrimitiveKind, PrimitiveSize, LenPrefixSize\)](#valuequalifierrepeatedprimitiveprimitivekind-primitivesize-lenprefixsize) — creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a repeated primitive value
- [ValueQualifier.Struct\(LenPrefixSize\)](#valuequalifierstructlenprefixsize) — creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a struct value
- [ValueQualifier.RepeatedStruct\(LenPrefixSize, LenPrefixSize\)](#valuequalifierrepeatedstructlenprefixsize-lenprefixsize) — creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a repeated struct value
- [ValueQualifier.GetLenPrefix\(uint\)](#valuequalifiergetlenprefixuint) — gets the required `LenPrefixSize` to store the specified length
- [ValueQualifier.Unpack\(byte\)](#valuequalifierunpackbyte) — unpacks a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) from a packed [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)


## Fields


### PackedSize

Size of the value qualifier in the packed format (1 byte).

```csharp
public const int PackedSize = sizeof(byte)
```


### Kind

The kind of the value.

```csharp
public ValueKind Kind
```


### LenPrefixSize

The size of the length prefix for the value.
Is used only for len-prefixed values: `RepeatedPrimitive`, `Struct`, `RepeatedStruct`.

```csharp
public LenPrefixSize LenPrefixSize
```


### PrimitiveKind

The kind of the primitive value.
Is used only for primitive-based values: `Primitive`, `RepeatedPrimitive`.

```csharp
public PrimitiveKind PrimitiveKind
```


### PrimitiveSize

The size of the primitive value.
Is used only for primitive-based value values: `Primitive`, `RepeatedPrimitive`.

For `RepeatedStruct` that field is reinterpreted as `ItemCountPrefixSize`.

```csharp
public PrimitiveSize PrimitiveSize
```


## Static Methods


### ValueQualifier.Primitive\(PrimitiveKind, PrimitiveSize\)

Creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a primitive value.

```csharp
public static ValueQualifier Primitive(PrimitiveKind pk, PrimitiveSize ps)
```

Parameters
- `pk` — the kind of the primitive value.
- `ps` — the size of the primitive value.

Returns a new [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).


### ValueQualifier.RepeatedPrimitive\(PrimitiveKind, PrimitiveSize, LenPrefixSize\)

Creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a repeated primitive value.

```csharp
public static ValueQualifier RepeatedPrimitive(PrimitiveKind pk, PrimitiveSize ps, LenPrefixSize lps)
```

Parameters
- `pk` — the kind of the primitive value.
- `ps` — the size of the primitive value.
- `lps` — the size of the length prefix.

Returns a new [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).


### ValueQualifier.Struct\(LenPrefixSize\)

Creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a struct value.

```csharp
public static ValueQualifier Struct(LenPrefixSize lps)
```

Parameters
- `lps` — the size of the length prefix.

Returns a new [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).


### ValueQualifier.RepeatedStruct\(LenPrefixSize, LenPrefixSize\)

Creates a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) for a repeated struct value.

```csharp
public static ValueQualifier RepeatedStruct(LenPrefixSize lps, LenPrefixSize cps)
```

Parameters
- `lps` — the size of the length prefix.
- `cps` — the size of the count prefix.

Returns a new [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).


### ValueQualifier.GetLenPrefix\(uint\)

Gets the required `LenPrefixSize` to store the specified length.

```csharp
public static LenPrefixSize GetLenPrefix(uint len)
```

Parameters
- `len` — the length value.

Returns the smallest `LenPrefixSize` that can accommodate the length.


### ValueQualifier.Unpack\(byte\)

Unpacks a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) from a packed [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1).

```csharp
public static ValueQualifier Unpack(byte packed)
```

Parameters
- `packed` — the packed byte.

Returns the unpacked [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).
