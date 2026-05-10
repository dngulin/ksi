# API Reference

> \[ [Getting Started](../getting-started.md)
> \| [Traits](../traits.md)
> \| [Collections](../collections.md)
> \| [Referencing](../borrow-checker-at-home.md)
> \| [ECS](../ecs.md)
> \| **API**
> \]

The API Reference is generated from XML documentation comments.

Extension methods are grouped together with their target types.


## Attributes

- [ExplicitCopyAttribute](T.ExplicitCopyAttribute.g.md) — a trait attribute that forbids structure implicit copying
- [DynSizedAttribute](T.DynSizedAttribute.g.md) — an attribute to indicate an [ExplicitCopy](T.ExplicitCopyAttribute.g.md) type that contains a dynamically sized buffer
- [DynNoResizeAttribute](T.DynNoResizeAttribute.g.md) — an attribute to disallow resizing operations on a [DynSized](T.DynSizedAttribute.g.md) parameter
- [DeallocAttribute](T.DeallocAttribute.g.md) — a trait attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that requires manual deallocation
- [NonAllocatedResultAttribute](T.NonAllocatedResultAttribute.g.md) — an attribute to mark a method returning a deallocated [Dealloc](T.DeallocAttribute.g.md) type reference
- [TempAllocAttribute](T.TempAllocAttribute.g.md) — a trait attribute to indicate a [DynSized](T.DynSizedAttribute.g.md) type that uses `Temp` allocator
- [RefPathAttribute](T.RefPathAttribute.g.md) — a hint attribute for the reference path analyzer
- [KsiHashTableAttribute](T.KsiHashTableAttribute.g.md) — an attribute to mark a hashtable-based collection
- [KsiHashTableSlotAttribute](T.KsiHashTableSlotAttribute.g.md) — an attribute to mark a hash table slot type
- [KsiComponentAttribute](T.KsiComponentAttribute.g.md) — an attribute to mark a component type
- [KsiEntityAttribute](T.KsiEntityAttribute.g.md) — an attribute to mark an entity type
- [KsiArchetypeAttribute](T.KsiArchetypeAttribute.g.md) — an attribute to mark a type that represents a sequence of entities
- [KsiDomainAttribute](T.KsiDomainAttribute.g.md) — an attribute to mark a domain that can be queried by [KsiQuery](T.KsiQueryAttribute.g.md) methods
- [KsiQueryAttribute](T.KsiQueryAttribute.g.md) — an attribute to produce the `ECS`-like query
- [KsiQueryParamAttribute](T.KsiQueryParamAttribute.g.md) — an attribute to mark a [KsiQuery](T.KsiQueryAttribute.g.md) parameter
- [KsiSerializableAttribute](T.KsiSerializableAttribute.g.md) — marks a struct for Ksi binary serialization source generation
- [KsiSerializeFieldAttribute](T.KsiSerializeFieldAttribute.g.md) — marks a serializable field and specifies its binary field id


## TRefList\<T\> Variants

- [RefList\<T\>](T.RefList-1.g.md) — a dynamic array collection wrapping the `Persistent` allocator
- [TempRefList\<T\>](T.TempRefList-1.g.md) — a dynamic array collection wrapping the `Temp` allocator
- [ManagedRefList\<T\>](T.ManagedRefList-1.g.md) — a dynamic array collection wrapping a managed array


## Other Types

- [ExclusiveAccess\<T\>](T.ExclusiveAccess-1.g.md) — a container that provides exclusive access to inner data
- [MutableAccessScope\<T\>](T.MutableAccessScope-1.g.md) — a structure that provides mutable exclusive access to wrapped data
- [ReadOnlyAccessScope\<T\>](T.ReadOnlyAccessScope-1.g.md) — a structure that provides readonly exclusive access to wrapped data
- [KsiExtensions](T.KsiExtensions.g.md) — general purpose extension methods provided by the ѯ-Framework
- [KsiHashTableSlotState](T.KsiHashTableSlotState.g.md) — an enum indicating a slot state for open addressing hash tables with lazy deletion
- [KsiPrimeUtil](T.KsiPrimeUtil.g.md) — utility class to work with prime numbers
- [BinaryReaderExtensions](T.BinaryReaderExtensions.g.md) — provides extension methods for [BinaryReader](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryReader?view=netstandard-2.1) to support Ksi serialization
- [BinaryWriterExtensions](T.BinaryWriterExtensions.g.md) — provides extension methods for [BinaryWriter](https://learn.microsoft.com/en-us/dotnet/api/System.IO.BinaryWriter?view=netstandard-2.1) to support Ksi serialization
- [PrefixedSizeOf](T.PrefixedSizeOf.g.md) — utility class for calculating the size of serialized data
- [ValueQualifier](T.ValueQualifier.g.md) — describes a value in the Ksi binary format
- [ValueQualifierExtensions](T.ValueQualifierExtensions.g.md) — provides extension methods for [ValueQualifier](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.ValueQualifier?view=netstandard-2.1)
- [LenPrefixSize](T.LenPrefixSize.g.md) — represents the size of the length prefix in bytes
- [LenPrefixSizeExtensions](T.LenPrefixSizeExtensions.g.md) — provides extension methods for [LenPrefixSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.LenPrefixSize?view=netstandard-2.1)
- [PrimitiveKind](T.PrimitiveKind.g.md) — represents the primitive value kind
- [PrimitiveSize](T.PrimitiveSize.g.md) — represents the size of a primitive value in bits
- [PrimitiveSizeExtensions](T.PrimitiveSizeExtensions.g.md) — provides extension methods for [PrimitiveSize](https://learn.microsoft.com/en-us/dotnet/api/Ksi.Serialization.PrimitiveSize?view=netstandard-2.1)
- [ValueKind](T.ValueKind.g.md) — represents the kind of value being serialized
