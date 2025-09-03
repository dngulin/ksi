using System;

namespace Ksi
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NonAllocatedResultAttribute : Attribute
    {
    }
}