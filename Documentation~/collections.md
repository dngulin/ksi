# Collections

> \[ [Traits](traits.md)
> \| **Collections**
> \| [Referencing](borrow-checker-at-home.md)
> \| [API](api/index.g.md)
> \]

There are three dynamic collections provided by the framework:
[RefList\<T\>](api/T.RefList-1.g.md), [TempRefList\<T\>](api/T.TempRefList-1.g.md)
and [ManagedRefList\<T\>](api/T.ManagedRefList-1.g.md).
All of them have the same public API but use different allocators
and inherit different sets of [trait attributes](traits.md).

| Collection          | Item Constraint | Allocator        | Trait Attributes                        | Burst |
|---------------------|-----------------|------------------|-----------------------------------------|-------|
| `RefList<T>`        | `unmanaged`     | Persistent       | `ExplicitCopy`, `DynSized`, `Dealloc`   | Yes   |
| `TempRefList<T>`    | `unmanaged`     | Temp             | `ExplicitCopy`, `DynSized`, `TempAlloc` | Yes   |
| `ManagedRefList<T>` | `struct`        | Runtime-provided | `ExplicitCopy`, `DynSized`              | No    |

## Data Access Control

ѯ-Framework collections API is provided with extension methods that allow having separate methods with _mutable_ and
_readonly_ access to the data.
It is possible because extension methods can be defined separately for `this ref` and `this in` parameters:

```csharp
// This method is available if you have ANY REFERENCE to the data
public static ref readonly T RefReadonlyAt<T>(this in RefList<T> self, int index)

// This method is available only if you have a MUTABLE REFERENCE to the data
public static ref T RefAt<T>(this ref RefList<T> self, int index)
```

The example above shows that you can get a mutable reference to a collection item
if you have a mutable reference to the collection itself.
But you can get a readonly reference to the item even if you have a mutable reference to the collection.

In practice, it means:

> [!IMPORTANT]
> If you compose your data with ѯ-Framework collections,
> you can control the access to the data by using the appropriate reference type. 

See the full list of extension methods in the API reference:
- [RefList\<T\>](api/T.RefList-1.g.md)
- [TempRefList\<T\>](api/T.TempRefList-1.g.md)
- [ManagedRefList\<T\>](api/T.ManagedRefList-1.g.md)

## Diagnostics

Diagnostics related to usage of the collections:

| Diagnostic Id | Severity | Title                                      |
|---------------|----------|--------------------------------------------|
| `REFLIST01`   | Error    | Generic `[RefList]` type usage is unsafe   |
| `REFLIST02`   | Error    | Jagged `[RefList]` types are not supported |
| `REFLIST03`   | Error    | Non-specialized `[RefList]` API call       |