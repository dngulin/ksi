# Referencing Rules

> \[ [Traits](traits.md)
> \| [Collections](collections.md)
> \| **Referencing**
> \| [API](api/index.g.md)
> \]

Referencing rules are based on the [DynSized](api/T.DynSizedAttribute.g.md) type usage analysis.
Any change to dynamically sized data can invalidate existing references.

There are two reference invalidation cases:
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

> [!NOTE]
> For analyzers' simplicity reasons,
> reassigning local references derived from `[DynSized]` data is not allowed.

In some cases when mutable access doesn't require resizing any collections,
you can mark the method parameter with the [DynNoResizeAttribute](api/T.DynNoResizeAttribute.g.md).
It will disallow resizing but keep mutable data access.

```csharp
void DoSomething([DynNoResize] ref RefList<int> list)
{
    list.Clear();
//  ^^^^ Error: DYNSIZED04
// It is not possible to resize a parameter that is marked with [DynNoResize]
}
```

## ExclusiveAccess\<T\>

Referencing rules make analyzers' logic quite simple, but it handles only parameters and local symbols.

It is not possible to quickly verify reference compatibility between by-ref arguments and local fields
because it is not possible to check if the argument is derived from the same field or not.
Furthermore, calling non-static methods having a reference to a `[DynSized]` field can also invalidate the reference.

To solve this problem, ѯ-Framework provides the [ExclusiveAccess\<T\>](api/T.ExclusiveAccess-1.g.md) type.
It is a wrapper around the `[DynSized]` data, that guarantees that the owned data is exclusively accessed.

It has two access properties that return mutable and read-only access scopes:
- `public MutableAccessScope<T> Mutable { get; }`
- `public ReadOnlyAccessScope<T> ReadOnly { get; }`

Each access scope is a disposable ref-struct that provides by-ref access to the data:
- `public ref T MutableAccessScope<T>.Value { get; }`
- `public ref readonly T ReadOnlyAccessScope<T>.Value { get; }`

Only one access scope can be active at a time.
Creating a new access scope within other access scope's lifetime throws the `InvalidOperationException`.

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

## RefLike Types Support

Reference safety analyzers support only these `ref struct` types:
- `Span<T>` and `ReadOnlySpan<T>` derived from a `[RefList]` collection
- [MutableAccessScope\<T\>](api/T.MutableAccessScope-1.g.md) and
[ReadOnlyAccessScope\<T\>](api/T.ReadOnlyAccessScope-1.g.md) for top-level data access

It is recommended to use spans only to work with external libraries that are not dependent on the ѯ-Framework.

> [!CAUTION]
> Wrapping references derived from `[DynSized]` data into any other RefLike types can cause memory access errors.

## RefPath

To make the safety checks possible by Roslyn analyzers,
there is a strict requirement for referencing `[DynSized]` data:

> [!IMPORTANT]
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

### RefPath Representation

Internally `RefPath` is composed of:
- `Segments` - array of strings representing the reference path
- `DynSizedLength` - number of segments that point to a `[DynSized]` data
- `ExplicitLength` - number of segments that explicitly point to some data
(number of segments before the first extension method in the path)

Every segment can be one of the following:
- local symbol name (variable or parameter)
- field name
- collection indexer `[n]`
- `[RefPath]` extension method name suffixed with `()`, e.g. `ExtMethodName()`

In the string representation the last `[DynSized]` segment is suffixed with `!` symbol, e.g. `myData.List![n]`.

> [!NOTE]
> Note that `[DynSized]` struct can contain a non-`[DynSized]` data but not vice versa.

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

In some cases it is necessary to get an internal reference applying some logic.
The [RefPathAttribute](api/T.RefPathAttribute.g.md) serves this purpose.

You can use it to mark extension methods that return inner references:
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

In the given example the `CurrentSide()` segment is a _non-explicit segment_.
That means it _can reference any internal data_ of the `state`.

> [!WARNING]
> Extensive usage of non-explicit references can trigger redundant reference compatibility errors
> because the analyzer can compare only explicit parts of paths.

If your extension method returns the same reference every time,
you can avoid that problem by passing the returning path to the `[RefPath]` attribute:

```csharp
[ExplicitCopy, DynSized, Dealloc]
public struct State { public RefList<Item> Items; }

[RefPath("self", "Items", "[n]", "!")]
public static ref RefList<Item> RefItemAt(this ref State self, int idx)
{
    return ref self.Items.RefAt(idx);
}

var state = CreateState();
ref var item = ref state.RefItemAt(42); // state.Items[n]!
```

> [!NOTE]
> When you pass the explicit path to the `[RefPath]` attribute,
> you have to pass a DynSized separator "!" as a segment.