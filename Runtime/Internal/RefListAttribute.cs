using System;

namespace Ksi.Internal
{
    [AttributeUsage(AttributeTargets.Struct)]
    internal class RefListAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class RefListIteratorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class RefListIndexerAttribute : Attribute
    {
    }
}