# KsiPrimeUtil

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / KsiPrimeUtil**
> \]

Utility class to work with prime numbers.
Used by generated hash table implementations produced by [KsiHashTableAttribute](T.KsiHashTableAttribute.g.md).

```csharp
public static class KsiPrimeUtil
```

Static Methods
- [KsiPrimeUtil.IsPrime\(int\)](#ksiprimeutilisprimeint) — checks if the given number is a prime number
- [KsiPrimeUtil.NextPrime\(int\)](#ksiprimeutilnextprimeint) — finds the smallest prime number greater than a given number
- [KsiPrimeUtil.EqualOrNextPrime\(int\)](#ksiprimeutilequalornextprimeint) — finds the smallest prime number equal or greater than a given number


## Static Methods


### KsiPrimeUtil.IsPrime\(int\)

Checks if the given number is a prime number.

```csharp
public static bool IsPrime(int n)
```

Parameters
- `n` — number to check

Returns `true` if the given number is a prime number, or `false` otherwise


### KsiPrimeUtil.NextPrime\(int\)

Finds the smallest prime number greater than a given number.

```csharp
public static int NextPrime(int n)
```

Parameters
- `n` — number to start the search

Returns the smallest prime number bigger than a given number


### KsiPrimeUtil.EqualOrNextPrime\(int\)

Finds the smallest prime number equal or greater than a given number.

```csharp
public static int EqualOrNextPrime(int n)
```

Parameters
- `n` — number to start the search

Returns the smallest prime number equal or greater than a given number
