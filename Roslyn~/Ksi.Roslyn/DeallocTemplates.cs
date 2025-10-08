namespace Ksi.Roslyn
{
    public static class DeallocTemplates
    {
        public static readonly string[] RefListExtensionNames = ["Dealloc", "Deallocated", "Clear", "RemoveAt"];

        public const string RefListDeallocItemsAndSelf =
            // language=cs
            """

            /// <summary>
            /// Deallocates collection and all items.
            /// </summary>
            /// <param name="self">collection to deallocate</param>
            public static void Dealloc(this ref {0}<{1}> self)
            {{
                foreach(ref var item in self.RefIter())
                    item.Dealloc();

                #pragma warning disable REFLIST03
                self.Dealloc<{1}>();
                #pragma warning restore REFLIST03
            }}
            """;

        public const string RefListDeallocOnlyItems =
            // language=cs
            """

            /// <summary>
            /// Deallocates all collection items.
            /// </summary>
            /// <param name="self">collection to deallocate items</param>
            public static void Dealloc(this ref {0}<{1}> self)
            {{
                foreach(ref var item in self.RefIter())
                    item.Dealloc();
            }}
            """;

        public const string RefListDeallocated =
            // language=cs
            """

            /// <summary>
            /// Deallcoates the collection and returns it.
            /// </summary>
            /// <param name="self">collection to deallocate</param>
            /// <returns>`self` as an assignable reference</returns>
            [RefPath("self", "!"), NonAllocatedResult]
            public static ref {0}<{1}> Deallocated(this ref {0}<{1}> self)
            {{
                self.Dealloc();
                return ref self;
            }}
            """;

        public const string RefListSpecialized =
            // language=cs
            """

            /// <summary>
            /// Clears the collection and deallocates all items. 
            /// </summary>
            /// <param name="self">collection to clear</param>
            public static void Clear(this ref {0}<{1}> self)
            {{
                foreach(ref var item in self.RefIter())
                    item.Dealloc();

                #pragma warning disable REFLIST03
                self.Clear<{1}>();
                #pragma warning restore REFLIST03
            }}

            /// <summary>
            /// Removes an item from the collection and deallcoates it.
            /// </summary>
            /// <param name="self">collection to remove an item</param>
            /// <param name="index">index of the item to remove</param>
            public static void RemoveAt(this ref {0}<{1}> self, int index)
            {{
                self.RefAt(index).Dealloc();

                #pragma warning disable REFLIST03
                self.RemoveAt<{1}>(index);
                #pragma warning restore REFLIST03
            }}
            """;

        public const string DeallocatedExtension =
            // language=cs
            """

            /// <summary>
            /// Deallcoates the structure owned resources and returns it.
            /// </summary>
            /// <param name="self">structure to deallocate</param>
            /// <returns>`self` as an assignable reference</returns>
            [RefPath("self", "!"), NonAllocatedResult]
            public static ref {0} Deallocated(this ref {0} self)
            {{
                self.Dealloc();
                return ref self;
            }}
            """;
    }
}