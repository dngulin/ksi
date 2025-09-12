using System;

namespace Ksi
{
    /// <summary>
    /// An internal hint attribute to trigger API code generation for RefList variants.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal class RefListAttribute : Attribute
    {
    }

    /// <summary>
    /// An internal hint attribute for the reference path analyzer.
    /// Indicates that the extension method returns a RefList iterator that iterates over list items.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal class RefListIteratorAttribute : Attribute
    {
    }
}