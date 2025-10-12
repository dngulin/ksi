using System;

namespace Ksi
{
    /// <summary>
    /// Attribute that forbids structure implicit copying and provides explicit copy extension methods.
    /// Can be applied only to POD types without any methods and private fields.
    /// Should be added to a struct that contains fields of <c>ExplicitCopy</c> type.
    /// Can be also applied to a generic type parameter to make it compatible with <c>ExplicitCopy</c> types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class ExplicitCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate an <see cref="ExplicitCopyAttribute">ExplicitCopy</see> type
    /// that contains a dynamically sized buffer.
    /// Should be added to a struct that contains fields of the <c>DynSized</c> type.
    /// Enables reference lifetime and aliasing diagnostics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class DynSizedAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute that disallows any resizing operations on a <see cref="DynSizedAttribute">DynSized</see> type instance.
    /// Allows getting mutable references to collection items but disallows collection resizing.
    /// Hints the reference lifetime analyzer that any internal buffer cannot be resized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DynNoResizeAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a <see cref="DynSizedAttribute">DynSized</see> type that requires manual deallocation.
    /// Should be added to a struct that contains fields of the <c>Dealloc</c> type.
    /// Can be also applied to a generic type parameter to make it compatible with <c>Dealloc</c> types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class DeallocAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a method that returns a reference to
    /// a deallocated instance of the <see cref="DeallocAttribute">Dealloc</see> type.
    /// Allows assigning a new value to the returned reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NonAllocatedResultAttribute : Attribute
    {
    }

    /// <summary>
    /// Attribute to indicate a <see cref="DynSizedAttribute">DynSized</see> type
    /// that uses temporary allocator and should be created only on stack.
    /// Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.
    /// Should be added to a struct that contains fields of the <c>TempAlloc</c> type.
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
    /// The "!" should be placed after the last <see cref="DynSizedAttribute">DynSized</see> segment.
    ///</para>
    /// <para>
    /// Examples:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>[RefList]</c> corresponds to the reference path <c>MethodName()</c>, meaning it can reference any inner data
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>[RefList("self", "!")]</c> corresponds to the reference path <c>self!</c>,
    /// meaning it doesn't contribute to the parent reference path
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>[RefList("self", "Field", "!", "[n]")]</c> corresponds to the reference path <c>self.Field![n]</c>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RefPathAttribute : Attribute
    {
        /// <summary>
        /// List of segments indicating the <c>RefPath</c> created by the marked extension method.
        /// Is empty array in case of the non-explicit <c>RefPath</c>.
        /// Can contain the <see cref="DynSizedAttribute">DynSized</see> separator "!".
        /// </summary>
        public string[] Segments { get; }

        /// <summary>
        /// Non-explicit <c>RefPath</c> attribute constructor.
        /// Will be embedded into the parent <c>RefPath</c> expression as the method name suffixed with <c>()</c>.
        /// </summary>
        public RefPathAttribute() => Segments = Array.Empty<string>();

        /// <summary>
        /// Explicit <c>RefPath</c> attribute constructor.
        /// Will be embedded into the parent <c>RefPath</c> expression as a sequence of segments.
        /// </summary>
        /// <param name="segments">
        /// Array of segments produced by return expression.
        /// The <see cref="DynSizedAttribute">DynSized</see> separator "!" should be passed as a separated segment.
        /// </param>
        public RefPathAttribute(params string[] segments) => Segments = segments;
    }
}