namespace DnDev.Roslyn
{
    public static class DeallocTemplates
    {
        public const string RefListDeallocMethods = @"
        /// <summary>
        /// Specialized implemenation for Dealloc types
        /// </summary>
        public static void Dealloc(this ref {0}<{1}> self)
        {{
            foreach(ref var item in self.RefIter())
                item.Dealloc();

            self.Dealloc<{1}>();
        }}

        /// <summary>
        /// Specialized implemenation for Dealloc types
        /// </summary>
        public static void Clear(this ref {0}<{1}> self)
        {{
            foreach(ref var item in self.RefIter())
                item.Dealloc();

            self.Clear<{1}>();
        }}

        /// <summary>
        /// Specialized implemenation for Dealloc types
        /// </summary>
        public static void RemoveAt(this ref {0}<{1}> self, int index)
        {{
            self.RefAt(index).Dealloc();
            self.RemoveAt<{1}>(index);
        }}";
    }
}