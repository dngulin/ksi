# ValueQualifierExtensions

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| [Serialization](../serialization.md)
> \| **[API](index.g.md) / ValueQualifierExtensions**
> \]

Provides extension methods for [ValueQualifier](T.ValueQualifier.g.md).

```csharp
namespace Ksi.Serialization
{
    public static class ValueQualifierExtensions
}
```

Static Methods
- [\(in ValueQualifier\).Packed\(\)](#in-valuequalifierpacked) — packs a [ValueQualifier](T.ValueQualifier.g.md) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1)
- [\(in ValueQualifier\).ItemCountPrefixSize\(\)](#in-valuequalifieritemcountprefixsize) — gets the [LenPrefixSize](T.LenPrefixSize.g.md) for the item count in a `RepeatedStruct`


## Static Methods


### \(in ValueQualifier\).Packed\(\)

Packs a [ValueQualifier](T.ValueQualifier.g.md) into a single [byte](https://learn.microsoft.com/en-us/dotnet/api/System.Byte?view=netstandard-2.1).

```csharp
public static byte Packed(this in ValueQualifier unpacked)
```

Parameters
- `unpacked` — the [ValueQualifier](T.ValueQualifier.g.md) to pack.

Returns the packed byte.


### \(in ValueQualifier\).ItemCountPrefixSize\(\)

Gets the [LenPrefixSize](T.LenPrefixSize.g.md) for the item count in a `RepeatedStruct`.

```csharp
public static LenPrefixSize ItemCountPrefixSize(this in ValueQualifier self)
```

Parameters
- `self` — the [ValueQualifier](T.ValueQualifier.g.md).

Returns the [LenPrefixSize](T.LenPrefixSize.g.md) used for item count.
