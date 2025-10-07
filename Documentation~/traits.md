# Trait System

ѯ-Framework defines a set of special attributes (traits) that enable extra diagnostics
and code generation for marked structures:

- Structures that should not be implicitly copied should be marked with the `[ExplicitCopy]` attribute
- Structures that own dynamically allocated data should be marked with the `[DynSized]` attribute
that enables referencing safety checks
- For some `[DynSized]` structures the allocator policy attribute is also required:
  - The `[Dealloc]` attribute generates deallocation API and enables extra diagnostics
  - The `[TempAlloc]` attribute makes deallocation not required in exchange to be stored only on stack

## ExplicitCopyAttribute

The `[ExplicitCopy]` attribute enforces move semantics for a marked struct preventing any implicit copying.
It should be used for structures that have any `[ExplicitCopy]` fields like `RefList<T>`.

For moving ownership use the `Move` extension method:
```
var listA = RefList.Empty<int>();
listA.RefAdd() = 42;
var listB = listA.Move(); // `listA` copied to `listB` and then zeroed
```

The trait also enables code generation that produces explicit copy extensions.

### Generated ExplicitCopy API

Usage of the `[ExplicitCopy]` attribute triggers code generation of `CopyTo` and `CopyFrom` methods
for the marked type and possible containers:

- `TStruct CopyTo(this in TStruct self, ref TStruct other)`
- `TStruct CopyForm(this ref TStruct self, in TStruct other)`
- `void CopyTo(this in TRefList<TStruct> self, ref TRefList<TStruct> other)`
- `void CopyFrom(this ref TRefList<TStruct> self, in TRefList<TStruct> other)`

Where `TStruct` is the structure name and `TRefList` is a [compatible collection](collections.md) name.

> [!WARNING]
> Explicit copy extension methods require access to the structure fields and their types:
> - the structure cannot have private fields
> - the structure cannot be generic

### ExplicitCopy Diagnostics

Diagnostics enabled by the `[ExplicitCopy]` attribute:

| Diagnostic Id | Title                                                   |
|---------------|---------------------------------------------------------|
| `EXPCOPY01`   | Missing `[ExplicitCopy]` attribute                      |
| `EXPCOPY02`   | Passing `[ExplicitCopy]` instance by value              |
| `EXPCOPY03`   | Returning a copy of the `[ExplicitCopy]` instance       |
| `EXPCOPY04`   | Assignment copy of the `[ExplicitCopy]` instance        |
| `EXPCOPY05`   | Defensive copy of the `[ExplicitCopy]` instance         |
| `EXPCOPY06`   | Capturing the `[ExplicitCopy]` instance by closure      |
| `EXPCOPY07`   | Boxing/unboxing the `[ExplicitCopy]` instance           |
| `EXPCOPY08`   | Private filed declaration in the `[ExplicitCopy]` type  |
| `EXPCOPY09`   | Generic `[ExplicitCopy]` type declaration               |
| `EXPCOPY10`   | Passing `[ExplicitCopy]` instance as a generic argument |
| `EXPCOPY11`   | Passing `[ExplicitCopy]` type as a type argument        |
| `EXPCOPY12`   | Using Span copying API with `[ExplicitCopy]` items      |

## DynSizedAttribute

The `[DynSized]` attribute indicates that the structure owns dynamically allocated data.
It should be used for structures that have any `[DynSized]` fields like `RefList<T>`.
And it also requires `[ExplicitCopy]` attribute.

The main purpose of the attribute is to indicate types affected by compile time referencing safety analysis.
For details see the [Referencing Rules](borrow-checker-at-home.md) section.

### DynSized Diagnostics

Diagnostics related to the `[DynSized]` attribute:

| Diagnostic Id | Title                                                  |
|---------------|--------------------------------------------------------|
| `DYNSIZED01`  | Missing `[DynSized]` attribute                         |
| `DYNSIZED02`  | Missing `[ExplicitCopy]` attribute                     |
| `DYNSIZED03`  | Redundant `[DynSized]` attribute                       |
| `DYNSIZED04`  | Resize is not allowed                                  |
| `DYNSIZED05`  | Redundant `[DynNoResize]` attribute                    |
| `DYNSIZED06`  | `[DynSized]` field of a reference type                 |
| `DYNSIZED07`  | Redundant `ExclusiveAccess<T>` usage                   |
| `BORROW01`    | Non-`[RefPath]` reference to `[DynSized]` data         |
| `BORROW02`    | Changing local `[DynSized]` reference is not supported |
| `BORROW03`    | Local reference invalidation                           |
| `BORROW04`    | Reference arguments aliasing                           |
| `BORROW05`    | Reference escapes the access scope                     |

## DeallocAttribute

### Generated Dealloc API

### Dealloc Diagnostics

## TempAllocAttribute

### TempAlloc Diagnostics
