# SpanWriteExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / SpanWriteExtensions**
> \]

Provides extension methods for [Span\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.Span-1?view=netstandard-2.1) to write binary data by prepending it to the end of the span.

```csharp
namespace Ksi.Serialization
{
    public static class SpanWriteExtensions
}
```

Static Methods
- [\(ref Span\<byte\>\).Prepend\(byte\)](#ref-spanbyteprependbyte) — prepends a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) to the end of the span and shrinks the span from the end by 1
- [\(ref Span\<byte\>\).Prepend\(sbyte\)](#ref-spanbyteprependsbyte) — prepends an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) to the end of the span and shrinks the span from the end by 1
- [\(ref Span\<byte\>\).Prepend\(ushort\)](#ref-spanbyteprependushort) — prepends a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(short\)](#ref-spanbyteprependshort) — prepends a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(uint\)](#ref-spanbyteprependuint) — prepends a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(int\)](#ref-spanbyteprependint) — prepends an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(float\)](#ref-spanbyteprependfloat) — prepends a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(ulong\)](#ref-spanbyteprependulong) — prepends a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(long\)](#ref-spanbyteprependlong) — prepends a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(double\)](#ref-spanbyteprependdouble) — prepends a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end
- [\(ref Span\<byte\>\).Prepend\(ReadOnlySpan\<byte\>\)](#ref-spanbyteprependreadonlyspanbyte) — prepends a [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1) of bytes to the end of the span and shrinks the span from the end
- [\(ref Span\<byte\>\).PrependLenPrefix\(uint, out LenPrefixSize\)](#ref-spanbyteprependlenprefixuint-out-lenprefixsize) — prepends a length prefix to the end of the span and shrinks the span from the end


## Static Methods


### \(ref Span\<byte\>\).Prepend\(byte\)

Prepends a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) to the end of the span and shrinks the span from the end by 1.

```csharp
public static void Prepend(this ref Span<byte> span, byte value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(sbyte\)

Prepends an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) to the end of the span and shrinks the span from the end by 1.

```csharp
public static void Prepend(this ref Span<byte> span, sbyte value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(ushort\)

Prepends a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, ushort value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(short\)

Prepends a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, short value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(uint\)

Prepends a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, uint value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(int\)

Prepends an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, int value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(float\)

Prepends a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, float value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(ulong\)

Prepends a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, ulong value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(long\)

Prepends a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, long value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(double\)

Prepends a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) to the end of the span in little-endian format and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, double value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).Prepend\(ReadOnlySpan\<byte\>\)

Prepends a [ReadOnlySpan\<T\>](https://learn.microsoft.com/en-us/dotnet/api/System.ReadOnlySpan-1?view=netstandard-2.1) of bytes to the end of the span and shrinks the span from the end.

```csharp
public static void Prepend(this ref Span<byte> span, ReadOnlySpan<byte> value)
```

Parameters
- `span` — the span to write to.
- `value` — the value to write.


### \(ref Span\<byte\>\).PrependLenPrefix\(uint, out LenPrefixSize\)

Prepends a length prefix to the end of the span and shrinks the span from the end.

```csharp
public static void PrependLenPrefix(this ref Span<byte> span, uint len, out LenPrefixSize lps)
```

Parameters
- `span` — the span to write to.
- `len` — the length to write.
- `lps` — the size of the length prefix used.
