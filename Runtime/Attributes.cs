using System;

namespace Ksi
{
    /// <summary>
    /// Attribute that forbids structure implicit copying and provides explicit copy extension methods.
    /// Can be applied only to POD types without any methods and private fields.
    /// Should be added to a struct that contains fields of NoCopy type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class NoCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a method that returns a new instance of the [NoCopy] type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NoCopyReturnAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to provide Dealloc extension methods.
    /// Should be added to a struct that contains fields of Dealloc or RefList types.
    /// Also requires a NoCopy attribute.
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

    [AttributeUsage(AttributeTargets.Struct)]
    public class DynSizedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class DynNoResizeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DynReturnsSelfAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RefListIndexerAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RefListIteratorAttribute : Attribute
    {
    }
}