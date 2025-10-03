namespace Ksi.Roslyn
{
    public static class DeallocTemplates
    {
        public static readonly string[] RefListExtensionNames = ["Dealloc", "Deallocated", "Clear", "RemoveAt"];

        public const string RefListDeallocItemsAndSelf = @"
        /// <summary>
        /// Specialized implementation for Dealloc item types
        /// </summary>
        public static void Dealloc(this ref {0}<{1}> self)
        {{
            foreach(ref var item in self.RefIter())
                item.Dealloc();

            #pragma warning disable REFLIST03
            self.Dealloc<{1}>();
            #pragma warning restore REFLIST03
        }}";

        public const string RefListDeallocOnlyItems = @"
        /// <summary>
        /// Specialized implementation for Dealloc item types
        /// </summary>
        public static void Dealloc(this ref {0}<{1}> self)
        {{
            foreach(ref var item in self.RefIter())
                item.Dealloc();
        }}";

        public const string RefListDeallocated = @"
        /// <summary>
        /// Specialized implementation for Dealloc item types
        /// </summary>
        [NonAllocatedResult]
        public static ref {0}<{1}> Deallocated(this ref {0}<{1}> self)
        {{
            self.Dealloc();
            return ref self;
        }}";

        public const string RefListSpecialized = @"
        /// <summary>
        /// Specialized implementation for Dealloc item types
        /// </summary>
        public static void Clear(this ref {0}<{1}> self)
        {{
            foreach(ref var item in self.RefIter())
                item.Dealloc();

            #pragma warning disable REFLIST03
            self.Clear<{1}>();
            #pragma warning restore REFLIST03
        }}

        /// <summary>
        /// Specialized implementation for Dealloc item types
        /// </summary>
        public static void RemoveAt(this ref {0}<{1}> self, int index)
        {{
            self.RefAt(index).Dealloc();

            #pragma warning disable REFLIST03
            self.RemoveAt<{1}>(index);
            #pragma warning restore REFLIST03
        }}";

        public const string DeallocatedExtension = @"
        [RefPath(""self"", ""!""), NonAllocatedResult]
        public static ref {0} Deallocated(this ref {0} self)
        {{
            self.Dealloc();
            return ref self;
        }}";
    }
}