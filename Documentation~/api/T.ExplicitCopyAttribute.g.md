# ExplicitCopyAttribute

Attribute that forbids structure implicit copying and provides explicit copy extension methods.
Can be applied only to POD types without any methods and private fields.
Should be added to a struct that contains fields of `ExplicitCopy` type.
Can be also applied to a generic type parameter to make it compatible with `ExplicitCopy` types.

```csharp
public sealed class ExplicitCopyAttribute : Attribute
```
