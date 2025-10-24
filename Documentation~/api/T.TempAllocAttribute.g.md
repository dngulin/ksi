# TempAllocAttribute

> \[ [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / TempAllocAttribute**
> \]

A trait attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that uses temporary allocator.
Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.

Should be stored only on stack, that makes it similar to `ref` structures.
The main difference is that you can use it as a generic parameter of the [TempRefList\<T\>](T.TempRefList-1.g.md).

Required for structs that have fields of the `TempAlloc` type.

```csharp
[AttributeUsage(AttributeTargets.Struct)]
public sealed class TempAllocAttribute : Attribute
```
