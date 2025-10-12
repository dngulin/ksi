# KsiExtensions

General purpose extension methods provided by the ѯ-Framework.

```csharp
public static class KsiExtensions
```

Static Methods
- [(ref T).Move()](#ref-tmove) — move structure ownership


## Static Methods


### (ref T).Move()

Move structure ownership. After invocation the original value is set to `default` (zeroed).
Can be required to work with `[ExplicitCopy]` types.

```csharp
public static T Move<[ExplicitCopy, Dealloc] T>(this ref T self) where T : struct
```

Parameters
- `self` — instance to be moved

Returns A new instance crated from the `self` parameter
