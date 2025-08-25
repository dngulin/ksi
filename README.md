# RefList Collections

This package provides three collections useful in data oriented designs:

- `RefList` - wraps a c# array of structs
- `NativeRefList` - wraps a natively allocated buffer of unmanaged structs
- `TempRefList` - wraps a natively allocated buffer of unmanaged structs that uses the `TempAllocator`

Key features:

- Move semantics
- Separate API for readonly and writable access
- By-ref indexers
- Hierarchical deallocation (native collections)
- Compatibility with `Burst` (native collections)

The package heavily relies on Roslyn analyzers and code generators to avoid some memory usage issues.

> [!WARNING]
> The project is on the prototype development stage:
> - API is not yet stabilized and not documented
> - Test coverage is very basic

## Move Semantics

`RefList` structure variants are annotated with the `[NoCopy]` attribute and depend on the `NoCopyAnalyzer`,
that prevents them from being passed/received  by value in most of the cases.

This is the core memory safety component that enforces to have only one instance of the structure.
So, any operation that modifies the internal collection state (memory buffer, item count and capacity)
implies that the only one instance of the structure is present.

For details see the [NoCopyAnalyzer](https://github.com/dngulin/NoCopyAnalyzer) project.

## Separate Readonly and Writable API

The package follows the approach used in the [PlainBuffers](https://github.com/dngulin/PlainBuffers) library:
*API is provided as extension methods*.

It allows to provide mutable API methods only for mutable references.
See an example:
```csharp
public static void Setter(this ref MyStruct self, int value) { ... }
public static int Getter(this in MyStruct self) { ... }

public static void SomeMethod(ref MyStruct rwInstance, in MyStruct roInstance)
{
    rwInstance.Setter(42);
    var value1 = rwInstance.Getter();

    // roInstance.Setter() is not accessible in this scope
    // You can only call the getter
    var value2 = roInstance.Getter();

    ...
}
```

So, all mutating `RefList` methods are available only if you have a mutable reference:
```csharp
public static int Capacity<T>(this in NativeRefList<T> self) where T : unmanaged { ... }
public static int Count<T>(this in NativeRefList<T> self) where T : unmanaged { ... }

public static void Add<T>(this ref NativeRefList<T> self, in T item) where T : unmanaged { ... }
public static void Clear<T>(this ref NativeRefList<T> self) where T : unmanaged { ... }
```

The same logic is applied for internal access. It is not possible to provide standard indexers (`[]` operator)
as extensions so they are implemented as extension methods:
```csharp
public static ref readonly T RefReadonlyAt<T>(this in NativeRefList<T> self, int index) where T : unmanaged { ... }
public static ref T RefAt<T>(this ref NativeRefList<T> self, int index) where T : unmanaged { ... }
```

## Hierarchical Deallocation

The package also provides the Roslyn analyzer and source generator pair to enforce deallocation pattern:
- A structure that is marked with the `[DeallocApi]` attribute has the generated `Dealloc` extension method
- A structure that has any field providing dealloc API should be marked with the `[DeallocApi]` attribute

For example:
- if you have a `NativeRefList` field in the `StructureA`, you should mark it with the `[DeallocApi]` attribute
- if you have a `StructureA` field in the `StructureB`, you should mark it with the `[DeallocApi]` attribute too
- the `StructureB.Dealloc` extension deallocate the `StructureA` field,
that deallocate the `NativeRefList` field of the `StructureA`

So, it just generates all boilerplate deallocation code for the root data structure and enforces that pattern.

## Burst Compatibility

The main goal of the library is to provide a convenient API to implement
a burst-compatible data-oriented game logic:

```csharp
[BurstCompile]
public static class GameLogic
{
    [BurstCompile]
    public static void Tick(in Specs specs, ref GameState state, ref FrameState frameState)
    {
        foreach (ref var entity in state.Entities.RefIter())
        {
            var modifier = specs.Entities.RefReadonlyAt(entity.SpecId).Modifier;
            entity.Position += entity.Velocity * frameState.DeltaTime * modifier;
        }
    }
}
```

In the given example:
- `specs`  represents the immutable data (things like a default weapon damage)
- `state` represents the mutable data (things like a player position)
- `frameState` represents the short lived data required for one tick simulation (like player inputs or event list)
