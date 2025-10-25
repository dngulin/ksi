# DeallocAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / DeallocAttribute**
> \]

A trait attribute to indicate a [DynSized](T.DynSizedAttribute.g.md)
type that requires manual deallocation.

Should be added to a struct that contains fields of the `Dealloc` type.

Can be also applied to a generic type parameter to make it compatible with `Dealloc` types.

Attribute triggers code generation for deallocation extension methods:
- `(ref TDealloc).Dealloc()` — deallocates all data owned by the struct
- `(ref TDealloc).Deallocated()` — deallocates the struct and returns a reference to it
- `(ref TRefList<TDealloc>).Dealloc()` — deallocates all data owned by the list
- `(ref TRefList<TDealloc>).Deallocated()` — deallocates the list and returns a reference to it
- `(ref TRefList<TDealloc>).RemoveAt(int index)` — deallocates an item and removes it from the list
- `(ref TRefList<TDealloc>).Clear()` — deallocates all items and clears the list

```csharp
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
public sealed class DeallocAttribute : Attribute
```
