# LenPrefixSizeExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / LenPrefixSizeExtensions**
> \]

Provides extension methods for [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1).

```csharp
namespace Ksi.Serialization
{
    public static class LenPrefixSizeExtensions
}
```

Static Methods
- [\(LenPrefixSize\).InBytes\(\)](#lenprefixsizeinbytes) — gets the size of the length prefix in bytes


## Static Methods


### \(LenPrefixSize\).InBytes\(\)

Gets the size of the length prefix in bytes.

```csharp
public static int InBytes(this LenPrefixSize size)
```

Parameters
- `size` — the [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1).

Returns the size in bytes.

> [!CAUTION]
> Possible exceptions: 
> - [ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/System.ArgumentOutOfRangeException?view=netstandard-2.1) — thrown when an invalid [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1) is provided.
