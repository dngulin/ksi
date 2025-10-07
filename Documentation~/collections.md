# Collections

There are three dynamic collections provided by the framework:
`RefList<T>`, `TempRefList<T>` and `ManagedRefList<T>`.
All of them have the same public API but use different allocators
and inherit different sets of [trait attributes](traits.md).

| Collection          | Item Constraint | Allocator        | Trait Attributes                        | Burst |
|---------------------|-----------------|------------------|-----------------------------------------|-------|
| `RefList<T>`        | `unmanaged`     | Persistent       | `ExplicitCopy`, `DynSized`, `Dealloc`   | Yes   |
| `TempRefList<T>`    | `unmanaged`     | Temp             | `ExplicitCopy`, `DynSized`, `TempAlloc` | Yes   |
| `ManagedRefList<T>` | `struct`        | Runtime-provided | `ExplicitCopy`, `DynSized`              | No    |

## Public API

TBD

## Diagnostics

Diagnostics related to usage of the collections:

| Diagnostic Id | Severity | Title                                      |
|---------------|----------|--------------------------------------------|
| `REFLIST01`   | Error    | Generic `[RefList]` type usage is unsafe   |
| `REFLIST02`   | Error    | Jagged `[RefList]` types are not supported |
| `REFLIST03`   | Error    | Non-specialized `[RefList]` API call       |