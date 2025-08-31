using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DnDev.Roslyn
{
    public static class AttributeUtil
    {
        private const string DeallocApi = "DeallocApi";
        private const string ExplicitCopyApi = "ExplicitCopyApi";

        private const string RefListApi = "RefListApi";
        private const string UnmanagedList = "UnmanagedRefList";

        private const string Suffix = "Attribute";

        public static bool ContainsDealloc(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsDealloc);
        }

        public static bool ContainsExplicitCopy(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsExplicitCopy);
        }

        public static bool ContainsRefList(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsRefList);
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

        private static bool IsExplicitCopy(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == ExplicitCopyApi || name == ExplicitCopyApi + Suffix;
        }

        public static bool IsExplicitCopy(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == ExplicitCopyApi + Suffix;
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