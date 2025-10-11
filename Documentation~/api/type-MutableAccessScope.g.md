# MutableAccessScope\<T\>

Structure that provides mutable exclusive access to wrapped data.
Should be disposed after usage to release the lock on data.

```csharp
public readonly ref struct MutableAccessScope<T> where T : struct
```


## Properties


### Value

Returns a mutable reference to the wrapped data.

```csharp
public ref T Value
```


## Methods


### Dispose()

Deactivates this access scope but allows creating a new one.

```csharp
public void Dispose()
```
