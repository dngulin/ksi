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

### Generated API

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

### Diagnostics

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

See also the [Referencing Rules](borrow-checker-at-home.md) section.

### DynNoResizeAttribute

### Diagnostics

## DeallocAttribute

### Generated API

### Diagnostics

## TempAllocAttribute

### Diagnostics
