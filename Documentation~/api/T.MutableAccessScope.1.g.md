# MutableAccessScope\<T\>

Structure that provides mutable exclusive access to wrapped data.
Should be disposed after usage to release the lock on data.

```csharp
public readonly ref struct MutableAccessScope<T> where T : struct
```

Properties
- [Value](#value)

Methods
- [Dispose()](#dispose)


## Properties


### Value

Returns a mutable reference to the wrapped data.

```csharp
public ref T Value
```

> [!CAUTION]
> Possible exceptions: 
> - `InvalidOperationException` — If the data is not available to this access scope (e.g. usage after disposing).


## Methods


### Dispose()

Deactivates this access scope but allows creating a new one.

```csharp
public void Dispose()
```
