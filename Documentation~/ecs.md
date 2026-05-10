# ECS-Like Data Composition

> \[ [Getting Started](getting-started.md)
> \| [Traits](traits.md)
> \| [Collections](collections.md)
> \| [Referencing](borrow-checker-at-home.md)
> \| **ECS**
> \| [API](api/index.g.md)
> \]

ECS is a common approach for laying out and processing data in data-oriented designs.
The idea is to represent data as collections of entities, which in turn are sets of components.
Data is then processed by querying sets of components from these entities.

ѯ-Framework provides several attributes to facilitate this pattern:
- [KsiComponent](api/T.KsiComponentAttribute.g.md) — Marks a structure as a component type.
- [KsiEntity](api/T.KsiEntityAttribute.g.md) and [KsiArchetype](api/T.KsiArchetypeAttribute.g.md) —
  Mark entities using `ArrayOfStructs` (AoS) or `StructOfArrays` (SoA) data layouts.
- [KsiDomain](api/T.KsiDomainAttribute.g.md) — Marks a root structure that stores entities.
- [KsiQuery](api/T.KsiQueryAttribute.g.md) — Marks a method that processes sets of components.
  It generates a wrapper method that queries components from the corresponding `KsiDomain`.

These attributes provide the necessary context for the query code generator.

## Data Layout

### Components

The lowest level of data composition is represented by `KsiComponent` structures.
Example:
```csharp
[KsiComponent]
public struct Component
{
    public int SomeData;
}
```

### Entities

The intermediate composition level is represented by collections of entities.
An entity must be composed of unique, public component fields.

There are two ways to represent entities within a domain:
- As an array of entities: `Entity[]` where `Entity { ComponentA, ComponentB, ... }`.
- As a structure of component arrays: `Archetype { ComponentA[], ComponentB[], ... }` where
  an `entity` is represented as a vertical slice across component arrays.

The first approach is more intuitive, but the second is often more CPU-cache-friendly.

[Entity](api/T.KsiEntityAttribute.g.md) example:
```csharp
[KsiEntity]
public struct Entity
{
    public ComponentA A;
    public ComponentB B;
}
```

[Archetype](api/T.KsiArchetypeAttribute.g.md) example:
```csharp
[KsiArchetype]
[ExplicitCopy, DynSized, Dealloc]
public struct Archetype
{
    public RefList<ComponentA> A;
    public RefList<ComponentB> B;
}
```

To keep all component arrays in a consistent state, helper extension methods are generated:
- `(in TArchetype).Count()` — gets the entity count.
- `(ref TArchetype).AppendDefault(int count)` — adds a specified number of `default` components to each inner list.
- `(ref TArchetype).RemoveAt(int index)` — removes the entity at the given index.
- `(ref TArchetype).Clear()` — clears all inner lists.

### Domains

The top composition level is represented by a domain — a structure that stores entities.
A domain should be composed of `KsiArchetype` and arrays of `KsiEntity` fields.

Example:
```csharp
[KsiArchetype]
[ExplicitCopy, DynSized, Dealloc]
public partial struct Domain
{
    public RefList<Entity> AoS;
    public Archetype SoA;
}
```

Marking a structure with the [KsiDomain](api/T.KsiDomainAttribute.g.md) attribute generates inner addressing types.

Example for a domain declared above:
```csharp
public partial struct Domain
{
    public enum KsiSection
    {
        AoS = 1,
        SoA = 2
    }
    
    public struct KsiHandle
    {
        public KsiSection Section;
        public int Index;
    
        public KsiHandle(KsiSection section, int index)
        {
            Section = section;
            Index = index;
        }
    }
}
```

> [!TIP]
> You can split entities into logical groups by storing them in separate fields of the same type.  

## Data Processing

To iterate over sets of components in a domain,
mark the processing method with the [KsiQuery](api/T.KsiQueryAttribute.g.md) attribute.
This will generate a wrapper method that queries components from the corresponding domain.

Example:
```csharp
public partial class ExampleSystem
{
    [KsiQuery]
    private static void Tick(
        in Domain.KsiHandle handle,
        ref ComponentA a,
        ref ComponentB b,
        [KsiQueryParam] ref CustomData data
    )
    {
        // Modify components here
    }
}
```

A public wrapper is generated that executes the query method for matching entity arrays:
```csharp
public partial class ExampleSystem
{
    public static void Tick(ref Domain domain, ref CustomData data)
    {
        var handle = new Domain.KsiHandle(Domain.KsiSection.AoS, 0);
        for (handle.Index = 0; handle.Index < domain.AoS.Count(); handle.Index++)
        {
            ref var entity = ref domain.AoS.RefAt(handle.Index);
            Tick(in handle, ref entity.A, ref entity.B, ref data);
        }
        
        handle.Section = Domain.KsiSection.SoA;
        for (handle.Index = 0; handle.Index < domain.SoA.Count(); handle.Index++)
        {
            ref var archetype = ref domain.SoA;
            Tick(in handle, ref archetype.A.RefAt(handle.Index), ref archetype.B.RefAt(handle.Index), ref data);
        }
    }
}
```

As shown above, the `KsiHandle` structure is initialized with the current entity's address
and can be used for referencing.

Additionally, the [KsiQueryParam](api/T.KsiQueryParamAttribute.g.md) attribute
is used to pass custom data to the query method.

## Diagnostics

Diagnostic messages related to ECS-like data composition:

| Diagnostic Id | Severity | Title                                              |
|---------------|----------|----------------------------------------------------|
| `KSICOMP01`   | Error    | Invalid field of data composition type             |
| `KSICOMP02`   | Error    | Repeated entity component                          |
| `KSICOMP03`   | Error    | Invalid `[KsiDomain]` declaration                  |
| `KSICOMP04`   | Error    | Invalid `[KsiArchetype]` accessibility             |
| `KSICOMP05`   | Error    | Non-top-level partial type containing `[KsiQuery]` |
| `KSICOMP06`   | Error    | Invalid `[KsiQuery]` method signature              |
| `KSICOMP07`   | Error    | Non-reference `[KsiQuery]` method parameter        |
| `KSICOMP08`   | Error    | Invalid `[KsiQuery]` method parameter type         |