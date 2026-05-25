# Serialization

> \[ [Getting Started](getting-started.md)
> \| [Traits](traits.md)
> \| [Collections](collections.md)
> \| [Referencing](borrow-checker-at-home.md)
> \| [ECS](ecs.md)
> \| **Serialization**
> \| [API](api/index.g.md)
> \]

The ѯ-Framework provides a source-generated binary serialization system
that uses Roslyn source generators to produce serialization code at compile time.

To enable serialization for a data structure, use the following attributes:
- [KsiSerializable](api/T.KsiSerializableAttribute.g.md) — Marks a struct for binary serialization source generation.
- [KsiSerializeField](api/T.KsiSerializeFieldAttribute.g.md) — Marks a field to be included in the serialized output.
Each field must have a unique `byte` identifier.
  - Only fields marked with this attribute are serialized.
  - The `id` is used to identify the field in the binary data.

The following types are supported for `[KsiSerializeField]` fields:
- **Primitives**: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`,
  `bool`, `char`.
- **Enums**: Serialized using their underlying primitive type.
- **Serializable Structs**: Structures marked with the `[KsiSerializable]` attribute.

Serialization attributes trigger code generation that produces stream- and buffer-based serialization APIs.
The buffer-based API receives a buffer by reference and shrinks it after reading or writing data.

Generated extension methods:
- `(in TSerializable).GetSerializedSize()` — Gets the serialized size.
- Stream-based API:
  - `(this BinaryWriter writer).Write(in TSerializable value)` — Serializes the struct to a stream.
  - `(in TSerializable).SerializeTo(BinaryWriter writer)` — Serializes the struct to a stream.
  - `(ref TSerializable).InitializeFrom(BinaryReader reader)` — Initializes the struct from a stream.
  - Low-level:
    - `(this BinaryWriter writer).Prepend(in TSerializable value, bool qualified)` — Serializes the struct to a stream, moving the position backwards.
    - `(ref TSerializable).InitializeFrom(BinaryReader reader, int len)` — Initializes the struct from a stream without reading a `ValueQualifier`.
- Buffer-based API:
  - `(ref Span<byte> buffer).Write(in TSerializable value)` — Serializes the struct to a buffer and shrinks the buffer size.
  - `(in TSerializable).SerializeTo(ref Span<byte> buffer)` — Serializes the struct to a buffer and shrinks the buffer size.
  - `(ref TSerializable).InitializeFrom(ref ReadOnlySpan<byte> buffer)` — Initializes the struct from a buffer and shrinks the buffer size.
  - Low-level:
      - `(ref Span<byte> buffer).Prepend(in TSerializable value, bool qualified)` — Serializes the struct to a buffer, shrinking it from the end.
      - `(ref TSerializable).InitializeFrom(ref ReadOnlySpan<byte> buffer, int len)` — Initializes the struct from a buffer without reading a `ValueQualifier`.

Usage example (Stream):
```csharp
[KsiSerializable]
[ExplicitCopy, DynSized, Dealloc]
public struct PlayerData
{
    [KsiSerializeField(0)] public int Health;
    [KsiSerializeField(1)] public RefList<int> Inventory;
    [KsiSerializeField(2)] public CustomState State;
}

[KsiSerializable]
public struct CustomState
{
    [KsiSerializeField(0)] public float Experience;
}

// Serialize
var data = new PlayerData { Health = 100 };
using (var writer = new BinaryWriter(stream))
    data.SerializeTo(writer);

// Deserialize
PlayerData loadedData = default; // Should be zeroed before calling InitializeFrom
using (var reader = new BinaryReader(stream))
    loadedData.InitializeFrom(reader);
```

Usage example (Buffer):
```csharp
var data = new PlayerData { Health = 100 };

// Prepare buffer
var size = data.GetSerializedSize();
var buffer = new byte[size];

// Serialize
Span<byte> writeSpan = buffer; 
data.SerializeTo(buffer); // use buffer.Prepend(data, true) for exact-sized buffers

// Deserialize
ReadOnlySpan<byte> readSpan = buffer; 
PlayerData loadedData = default;
loadedData.InitializeFrom(ref readSpan);
```

## Data Layout

The binary format uses a "tag-length-value" (TLV) inspired approach.
Every serialized value is preceded by a [Value Qualifier](api/T.ValueQualifier.g.md).

The Value Qualifier is composed of:
- [Value Kind](api/T.ValueKind.g.md): `Primitive`, `RepeatedPrimitive`, `Struct`, or `RepeatedStruct`.
- [Length Prefix Size](api/T.LenPrefixSize.g.md): `0`, `8`, `16`, or `32` bits.
  It is not used for the `Primitive` value kinds.
- [Primitive Kind](api/T.PrimitiveKind.g.md): `Unsigned Integer`, `Integer`, `Float`.
  Only used for the `Primitive` and `RepeatedPrimitive` value kinds.
- [Primitive Size](api/T.PrimitiveSize.g.md): `8`, `16`, `32`, or `64` bits. 
  Used for the `Primitive` and `RepeatedPrimitive` value kinds.
  In the case of `RepeatedStruct`, it is reinterpreted as
  the [Count Prefix Size](api/T.ValueQualifier.g.md#in-valuequalifieritemcountprefixsize).

All four components of the Value Qualifier can have no more than 4 values.
So, in the [packed](api/T.ValueQualifier.g.md#in-valuequalifierpacked) form
the Value Qualifier is represented as a single byte:

```
[Value Kind] [Length Prefix Size] [Primitive Kind] [Primitive Size / Count Prefix Size]
 2 bits       2 bits               2 bits           2 bits
```

### Serialized Layouts

#### Primitive

Serialized layout for a single primitive:
```
[Qualifier] [Primitive]
 1 byte      1-8 bytes
```

Notes:
- The `Qualifier` determines the size and type of the primitive.

#### Repeated Primitive

Serialized layout for a repeated primitive:
```
[Qualifier] [Length Prefix] [Primitive] [Primitive] ...
 1 byte      0-4 bytes       1-8 bytes   1-8 bytes
```

Notes:
- The `Qualifier` determines the size of the length prefix, as well as the size and type of the primitive.
- For empty arrays, the `Length` is omitted.
- The item count can be calculated by dividing the `Length` by the `PrimitiveSize`.

#### Struct

Serialized layout for a single structure:
```
[Qualifier] [Length Prefix] [Field ID] [Value  ] [Field ID] [Value  ] ...
 1 byte      0-4 bytes       1 byte     N bytes   1 byte     N bytes
                                        N > 0                N > 0               
```

Notes:
- Default (zeroed) and empty repeated fields are not serialized.
- For empty structs, the `Length` is omitted.
- `[Value]` is any serialized value: `Primitive`, `RepeatedPrimitive`, `Struct`, or `RepeatedStruct`.

> [!IMPORTANT]
> Zeroed fields are not serialized, so during deserialization, a field cannot be reset to its zeroed state.
> Always call `InitializeFrom` on a zeroed structure.

#### Repeated Struct

Serialized layout for a repeated structure:
```
[Qualifier] [Length Prefix] [Item Count] [Struct ] [Struct ] ...
 1 byte      0-4 bytes       0-4 bytes    N bytes   N bytes
                                          N > 0     N > 0
```

Notes:
- The `Qualifier` determines the size of the length and count prefixes.

## Serialization Diagnostics

Diagnostics related to serialization:

| Diagnostic Id | Severity | Title                                   |
|---------------|----------|-----------------------------------------|
| `SERDE01`     | Error    | Missing `[KsiSerializable]` attribute   |
| `SERDE02`     | Error    | Duplicate `[KsiSerializeField]` id      |
| `SERDE03`     | Error    | Invalid `[KsiSerializeField]` type      |
| `SERDE04`     | Error    | Low `[KsiSerializable]` accessibility   |
| `SERDE05`     | Error    | Low `[KsiSerializeField]` accessibility |
| `SERDE06`     | Error    | Static `[KsiSerializeField]`            |
