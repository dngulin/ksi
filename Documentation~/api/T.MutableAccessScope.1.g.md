# MutableAccessScope\<T\>

Structure that provides mutable exclusive access to wrapped data.
Should be disposed after usage to release access lock from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess.1.g.md) instance.

```csharp
public readonly ref struct MutableAccessScope<T> where T : struct
```

Properties
- [Value](#value) — returns a mutable reference to the wrapped data

Methods
- [Dispose\(\)](#dispose) — deactivates the access scope and allows creating a new one from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess.1.g.md) instance


## Properties


### Value

Returns a mutable reference to the wrapped data.

```csharp
public ref T Value
```

> [!CAUTION]
> Possible exceptions: 
> - `InvalidOperationException` — if the data is not available to this access scope (e.g. usage after disposing).


## Methods


### Dispose\(\)

Deactivates the access scope and allows creating a new one from the parent [ExclusiveAccess\<T\>](T.ExclusiveAccess.1.g.md) instance.

```csharp
public void Dispose()
```
