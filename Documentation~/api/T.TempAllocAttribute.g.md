# TempAllocAttribute

Attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type
that uses temporary allocator and should be created only on stack.
Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.
Should be added to a struct that contains fields of the `TempAlloc` type.

```csharp
public sealed class TempAllocAttribute : Attribute
```
