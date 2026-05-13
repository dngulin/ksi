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
  - The `id` is used to identify the field in the binary stream.

Generated extension methods:
- `(in TSerializable).GetSerializedSize()` — Gets the serialized size.
- `(in TSerializable).SerializeTo(BinaryWriter writer)` — Serializes the struct to a stream.
- `(ref TSerializable).InitializeFrom(BinaryReader reader)` — Initializes the struct from serialized data.

Usage example:
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
  In the case of `RepeatedStruct`, it is reinterpreted as the [Count Prefix Size](api/T.ValueQualifierExtensions.g.md#in-valuequalifieritemcountprefixsize).

All four components of the Value Qualifier can have no more than 4 values.
So, in the [packed](api/T.ValueQualifierExtensions.g.md#in-valuequalifierpacked) form
the Value Qualifier is represented as a single byte:

```
[Value Kind: 2 bits][Length Prefix Size: 2 bits][Primitive Kind: 2 bits][Primitive Size / Count Prefix Size: 2 bits]
```

### Serialized Layouts

#### Primitive

Serialized layout for a single primitive:
```
[Qualifier: 1 byte] [Primitive: 1-8 bytes]
```

Notes:
- The `Qualifier` determines the size and type of the primitive.

#### Repeated Primitive

Serialized layout for a repeated primitive:
```
[Qualifier: 1 byte] [Length: 0-4 bytes] [Primitive: 1-8 bytes] [Primitive: 1-8 bytes] ...
```

Notes:
- The `Qualifier` determines the size of the length prefix, as well as the size and type of the primitive.
- For empty arrays, the `Length` is omitted.
- The item count can be calculated by dividing the `Length` by the `PrimitiveSize`.

#### Struct

Serialized layout for a single structure:
```
[Qualifier: 1 byte] [Length: 0-4 bytes] [Field ID: 1 byte] [Value] [Field ID: 1 byte] [Value] ...
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
[Qualifier: 1 byte] [Length: 0-4 bytes] [Item Count: 0-4 bytes] [Struct] [Struct] ...
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
