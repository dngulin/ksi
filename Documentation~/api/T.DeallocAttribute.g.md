# DeallocAttribute

Attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that requires manual deallocation.
Should be added to a struct that contains fields of the `Dealloc` type.
Can be also applied to a generic type parameter to make it compatible with `Dealloc` types.

```csharp
public sealed class DeallocAttribute : Attribute
```
