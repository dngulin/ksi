# Referencing Rules

> \[ [Getting Started](getting-started.md)
> \| [Traits](traits.md)
> \| [Collections](collections.md)
> \| **Referencing**
> \| [ECS](ecs.md)
> \| [Serialization](serialization.md)
> \| [API](api/index.g.md)
> \]

Referencing rules are based on the [DynSized](api/T.DynSizedAttribute.g.md) type usage analysis.
Any change to dynamically sized data can invalidate existing references.

There are two reference invalidation cases:
- Passing a mutable reference to `[DynSized]` data while a local reference
  derived from the same data is still active:
    ```csharp
    var list = RefList.Empty<int>();
    PopulateList(ref list);
    
    ref var x = ref list.RefAt(0);
    DoSomething(ref list);
    //              ^^^^ Error: BORROW03
    // It is possible to clear or re-allocate the list within the method,
    // so the `x` variable will point to unreachable or deallocated memory.
    UseValue(in x);
    ```
- Passing a mutable reference to `[DynSized]` data alongside another reference
  derived from (or capable of producing a reference from) the same data:
    ```csharp
    var list = RefList.Empty<int>();
    PopulateList(ref list);
    
    ref var x = ref list.RefAt(0);
    DoSomething(ref list, in x);
    //              ^^^^ Error: BORROW04
    // It is possible to clear or re-allocate the list within the method,
    // so the `x` argument will point to unreachable or deallocated memory.
    ```

> [!NOTE]
> For simplicity in analyzer logic,
> reassigning local references derived from `[DynSized]` data is not allowed.

In cases where mutable access does not require resizing collections,
you can mark a method parameter with the [DynNoResizeAttribute](api/T.DynNoResizeAttribute.g.md).
This will disallow resizing while maintaining mutable data access.

```csharp
void DoSomething([DynNoResize] ref RefList<int> list)
{
    list.Clear();
//  ^^^^ Error: DYNSIZED04
// Resizing a parameter marked with [DynNoResize] is not allowed.
}
```

## ExclusiveAccess\<T\>

Referencing rules keep analyzer logic simple, but they only handle parameters and local symbols.

It is difficult to verify reference compatibility between by-ref arguments and fields
because it is hard to determine if an argument is derived from a specific field.
Furthermore, calling non-static methods while holding a reference to a `[DynSized]` field can also invalidate that reference.

To address this, ѯ-Framework provides the [ExclusiveAccess\<T\>](api/T.ExclusiveAccess-1.g.md) type.
It is a wrapper around `[DynSized]` data that guarantees exclusive access.

It provides two properties that return mutable and read-only access scopes:
- `public MutableAccessScope<T> Mutable { get; }`
- `public ReadOnlyAccessScope<T> ReadOnly { get; }`

Each access scope is a disposable ref struct that provides by-ref access to the data:
- `public ref T MutableAccessScope<T>.Value { get; }`
- `public ref readonly T ReadOnlyAccessScope<T>.Value { get; }`

Only one access scope can be active at a time.
Attempting to create a new access scope within another's lifetime throws an `InvalidOperationException`.

Example:
```csharp
[ExplicitCopy, DynSized, Dealloc]
public struct DynStruct { public RefList<int> List; }

public class DataOwner
{
    private readonly ExclusiveAccess<DynStruct> _data = new ExclusiveAccess<DynStruct>();
    
    public void IncrementFirstItem()
    {
        using var data = _data.Mutable;
        
        ref var x = data.Value.List.RefAt(0);
        DeallocList(); // Throws InvalidOperationException
        x++;
    }
    
    private void DeallocList()
    {
        using var data = _data.Mutable;
        data.Value.List.Dealloc();
    }
}
```

## Ref-like Types Support

Reference safety analyzers support only these `ref struct` types:
- `Span<T>` and `ReadOnlySpan<T>` derived from a `TRefList<T>`.
- [MutableAccessScope\<T\>](api/T.MutableAccessScope-1.g.md) and
[ReadOnlyAccessScope\<T\>](api/T.ReadOnlyAccessScope-1.g.md) for top-level data access.

It is recommended to use spans only when working with external libraries that do not depend on ѯ-Framework.

> [!CAUTION]
> Wrapping references derived from `[DynSized]` data into any other ref-like types can cause memory access errors.

## RefPath

To enable safety checks via Roslyn analyzers,
there is a strict requirement for referencing `[DynSized]` data:

> [!IMPORTANT]
> If an expression returns a reference and also references `[DynSized]` data,
> it must be a `RefPath`-compatible expression.

A valid `RefPath` expression must be composed of:
- Local variable references
- Parameter references
- Field references (except `static` and `this` fields)
- `TRefList<T>` API:
  - Indexing extension methods: `RefAt`, `RefReadonlyAt`, `RefAdd`
  - Span representation methods: `AsSpan`, `AsReadOnlySpan`
  - Iterator items produced by: `RefIter`, `RefIterReversed`, `AsSpan`, `AsReadOnlySpan`
- `(ReadOnly)Span<T>` indexers and `Slice` methods
- `(ReadOnly)AccessScope<T>.Value` property
- `[RefPath]` extension methods

### RefPath Representation

Internally, a `RefPath` is composed of:
- `Segments` — an array of strings representing the reference path.
- `DynSizedLength` — the number of segments pointing to `[DynSized]` data.
- `ExplicitLength` — the number of segments explicitly pointing to data
  (the number of segments before the first extension method in the path).

Each segment can be one of the following:
- A local symbol name (variable or parameter).
- A field name.
- A collection indexer `[n]`.
- A `[RefPath]` extension method name suffixed with `()`, e.g., `ExtMethodName()`.

In string representation, the last `[DynSized]` segment is suffixed with the `!` symbol, e.g., `myData.List![n]`.

> [!NOTE]
> A `[DynSized]` struct can contain non-`[DynSized]` data, but not vice versa.

Examples:
```csharp
[ExplicitCopy, DynSized, Dealloc]
public struct DynStruct { public RefList<int> Numbers; }

public struct NonDynStruct { public int Number; }

[ExplicitCopy, DynSized, Dealloc]
public struct RootStruct
{
    public RefList<DynStruct> ListOfDyn;
    public RefList<NonDynStruct> ListOfNonDyn;
}

var root = new RootStruct();

ref var a = ref root.ListOfDyn.RefAt(0); // root.ListOfDyn[n]!
ref var b = ref a.Numbers; // root.ListOfDyn[n].Numbers!
ref var c = ref b.RefAt(0); // root.ListOfDyn[n].Numbers![n]

ref var x = ref root.ListOfNonDyn[0]; // root.ListOfNonDyn![n]
ref var y = ref x.Number; // root.ListOfNonDyn![n].Number
```

### RefPathAttribute

In some cases, it is necessary to get an internal reference using custom logic.
The [RefPathAttribute](api/T.RefPathAttribute.g.md) serves this purpose.

You can use it to mark extension methods that return internal references:
```csharp
[ExplicitCopy, DynSized, Dealloc]
public struct State
{
    public RefList<Item> Left;
    public RefList<Item> Right;
}

[RefPath]
public static ref RefList<Item> CurrentSide(this ref State self, int turn)
{
    if (turn % 2 == 0)
        return ref self.Left;

    return ref self.Right;
}

var state = CreateState();
ref var items = ref state.CurrentSide(turn); // state.CurrentSide()!
ProcessItems(ref items);
```

In this example, the `CurrentSide()` segment is a _non-explicit segment_.
This means it _can reference any internal data_ of the `state`.

> [!WARNING]
> Excessive use of non-explicit references can trigger redundant reference compatibility errors
> because the analyzer can only compare explicit parts of paths.

If your extension method returns the same reference every time,
you can avoid this problem by passing the return path to the `[RefPath]` attribute:

```csharp
[ExplicitCopy, DynSized, Dealloc]
public struct State { public RefList<Item> Items; }

[RefPath("self", "Items", "[n]", "!")]
public static ref Item RefItemAt(this ref State self, int idx)
{
    return ref self.Items.RefAt(idx);
}

var state = CreateState();
ref var item = ref state.RefItemAt(42); // state.Items[n]!
```

> [!NOTE]
> When passing an explicit path to the `[RefPath]` attribute,
> you must include the DynSized separator "!" as a segment.