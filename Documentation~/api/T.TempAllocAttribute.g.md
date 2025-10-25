# TempAllocAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / TempAllocAttribute**
> \]

A trait attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that uses `Temp` allocator.
Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.

Heap-allocated `TempAlloc` structures can be allocated only with the `Temp` allocator.
In other words, they can be stored only in the [TempRefList\<T\>](T.TempRefList-1.g.md).
It means that the root `TempAlloc` structure can be stored only on stack similarly to a `ref struct`.

Required for structs that have fields of the `TempAlloc` types.

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TempAllocAttribute : Attribute
```
