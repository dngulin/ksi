using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DnDev.Roslyn
{
    public static class AttributeUtil
    {
        private const string DeallocApi = "DeallocApi";
        private const string NoCopy = "NoCopy";

        private const string RefListApi = "RefListApi";
        private const string UnmanagedList = "UnmanagedRefList";

        private const string Suffix = "Attribute";

        public static bool ContainsDealloc(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsDealloc);
        }

        private static bool IsDealloc(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == DeallocApi || name == DeallocApi + Suffix;
        }

        public static bool IsDealloc(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == DeallocApi + Suffix;
        }



        public static bool ContainsNoCopy(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsNoCopy);
        }

        private static bool IsNoCopy(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == NoCopy || name == NoCopy + Suffix;
        }

        public static bool IsNoCopy(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == NoCopy + Suffix;
        }

        public static bool ContainsRefList(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsRefList);
        }

        private static bool IsRefList(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == RefListApi || name == RefListApi + Suffix;
        }

        public static bool IsUnmanagedRefList(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == UnmanagedList + Suffix;
        }
    }
}