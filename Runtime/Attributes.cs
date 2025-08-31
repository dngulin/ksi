using System;

namespace DnDev
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DeallocAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class RefListAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class UnmanagedRefListAttribute : Attribute
    {
    }
}