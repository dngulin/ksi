using System;

namespace DnDev
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class DeallocApiAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class ImplementedDeallocAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class RefListApiAttribute : Attribute
    {
    }
}