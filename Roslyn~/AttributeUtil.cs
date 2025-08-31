using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DnDev.Roslyn
{
    public static class AttributeUtil
    {
        private const string NoCopy = "NoCopy";
        private const string Dealloc = "Dealloc";
        private const string RefList = "RefList";

        private const string Suffix = "Attribute";

        public static bool ContainsDealloc(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any(IsDealloc);
        }

        private static bool IsDealloc(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == Dealloc || name == Dealloc + Suffix;
        }

        public static bool IsDealloc(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == Dealloc + Suffix;
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
            return name == RefList || name == RefList + Suffix;
        }

        public static bool IsRefList(AttributeData attribute)
        {
            return attribute.AttributeClass != null && attribute.AttributeClass.Name == RefList + Suffix;
        }
    }
}