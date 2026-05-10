# BinaryReaderExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / BinaryReaderExtensions**
> \]

Provides extension methods for [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) to support Ksi serialization.

```csharp
namespace Ksi.Serialization
{
    public static class BinaryReaderExtensions
}
```

Static Methods
- [\(BinaryReader\).ReadLenPrefix\(LenPrefixSize\)](#binaryreaderreadlenprefixlenprefixsize) — reads a length prefix of the specified size from the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1)
- [\(BinaryReader\).Skip\(ValueQualifier\)](#binaryreaderskipvaluequalifier) — skips a value in the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) based on the provided [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1)


## Static Methods


### \(BinaryReader\).ReadLenPrefix\(LenPrefixSize\)

Reads a length prefix of the specified size from the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1).

```csharp
public static uint ReadLenPrefix(this BinaryReader br, LenPrefixSize lps)
```

Parameters
- `br` — the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) to read from.
- `lps` — the size of the length prefix.

Returns the length value read from the stream.

> [!CAUTION]
> Possible exceptions: 
> - [InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/System.InvalidOperationException?view=netstandard-2.1) — thrown when an invalid [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1) is provided.


### \(BinaryReader\).Skip\(ValueQualifier\)

Skips a value in the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) based on the provided [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).

```csharp
public static void Skip(this BinaryReader br, ValueQualifier q)
```

Parameters
- `br` — the [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) to skip in.
- `q` — the [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) describing the value to skip.
