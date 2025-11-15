# MutableAccessScope\<T\>

> \[ [Getting Started](getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / MutableAccessScope\<T\>**
> \]

A structure that provides mutable exclusive access to wrapped data.
Should be disposed after usage to release access lock from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess-1.g.md) instance.

```csharp
public readonly ref struct MutableAccessScope<[ExplicitCopy, DynSized, Dealloc] T> where T : struct
```

Properties
- [Value](#value) — returns a mutable reference to the wrapped data

Methods
- [Dispose\(\)](#dispose) — deactivates the access scope and allows creating a new one from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess-1.g.md) instance


## Properties


### Value

Returns a mutable reference to the wrapped data.

Does not add any segments to `RefPath`.

```csharp
public ref T Value { get; }
```

> [!CAUTION]
> Possible exceptions: 
> - [InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/System.InvalidOperationException?view=netstandard-2.1) — if the data is not available to this access scope (e.g. usage after disposing).


## Methods


### Dispose\(\)

Deactivates the access scope and allows creating a new one from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess-1.g.md) instance.

```csharp
public void Dispose()
```
