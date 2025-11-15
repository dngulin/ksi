# NonAllocatedResultAttribute

> \[ [Getting Started](getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **[API](index.g.md) / NonAllocatedResultAttribute**
> \]

An attribute to mark a method returning a deallocated [Dealloc](T.DeallocAttribute.g.md) type reference.
Allows assigning a new value to the returned reference.

Attribute usage is not verified by roslyn analyzers.
Returning a non-deallocated instance can cause memory leaks.

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class NonAllocatedResultAttribute : Attribute
```
