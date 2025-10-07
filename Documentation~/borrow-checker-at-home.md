# Referencing Rules

Referencing rules are based on the `[DynSized]` type usage analysis.
Any change to dynamically sized data can invalidate existing references.

There are two reference safety cases:
- Passing a mutable reference to `[DynSized]` data within the lifetime
of a local reference that is derived from the same data:
    ```csharp
    var list = RefList.Empty<int>();
    PopulateList(ref list);
    
    ref var x = ref list.RefAt(0);
    DoSomething(ref list);
    //              ^^^^ Error: BORROW03
    // It is possible to clear or re-allcoate the list within the method,
    // so the `x` varibale will point to unreachable or deallcoated memory
    UseValue(in x);
    ```
- Passing a mutable reference to `[DynSized]` data alongside a reference
that is derived from the same data or can produce a reference derived from the same data:
    ```csharp
    var list = RefList.Empty<int>();
    PopulateList(ref list);
    
    ref var x = ref list.RefAt(0);
    DoSomething(ref list, in x);
    //              ^^^^ Error: BORROW04
    // It is possible to clear or re-allcoate the list within the method,
    // so the `x` argument will point to unreachable or deallcoated memory
    ```

In some cases when mutable access doesn't require resizing any collections,
you can mark the method parameter with the `[DynNoResize]` attribute.
It will disallow resizing but keep mutable data access.

```csharp
void DoSomething([DynNoResize] ref RefList<int> list) {
    list.Clear();
//  ^^^^ Error: DYNSIZED04
// It is not possible to resize a parameter that is marked with [DynNoResize]
}
```

## ExclusiveAccess\<T\>

TBD

## RefPath

To make the safety checks possible by Roslyn analyzers,
there is a strict requirement for referencing `[DynSized]` data:

> If expression returns a reference and also references a `[DynSized]` data,
> it should be a `RefPath`-compatible expression.

A valid `RefPath` expression should be composed of:
- Local variable reference
- Parameter reference
- Field reference (except `static` and `this` fields)
- `[RefList]` API calls:
  - Indexing extension methods: `RefAt`, `ReferadOnlyAt`, `RefAdd`
  - Span representation methods: `AsSpan`, `AsReadOnlySpan`
  - Iterator items produced by: `RefIter`, `RefIterReversed`, `AsSpan`, `AsReadOnlySpan`
- `(ReadOnly)Span<T>` indexers and `Slcie` methods
- `(ReadOnly)AccessScope<T>.Value` property
- `[RefPath]` extension methods

### RefPath Items

TBD

### RefPathAttribute

TBD