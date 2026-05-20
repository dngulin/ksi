# SpanReadExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / SpanReadExtensions**
> \]

Provides extension methods for [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1) to read binary data.

```csharp
namespace Ksi.Serialization
{
    public static class SpanReadExtensions
}
```

Static Methods
- [\(ref ReadOnlySpan\<byte\>\).ReadByte\(\)](#ref-readonlyspanbytereadbyte) — reads a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) from the start of the span and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadSByte\(\)](#ref-readonlyspanbytereadsbyte) — reads an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) from the start of the span and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadUInt16\(\)](#ref-readonlyspanbytereaduint16) — reads a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadInt16\(\)](#ref-readonlyspanbytereadint16) — reads a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadUInt32\(\)](#ref-readonlyspanbytereaduint32) — reads a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadInt32\(\)](#ref-readonlyspanbytereadint32) — reads an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadSingle\(\)](#ref-readonlyspanbytereadsingle) — reads a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadUInt64\(\)](#ref-readonlyspanbytereaduint64) — reads a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadInt64\(\)](#ref-readonlyspanbytereadint64) — reads a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadDouble\(\)](#ref-readonlyspanbytereaddouble) — reads a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).Read\(Span\<byte\>\)](#ref-readonlyspanbytereadspanbyte) — copies data from the start of the source span to the destination span and shrinks the source span from the start
- [\(ref ReadOnlySpan\<byte\>\).ReadLenPrefix\(LenPrefixSize\)](#ref-readonlyspanbytereadlenprefixlenprefixsize) — reads a length prefix from the start of the span and shrinks the span from the start
- [\(ref ReadOnlySpan\<byte\>\).Skip\(ValueQualifier\)](#ref-readonlyspanbyteskipvaluequalifier) — skips a value in the span based on its qualifier and shrinks the span from the start


## Static Methods


### \(ref ReadOnlySpan\<byte\>\).ReadByte\(\)

Reads a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) from the start of the span and shrinks the span from the start.

```csharp
public static byte ReadByte(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadSByte\(\)

Reads an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) from the start of the span and shrinks the span from the start.

```csharp
public static sbyte ReadSByte(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadUInt16\(\)

Reads a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static ushort ReadUInt16(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadInt16\(\)

Reads a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static short ReadInt16(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadUInt32\(\)

Reads a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static uint ReadUInt32(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadInt32\(\)

Reads an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static int ReadInt32(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadSingle\(\)

Reads a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static float ReadSingle(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadUInt64\(\)

Reads a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static ulong ReadUInt64(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadInt64\(\)

Reads a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static long ReadInt64(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).ReadDouble\(\)

Reads a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) from the start of the span in little-endian format and shrinks the span from the start.

```csharp
public static double ReadDouble(this ref ReadOnlySpan<byte> span)
```

Parameters
- `span` — the span to read from.

Returns the value read.


### \(ref ReadOnlySpan\<byte\>\).Read\(Span\<byte\>\)

Copies data from the start of the source span to the destination span and shrinks the source span from the start.

```csharp
public static void Read(this ref ReadOnlySpan<byte> span, Span<byte> value)
```

Parameters
- `span` — the source span to read from.
- `value` — the destination span to write to.


### \(ref ReadOnlySpan\<byte\>\).ReadLenPrefix\(LenPrefixSize\)

Reads a length prefix from the start of the span and shrinks the span from the start.

```csharp
public static uint ReadLenPrefix(this ref ReadOnlySpan<byte> span, LenPrefixSize lps)
```

Parameters
- `span` — the span to read from.
- `lps` — the size of the length prefix to read.

Returns the length value read.


### \(ref ReadOnlySpan\<byte\>\).Skip\(ValueQualifier\)

Skips a value in the span based on its qualifier and shrinks the span from the start.

```csharp
public static void Skip(this ref ReadOnlySpan<byte> span, ValueQualifier q)
```

Parameters
- `span` — the span to skip in.
- `q` — the qualifier of the value to skip.
