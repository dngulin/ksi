namespace Ksi.Roslyn
{
    public static class ExplicitCopyTemplates
    {
        public static readonly string[] RefListExtensionNames = ["CopyFrom", "CopyTo"];

        public const string RefListExtensions = @"
        /// <summary>
        /// Specialized implementation for ExplicitCopy types
        /// </summary>
        public static void CopyFrom(this ref {0}<{1}> self, in {0}<{1}> other)
        {{
            self.Clear();
            self.AppendDefault(other.Count());
            
            for (var i = 0; i < other.Count(); i++)
                self.RefAt(i).CopyFrom(other.RefReadonlyAt(i));
        }}

        /// <summary>
        /// Specialized implementation for ExplicitCopy types
        /// </summary>
        public static void CopyTo(this in {0}<{1}> self, ref {0}<{1}> other)
        {{
            other.CopyFrom(self);
        }}";

        public const string RefListExtensionsDealloc = @"
        /// <summary>
        /// Specialized implementation for ExplicitCopy+Dealloc types
        /// </summary>
        public static void CopyFrom(this ref {0}<{1}> self, in {0}<{1}> other)
        {{
            while (self.Count() > other.Count())
                self.RemoveAt(self.Count() - 1);

            if (self.Count() < other.Count())
                self.AppendDefault(other.Count() - self.Count());
            
            for (var i = 0; i < other.Count(); i++)
                self.RefAt(i).CopyFrom(other.RefReadonlyAt(i));
        }}

        /// <summary>
        /// Specialized implementation for ExplicitCopy+Dealloc types
        /// </summary>
        public static void CopyTo(this in {0}<{1}> self, ref {0}<{1}> other)
        {{
            other.CopyFrom(self);
        }}";
    }
}