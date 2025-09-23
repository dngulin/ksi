using System;

namespace Ksi
{
    /// <summary>
    /// Attribute that forbids structure implicit copying and provides explicit copy extension methods.
    /// Can be applied only to POD types without any methods and private fields.
    /// Should be added to a struct that contains fields of ExplicitCopy type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class ExplicitCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a method that returns a new instance of the ExplicitCopy type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ExplicitCopyReturnAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to provide Dealloc extension methods.
    /// Should be added to a struct that contains fields of Dealloc or RefList types.
    /// Also requires an ExplicitCopy attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class DeallocAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a method that returns a reference to a deallocated instance of the [Dealloc] type.
    /// Allows assigning a new value to the returned reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NonAllocatedResultAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a "dynamically sized" type.
    /// Should be added to a struct that contains any [DynSized] field.
    /// Enables reference lifetime and aliasing diagnostics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class DynSizedAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute that disallows any resizing operations on a [DynSized] type instance.
    /// Allows getting mutable references to collection items but disallows collection resizing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class DynNoResizeAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a temporary type that can be crated only on stack.
    /// Behaves similarly to ref struct but can be used as a generic parameter.
    /// Should be added to a struct that contains any [Temp] field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class TempAttribute : Attribute
    {
    }

    /// <summary>
    /// A hint attribute for the reference path analyzer.
    /// Indicates that the extension method returns any inner reference of `this` parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RefPathItemAttribute : Attribute
    {
    }

    /// <summary>
    /// A hint attribute for the reference path analyzer.
    /// Indicates that the extension method returns `this` parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RefPathSkipAttribute : Attribute
    {
    }

    /// <summary>
    /// A hint attribute for the reference path analyzer.
    /// Indicates that the extension method returns a RefList item reference.
    /// Can be added only for the RefList extension method that returns a RefList's item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RefListIndexerAttribute : Attribute
    {
    }
}