# NonAllocatedResultAttribute

Attribute to indicate a method that returns a reference to
a deallocated instance of the [Dealloc](T.DeallocAttribute.g.md) type.
Allows assigning a new value to the returned reference.

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class NonAllocatedResultAttribute : Attribute
```
