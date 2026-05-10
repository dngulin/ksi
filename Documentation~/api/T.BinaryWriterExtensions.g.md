# BinaryWriterExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / BinaryWriterExtensions**
> \]

Provides extension methods for [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1) to support Ksi serialization.

```csharp
namespace Ksi.Serialization
{
    public static class BinaryWriterExtensions
}
```

Static Methods
- [\(BinaryWriter\).Prepend\(byte\)](#binarywriterprependbyte) — prepends a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(sbyte\)](#binarywriterprependsbyte) — prepends an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(bool\)](#binarywriterprependbool) — prepends a [bool](https://learn.microsoft.com/en-us/dotnet/api/System.Boolean?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(ushort\)](#binarywriterprependushort) — prepends a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(short\)](#binarywriterprependshort) — prepends a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(uint\)](#binarywriterprependuint) — prepends a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(int\)](#binarywriterprependint) — prepends an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(float\)](#binarywriterprependfloat) — prepends a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(ulong\)](#binarywriterprependulong) — prepends a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(long\)](#binarywriterprependlong) — prepends a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(double\)](#binarywriterprependdouble) — prepends a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) to the stream
- [\(BinaryWriter\).Prepend\(ReadOnlySpan\<byte\>\)](#binarywriterprependreadonlyspanbyte) — prepends a span of [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)s to the stream
- [\(BinaryWriter\).PrependLenPrefix\(uint, out LenPrefixSize\)](#binarywriterprependlenprefixuint-out-lenprefixsize) — prepends a length prefix to the stream


## Static Methods


### \(BinaryWriter\).Prepend\(byte\)

Prepends a [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, byte value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(sbyte\)

Prepends an [sbyte](https://learn.microsoft.com/en-us/dotnet/api/System.SByte?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, sbyte value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(bool\)

Prepends a [bool](https://learn.microsoft.com/en-us/dotnet/api/System.Boolean?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, bool value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(ushort\)

Prepends a [ushort](https://learn.microsoft.com/en-us/dotnet/api/System.UInt16?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, ushort value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(short\)

Prepends a [short](https://learn.microsoft.com/en-us/dotnet/api/System.Int16?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, short value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(uint\)

Prepends a [uint](https://learn.microsoft.com/en-us/dotnet/api/System.UInt32?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, uint value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(int\)

Prepends an [int](https://learn.microsoft.com/en-us/dotnet/api/System.Int32?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, int value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(float\)

Prepends a [float](https://learn.microsoft.com/en-us/dotnet/api/System.Single?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, float value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(ulong\)

Prepends a [ulong](https://learn.microsoft.com/en-us/dotnet/api/System.UInt64?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, ulong value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(long\)

Prepends a [long](https://learn.microsoft.com/en-us/dotnet/api/System.Int64?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, long value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(double\)

Prepends a [double](https://learn.microsoft.com/en-us/dotnet/api/System.Double?view=netstandard-2.1) to the stream.

```csharp
public static void Prepend(this BinaryWriter self, double value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the value to prepend.


### \(BinaryWriter\).Prepend\(ReadOnlySpan\<byte\>\)

Prepends a span of [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)s to the stream.

```csharp
public static void Prepend(this BinaryWriter self, ReadOnlySpan<byte> value)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `value` — the span to prepend.


### \(BinaryWriter\).PrependLenPrefix\(uint, out LenPrefixSize\)

Prepends a length prefix to the stream.

```csharp
public static void PrependLenPrefix(this BinaryWriter self, uint len, out LenPrefixSize lps)
```

Parameters
- `self` — the [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1).
- `len` — the length to prepend.
- `lps` — the size of the length prefix that was prepended.
