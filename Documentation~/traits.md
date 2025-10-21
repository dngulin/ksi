# Trait Attributes

> \[ **Traits**
> \| [Collections](collections.md)
> \| [Referencing](borrow-checker-at-home.md)
> \| [ECS](ecs.md)
> \| [API](api/index.g.md)
> \]

ѯ-Framework defines a set of special attributes (traits) that enable extra diagnostics
and code generation for marked structures:

- Structures that should not be implicitly copied should be marked with
the [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md)
- Structures that own dynamically allocated data should be marked with
the [DynSizedAttribute](api/T.DynSizedAttribute.g.md) that enables referencing safety checks
- For some `[DynSized]` structures the allocator policy trait is also required:
  - The [DeallocAttribute](api/T.DeallocAttribute.g.md) generates deallocation API and enables extra diagnostics
  - The [TempAllocAttribute](api/T.TempAllocAttribute.g.md) makes deallocation not required
  in exchange to be stored only on stack

## ExplicitCopy Attribute

The [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md) enforces move semantics for a marked struct
preventing any implicit copying.
It should be used for structures that have any `[ExplicitCopy]` fields like [RefList\<T\>](api/T.RefList-1.g.md).

Usage example:
```csharp
[ExplicitCopy]
public struct ChildStruct
{
    public int UniqueId;
}

[ExplicitCopy] // <--- required because of the `ChildStruct` field
public struc ParentStruct
{
    public ChildStruct Child;
}
```

For moving ownership use the `Move` extension method:
```csharp
var listA = RefList.Empty<int>();
listA.RefAdd() = 42;
var listB = listA.Move(); // `listA` copied to `listB` and then zeroed
```

The trait also enables code generation that produces explicit copy extensions.

### Generated ExplicitCopy API

Usage of the [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md) triggers
code generation of `CopyTo` and `CopyFrom` methods for the marked type and possible containers:

- `TStruct CopyTo(this in TStruct self, ref TStruct other)`
- `TStruct CopyForm(this ref TStruct self, in TStruct other)`
- `void CopyTo(this in TRefList<TStruct> self, ref TRefList<TStruct> other)`
- `void CopyFrom(this ref TRefList<TStruct> self, in TRefList<TStruct> other)`

Where `TStruct` is the structure name and `TRefList` is a [compatible collection](collections.md) name.

> [!IMPORTANT]
> Explicit copy extension methods require access to the structure fields and their types:
> - the structure cannot have private fields
> - the structure cannot be generic

### ExplicitCopy Diagnostics

Diagnostics related to the [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md):

| Diagnostic Id | Severity | Title                                                    |
|---------------|----------|----------------------------------------------------------|
| `EXPCOPY01`   | Error    | Missing `[ExplicitCopy]` attribute                       |
| `EXPCOPY02`   | Error    | Passing `[ExplicitCopy]` instance by value               |
| `EXPCOPY03`   | Error    | Returning a copy of the `[ExplicitCopy]` instance        |
| `EXPCOPY04`   | Error    | Assignment copy of the `[ExplicitCopy]` instance         |
| `EXPCOPY05`   | Error    | Defensive copy of the `[ExplicitCopy]` instance          |
| `EXPCOPY06`   | Error    | Capturing the `[ExplicitCopy]` instance by closure       |
| `EXPCOPY07`   | Error    | Boxing/unboxing the `[ExplicitCopy]` instance            |
| `EXPCOPY08`   | Error    | Private field declaration in the `[ExplicitCopy]` type   |
| `EXPCOPY09`   | Error    | Generic `[ExplicitCopy]` type declaration                |
| `EXPCOPY10`   | Error    | Passing `[ExplicitCopy]` instance as a generic argument  |
| `EXPCOPY11`   | Error    | Passing `[ExplicitCopy]` type as a type argument         |
| `EXPCOPY12`   | Error    | Using Span copying API with `[ExplicitCopy]` items       |
| `EXPCOPY13`   | Error    | Declaring `[ExplicitCopy]` struct with low accessibility |

## DynSized Attribute

The [DynSizedAttribute](api/T.DynSizedAttribute.g.md) indicates that the structure owns dynamically allocated data.
It should be used for structures that have any `[DynSized]` fields like [RefList\<T\>](api/T.RefList-1.g.md).
And it also requires [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md).

The main purpose of the attribute is to indicate types affected by compile time referencing safety analysis.
For details see the [Referencing Rules](borrow-checker-at-home.md) section.

Usage example:
```csharp
[ExplicitCopy, DynSized] // <--- required because of the `ManagedRefList<int>` field
public struct ChildStruct
{
    public ManagedRefList<int> Numbers;
}

[ExplicitCopy, DynSized] // <--- required because of the `ChildStruct` field
public struc ParentStruct
{
    public ChildStruct Child;
}
```

### DynSized Diagnostics

Diagnostics related to the [DynSizedAttribute](api/T.DynSizedAttribute.g.md):

| Diagnostic Id | Severity | Title                                                  |
|---------------|----------|--------------------------------------------------------|
| `DYNSIZED01`  | Error    | Missing `[DynSized]` attribute                         |
| `DYNSIZED02`  | Error    | Missing `[ExplicitCopy]` attribute                     |
| `DYNSIZED03`  | Warning  | Redundant `[DynSized]` attribute                       |
| `DYNSIZED04`  | Error    | Resize is not allowed                                  |
| `DYNSIZED05`  | Warning  | Redundant `[DynNoResize]` attribute                    |
| `DYNSIZED06`  | Error    | `[DynSized]` field of a reference type                 |
| `DYNSIZED07`  | Warning  | Redundant `ExclusiveAccess<T>` usage                   |
| `BORROW01`    | Error    | Non-`[RefPath]` reference to `[DynSized]` data         |
| `BORROW02`    | Error    | Changing local `[DynSized]` reference is not supported |
| `BORROW03`    | Error    | Local reference invalidation                           |
| `BORROW04`    | Error    | Reference arguments aliasing                           |
| `BORROW05`    | Error    | Reference escapes the access scope                     |

## Dealloc Attribute

The [DeallocAttribute](api/T.DeallocAttribute.g.md) indicates a type that should be deallocated
with the `Dealloc` extension method. It requires [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md)
and should be used for structures that have any `[Dealloc]` fields like [RefList\<T\>](api/T.RefList-1.g.md).

Usage example:
```csharp
[ExplicitCopy, DynSized, Dealloc] // <--- required because of the `RefList<int>` field
public struct ChildStruct
{
    public RefList<int> Numbers;
}

[ExplicitCopy, DynSized, Dealloc] // <--- required because of the `ChildStruct` field
public struc ParentStruct
{
    public ChildStruct Child;
}
```

### Generated Dealloc API

Usage of the [DeallocAttribute](api/T.DeallocAttribute.g.md) attribute triggers
code generation of `Dealloc` and `Deallocated` methods for the marked type:

- `void Dealloc(this ref TStruct self)` - deallocates all data owned by the struct
- `ref TStruct Deallocated(this ref TStruct self)` - deallocate the struct and returns it as
an _assignable_ reference (see the `DEALLOC04` diagnostic message)

It also generates specialized API calls for [compatible collections](collections.md):
- `void Dealloc(this in TRefList<TStruct> self)` - specialized version that deallocate collection items
- `ref TRefList Deallocated(this ref TRefList<TStruct> self)` - wrapper around the specialized `Dealloc` extension
- `void RemoveAt(this ref TRefList<TStruct> self, int index)` - specialized version that deallocate collection items
- `void Clear(this ref TRefList<TStruct> self)` - specialized version that deallocate collection items

> [!NOTE]
> The `Dealloc` extension method is also generated for collections
> that don't require deallocation like `TempRefList<T>`.
> In that case collection items are deallocated, but the collection itself is not cleared

### NonAllocatedResult Attribute

If you are sure that your method returns a deallocated instance, you can use
the [NonAllocatedResultAttribute](api/T.NonAllocatedResultAttribute.g.md) to suppress the `DEALLOC04` diagnostic.

> [!CAUTION]
> There is no diagnostic analyzer to verify if the instance is actually deallocated.
> Incorrect usage of the attribute can cause memory leaks due to missing deallocation.


Example:
```csharp
var list = RefList.Empty<Entity>();
list.RefAdd() = CreateRandomEntity(); // `RefAdd` has the `[NonAllocatedResult]` attribute

list.RefAt(0) = CreateRandomEntity(); // <--- Error: DEALLOC04
list.RefAt(0).Deallocated() = CreateRandomEntity(); // Assignment is allowed
```

### Dealloc Diagnostics

Diagnostics related to the [DeallocAttribute](api/T.DeallocAttribute.g.md):

| Diagnostic Id | Severity | Title                                              |
|---------------|----------|----------------------------------------------------|
| `DEALLOC01`   | Error    | Missing `[Dealloc]` attribute                      |
| `DEALLOC02`   | Error    | Missing `[ExplicitCopy]` attribute                 |
| `DEALLOC03`   | Warning  | Redundant `[Dealloc]` attribute                    |
| `DEALLOC04`   | Error    | Overwriting `[Dealloc]` instance                   |
| `DEALLOC05`   | Error    | Unused `[Dealloc]` instance                        |
| `DEALLOC06`   | Error    | Passing `[Dealloc]` instance as a generic argument |

## TempAlloc Attribute

The [TempAllocAttribute](api/T.TempAllocAttribute.g.md) indicates a type that should be stored only on stack.
It requires [ExplicitCopyAttribute](api/T.ExplicitCopyAttribute.g.md) and should be used for structures that have
any `[TempAlloc]` fields like [TempRefList\<T\>](api/T.TempRefList-1.g.md).

`[TempAlloc]` types are similar to `ref struct` types,
but can be used as generic parameters of `[TempAlloc]` collections.

Usage example:
```csharp
[ExplicitCopy, DynSized, TempAlloc] // <--- required because of the `TempRefList<int>` field
public struct ChildStruct
{
    public TempRefList<int> Numbers;
}

[ExplicitCopy, DynSized, TempAlloc] // <--- required because of the `ChildStruct` field
public struc ParentStruct
{
    public ChildStruct Child;
}
```

### TempAlloc Diagnostics

Diagnostics related to the [TempAllocAttribute](api/T.TempAllocAttribute.g.md):

| Diagnostic Id  | Severity | Title                                              |
|----------------|----------|----------------------------------------------------|
| `TEMPALLOC01`  | Error    | Missing `[TempAlloc]` attribute                    |
| `TEMPALLOC02`  | Error    | Missing `[ExplicitCopy]` attribute                 |
| `TEMPALLOC03`  | Warning  | Redundant `[TempAlloc]` attribute                  |
| `TEMPALLOC04`  | Error    | Incompatible allocator with the `[TempAlloc]` type |