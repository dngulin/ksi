# ECS-Like Data Composition

> \[ [Traits](traits.md)
> \| [Collections](collections.md)
> \| [Referencing](borrow-checker-at-home.md)
> \| **ECS**
> \| [API](api/index.g.md)
> \]

ECS is a typical approach to lay out and process your data in data-oriented designs.
The idea is to represent the data as arrays of entities, which in turn are sets of components.
The data is processed by querying sets of components from entities.

ѯ-Framework provides attributes to compose and process your data this way:
- [KsiComponent](api/T.KsiComponentAttribute.g.md) — an attribute to mark your component type.
- [KsiEntity](api/T.KsiEntityAttribute.g.md) and [KsiArchetype](api/T.KsiArchetypeAttribute.g.md) —
attributes to mark yor entities with `ArrayOfStructs` and `StructOfArrays` data layouts
- [KsiDomain](api/T.KsiDomainAttribute.g.md) — an attribute to mark a root structure that stores entities
- [KsiQuery](api/T.KsiQueryAttribute.g.md) — an attribute to mark a method that processes sets of components.
It produces a wrapper method that queries components from the corresponding `KsiDomain`

All data-marking attributes are required only to provide context for the query code generator.

## Data Layout

### Components

Lower data composition level is represented by `KsiComponent` structures.
Example:
```csharp
[KsiComponent]
public struct Component
{
    public int SomeData;
}
```

### Entities

The intermediate composition level is represented by arrays of entities.
An entity should be composed of non-repeated public component fields.

There are two ways to represent entities within the domain:
- As an array of entities: `Entity[]` where `Entity { ComponentA, ComponentB, .. }`
- As a structure of component arrays: `Archetype { ComponentA[], ComponentB[], .. }` where
an `entity` is represented as a vertical slice of component arrays

The first approach is more intuitive, but it is less CPU-cache-friendly.

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

To keep all component arrays in a consistent state, helper extension methods are produced:
- `(in T).Count()` — gets entity count
- `(ref T).AppendDefault(int)` — adds a specified number of `default` components to each inner list
- `(ref T).RemoveAt(int)` — removes an entity at the given index
- `(ref T).Clear()` — clears all inner lists

### Domains

The top composition level is represented by a domain — structure that stores entities.
Domain should be composed of `KsiArchetype` and arrays of `KsiEntity` fields.

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

To iterate over component sets in the domain,
mark the processing method with the [KsiQuery](api/T.KsiQueryAttribute.g.md) attribute.
It will generate a wrapper method that queries components from the corresponding domain.

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

It will generate a public wrapper that executes the query method for matching entity arrays:
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

As you can see, the `KsiHandle` structure is initialized with the current entity address,
and can be used for referencing.

Also, note that the [KsiQueryParam](api/T.KsiQueryParamAttribute.g.md) attribute
is used to pass custom data to the query method.