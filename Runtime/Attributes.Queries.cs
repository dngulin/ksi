using System;

namespace Ksi
{
    /// <summary>
    /// An attribute to mark a data type that can be queried
    /// with the <see cref="KsiQueryAttribute">KsiQuery</see> from the <see cref="KsiDomainAttribute">KsiDomain</see>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiComponentAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to mark an entity type (set of the <see cref="KsiComponentAttribute">KsiComponent</see> types)
    /// that should be stored in the <c>RefList</c> collection
    /// within the <see cref="KsiDomainAttribute">KsiDomain</see> structure.
    /// Use it if you need the <c>Array of Structures</c> data layout.
    /// </para>
    /// <para>
    /// Requirements:
    /// <list type="bullet">
    /// <item><description>All field types should be marked with the <see cref="KsiComponentAttribute"/></description></item>
    /// <item><description>All field types should be unique</description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiEntityAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to mark a type that represents a list of entities
    /// (set of the <see cref="KsiComponentAttribute">KsiComponent</see> types)
    /// within the <see cref="KsiDomainAttribute">KsiDomain</see> structure.
    /// Use it if you need the <c>Structure of Arrays</c> data layout.
    /// </para>
    /// <para>
    /// Requirements:
    /// <list type="bullet">
    /// <item><description>
    /// All field types should be <c>RefList</c> collections
    /// with the <see cref="KsiComponentAttribute">KsiComponent</see> item type
    /// </description></item>
    /// <item><description>All field types should be unique</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Triggers extension methods code generation to keep all inner lists with the same length:
    /// <list type="bullet">
    /// <item><description><c>(in T).Count()</c> — get entity count</description></item>
    /// <item><description><c>(ref T).RemoveAt(int)</c> — remove an entity at the given index</description></item>
    /// <item><description><c>(ref T).Add()</c> — add a new <c>default</c> value to each inner list</description></item>
    /// <item><description><c>(ref T).Clear()</c> — clear all inner lists</description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiArchetypeAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to mark a domain that can be extended with <see cref="KsiQueryAttribute">KsiQuery</see> methods.
    /// </para>
    /// <para>
    /// Should be a <c>partial struct</c> that has fields only of these types:
    /// <list type="bullet">
    /// <item><description>
    /// <c>RefList</c> of the <see cref="KsiEntityAttribute">KsiEntity</see> type
    /// for the <c>Array of Structures</c> data layout
    /// </description></item>
    /// <item><description>
    /// <see cref="KsiArchetypeAttribute">KsiArchetype</see> type for the <c>Structure of Arrays</c> data layout
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Triggers code generation to produce
    /// the <c>{DomainTypeName}.KsiSection</c> enum and the <c>{DomainTypeName}.KsiHandle</c> structure:
    /// <list type="bullet">
    /// <item><description>
    /// <c>{DomainTypeName}.KsiSection</c> — an enum that represent each field within the domain
    /// </description></item>
    /// <item><description>
    /// <c>{DomainTypeName}.KsiHandle</c> — a structure to represent a <see cref="KsiQueryAttribute">KsiQuery</see> address.
    /// Is composed of the <c>{DomainTypeName}.KsiSection</c> and the entity index (<c>int</c>) in the section.
    /// Should be used as the first argument for <see cref="KsiQueryAttribute">KsiQuery</see> methods
    /// </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiDomainAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>
    /// An attribute to produce the <c>ECS</c>-like query.
    /// It creates a code-generated extension method for the <see cref="KsiDomainAttribute">KsiDomain</see>
    /// structure that calls the marked method for each matching entity in the domain.
    /// </para>
    /// <para>
    /// Method signature requirements:
    /// <list type="bullet">
    /// <item><description>Should be a non-generic <c>static</c> method</description></item>
    /// <item><description>All parameters should be by-ref parameters to named structures</description></item>
    /// <item><description>
    /// The first argument should be a readonly reference to
    /// a <see cref="KsiDomainAttribute">KsiDomain</see> <c>Handle</c>
    /// </description></item>
    /// <item><description>
    /// The following parameters should be references to <see cref="KsiComponentAttribute">KsiComponent</see> types.
    /// At least one argument of that kind should be present
    /// </description></item>
    /// <item><description>
    /// Optionally, after that you can declare parameters marked with <see cref="KsiQueryParamAttribute"/>
    /// that are passed through from the generated extension method to the marked query method.
    /// Only the <see cref="DynNoResizeAttribute"/> is inherited for these parameters
    /// </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class KsiQueryAttribute : Attribute
    {
    }

    /// <summary>
    /// An attribute to mark a <see cref="KsiQueryAttribute">KsiQuery</see> parameter that
    /// is passed through from the generated extension method to the marked query method
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class KsiQueryParamAttribute : Attribute
    {
    }
}