# ValueQualifierExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / ValueQualifierExtensions**
> \]

Provides extension methods for [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).

```csharp
namespace Ksi.Serialization
{
    public static class ValueQualifierExtensions
}
```

Static Methods
- [\(in ValueQualifier\).Packed\(\)](#in-valuequalifierpacked) — packs a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)
- [\(in ValueQualifier\).ItemCountPrefixSize\(\)](#in-valuequalifieritemcountprefixsize) — gets the [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1) for the item count in a `RepeatedStruct`


## Static Methods


### \(in ValueQualifier\).Packed\(\)

Packs a [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1).

```csharp
public static byte Packed(this in ValueQualifier unpacked)
```

Parameters
- `unpacked` — the [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1) to pack.

Returns the packed byte.


### \(in ValueQualifier\).ItemCountPrefixSize\(\)

Gets the [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1) for the item count in a `RepeatedStruct`.

```csharp
public static LenPrefixSize ItemCountPrefixSize(this in ValueQualifier self)
```

Parameters
- `self` — the [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1).

Returns the [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1) used for item count.
