using System;

namespace Ksi
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