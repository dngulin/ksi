# DynNoResizeAttribute

Attribute that disallows any resizing operations on a [DynSized] type instance.
Allows getting mutable references to collection items but disallows collection resizing.
Hints the reference lifetime analyzer that any internal buffer cannot be resized.

```csharp
public sealed class DynNoResizeAttribute : Attribute
```
