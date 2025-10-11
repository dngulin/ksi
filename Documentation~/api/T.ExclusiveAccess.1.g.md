# ExclusiveAccess\<T\>

Container designed to provide exclusive access to inner data.
It is achieved by maintaining only one active `MutableAccessScope` or `ReadOnlyAccessScope` wrapping inner data.
Supposed to wrap `[DynSized]` structures.

```csharp
public sealed class ExclusiveAccess<T> where T: struct
```


## Properties


### Mutable

Creates a new instance of `MutableAccessScope`.

```csharp
public MutableAccessScope<T> Mutable
```


### ReadOnly

Creates a new instance of `ReadOnlyAccessScope`.

```csharp
public ReadOnlyAccessScope<T> ReadOnly
```
