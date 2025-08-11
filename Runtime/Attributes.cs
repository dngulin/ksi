using System;

namespace DnDev.Collections
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DeallocApiAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class UnmanagedRefListAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class RefListApiAttribute : Attribute
    {
    }
}