using System;

namespace Ksi
{
    /// <summary>
    /// <para>A trait attribute that forbids structure implicit copying.</para>
    /// <para>Should be added to a struct that contains fields of <c>ExplicitCopy</c> type.</para>
    /// <para>Can be also applied to a generic type parameter to make it compatible with <c>ExplicitCopy</c> types.</para>
    /// <para>
    /// Attribute triggers code generation for explicit copy extension methods:
    /// <list type="bullet">
    /// <item><description>
    /// <c>(in TExpCopy).CopyTo(ref TExpCopy other)</c> — copies the current struct to another one
    /// </description></item>
    /// <item><description>
    /// <c>(ref TExpCopy).CopyFrom(in TExpCopy other)</c> — copies another struct to the current one
    /// </description></item>
    /// <item><description>
    /// <c>(in TRefList&lt;TExpCopy&gt;).CopyTo(ref TRefList&lt;TExpCopy&gt; other)</c> — copies all items
    /// of the current list to another one
    /// </description></item>
    /// <item><description>
    /// <c>(ref TRefList&lt;TExpCopy&gt;).CopyFrom(in TRefList&lt;TExpCopy&gt; other)</c> — copies all items
    /// of another struct to the current one
    /// </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class ExplicitCopyAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to indicate an <see cref="ExplicitCopyAttribute">ExplicitCopy</see> type
    /// that contains a dynamically sized buffer.
    /// It enables reference lifetime and aliasing diagnostics for the marked struct.
    /// </para>
    /// <para>Should be added to a struct that contains fields of the <c>DynSized</c> type.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class DynSizedAttribute : Attribute
    {
    }

    /// <summary>
    /// <para> An attribute to disallow resizing operations on a <see cref="DynSizedAttribute">DynSized</see> parameter.</para>
    /// <para>Hints the reference lifetime analyzer that any internal buffer cannot be resized.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class DynNoResizeAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// A trait attribute to indicate a <see cref="DynSizedAttribute">DynSized</see>
    /// type that requires manual deallocation.
    /// </para>
    /// <para>Should be added to a struct that contains fields of the <c>Dealloc</c> type.</para>
    /// <para>Can be also applied to a generic type parameter to make it compatible with <c>Dealloc</c> types.</para>
    /// <para>
    /// Attribute triggers code generation for deallocation extension methods:
    /// <list type="bullet">
    /// <item><description>
    /// <c>(ref TDealloc).Dealloc()</c> — deallocates all data owned by the struct
    /// </description></item>
    /// <item><description>
    /// <c>(ref TDealloc).Deallocated()</c> — deallocates the struct and returns a reference to it
    /// </description></item>
    /// <item><description>
    /// <c>(ref TRefList&lt;TDealloc&gt;).Dealloc()</c> — deallocates all data owned by the list
    /// </description></item>
    /// <item><description>
    /// <c>(ref TRefList&lt;TDealloc&gt;).Deallocated()</c> — deallocates the list and returns a reference to it
    /// </description></item>
    /// <item><description>
    /// <c>(ref TRefList&lt;TDealloc&gt;).RemoveAt(int index)</c> — deallocates an item and removes it from the list
    /// </description></item>
    /// <item><description>
    /// <c>(ref TRefList&lt;TDealloc&gt;).Clear()</c> — deallocates all items and clears the list
    /// </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.GenericParameter)]
    public sealed class DeallocAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to mark a method returning a deallocated <see cref="DeallocAttribute">Dealloc</see> type reference.
    /// Allows assigning a new value to the returned reference.
    /// </para>
    /// <para>
    /// Attribute usage is not verified by roslyn analyzers.
    /// Returning a non-deallocated instance can cause memory leaks.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NonAllocatedResultAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// A trait attribute to indicate a <see cref="DynSizedAttribute">DynSized</see> type that uses <c>Temp</c> allocator.
    /// Allows omitting manual deallocation in exchange for a lifetime limited by a frame time.
    /// </para>
    /// <para>
    /// Heap-allocated <c>TempAlloc</c> structures can be allocated only with the <c>Temp</c> allocator.
    /// In other words, they can be stored only in the <see cref="TempRefList{T}"/>.
    /// It means that the root <c>TempAlloc</c> structure can be stored only on stack similarly to a <c>ref struct</c>.
    /// </para>
    /// <para>Required for structs that have fields of the <c>TempAlloc</c> types.</para>
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
    /// <c>[RefPath]</c> corresponds to the reference path <c>MethodName()</c>, meaning it can reference any inner data
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>[RefPath("self", "!")]</c> corresponds to the reference path <c>self!</c>,
    /// meaning it doesn't contribute to the parent reference path
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>[RefPath("self", "Field", "!", "[n]")]</c> corresponds to the reference path <c>self.Field![n]</c>
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