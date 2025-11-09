# ExclusiveAccess\<T\>

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / ExclusiveAccess\<T\>**
> \]

A container that provides exclusive access to inner data.
It is achieved by maintaining only one active [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md) or [ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md) wrapping inner data.
Supposed to wrap [DynSized](T.DynSizedAttribute.g.md) structures.

```csharp
public sealed class ExclusiveAccess<[ExplicitCopy, Dealloc] T> where T: struct
```

Properties
- [Mutable](#mutable) — creates a new instance of [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md)
- [ReadOnly](#readonly) — creates a new instance of `[ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md)


## Properties


### Mutable

Creates a new instance of [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md).

```csharp
public MutableAccessScope<T> Mutable { get; }
```

> [!CAUTION]
> Possible exceptions: 
> - [InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/System.InvalidOperationException?view=netstandard-2.1) — if an active instance of [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md) or [ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md) already exists.


### ReadOnly

Creates a new instance of `[ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md).

```csharp
public ReadOnlyAccessScope<T> ReadOnly { get; }
```

> [!CAUTION]
> Possible exceptions: 
> - [InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/System.InvalidOperationException?view=netstandard-2.1) — if an active instance of [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md) or [ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md) already exists.
