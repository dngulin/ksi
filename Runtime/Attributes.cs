using System;

namespace Ksi
{
    /// <summary>
    /// Attribute that forbids structure implicit copying and provides explicit copy extension methods.
    /// Can be applied only to POD types without any methods and private fields.
    /// Should be added to a struct that contains fields of ExplicitCopy type.
    /// Can be also applied to a generic type parameter to make it compatible with [ExplicitCopy] types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class ExplicitCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a [ExplicitCopy] type that contains a dynamically sized buffer.
    /// Should be added to a struct that contains fields of the [DynSized] type.
    /// Enables reference lifetime and aliasing diagnostics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class DynSizedAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute that disallows any resizing operations on a [DynSized] type instance.
    /// Allows getting mutable references to collection items but disallows collection resizing.
    /// Hints the reference lifetime analyzer that any internal buffer cannot be resized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DynNoResizeAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a [DynSized] type that requires manual deallocation.
    /// Should be added to a struct that contains fields of the [Dealloc] type.
    /// Can be also applied to a generic type parameter to make it compatible with [Dealloc] types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class DeallocAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a method that returns a reference to a deallocated instance of the [Dealloc] type.
    /// Allows assigning a new value to the returned reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NonAllocatedResultAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a [DynSized] type that uses temporary allocator and should be created only on stack.
    /// Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.
    /// Should be added to a struct that contains fields of the [TempAlloc] type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class TempAllocAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// A hint attribute for the reference path analyzer.
    /// If the method returns a specific reference path, specify it with positional parameters,
    /// otherwise omit them (that means the method can return any inner reference).
    ///</para>
    /// <para>
    /// Indexers should be indicated with the "[n]" segments.
    /// The "!" should be placed after the last `[SynSized]` segment.
    ///</para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `[RefList]` corresponds to the reference path `MethodName()`, meaning it can reference any inner data
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `[RefList("self", "!")]` corresponds to the reference path `self!`, meaning it doesn't contribute to the parent reference path
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `[RefList("self", "Field", "!", "[n]")]` corresponds to the reference path `self.Field![n]`
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RefPathAttribute : Attribute
    {
        /// <summary>
        /// List of segments indicating the `RefPath` created by the marked extension method.
        /// Is empty array in case of the non-explicit `RefPath`.
        /// Can contain the `[DynSized]` separator "!".
        /// </summary>
        public string[] Segments { get; }

        /// <summary>
        /// Non-explicit `[RefPath]` attribute constructor.
        /// Will be embedded into the calling expression `RefPath` as the method name suffixed with "()".
        /// </summary>
        public RefPathAttribute() => Segments = Array.Empty<string>();

        /// <summary>
        /// Explicit `[RefPath]` attribute constructor.
        /// Will be embedded into the calling expression `RefPath` as a sequence of segments.
        /// </summary>
        /// <param name="segments">
        /// Array of segments produced by return expression.
        /// The `[DynSized]` separator "!" should be passed as a separated segment.
        /// </param>
        public RefPathAttribute(params string[] segments) => Segments = segments;
    }
}