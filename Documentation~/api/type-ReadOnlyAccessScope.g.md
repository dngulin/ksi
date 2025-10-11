# ReadOnlyAccessScope\<T\>

Structure that provides readonly exclusive access to wrapped data.
Should be disposed after usage to release the lock on data.

```csharp
public readonly ref struct ReadOnlyAccessScope<T> where T : struct
```


## Properties


### Value

Returns a readonly reference to the wrapped data.

```csharp
public ref readonly T Value
```


## Methods


### Dispose()

Deactivates this access scope but allows creating a new one.

```csharp
public void Dispose()
```
