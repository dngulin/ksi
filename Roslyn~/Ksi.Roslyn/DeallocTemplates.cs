namespace Ksi.Roslyn
{
    public static class DeallocTemplates
    {
        public static readonly string[] RefListExtensionNames = ["Dealloc", "Deallocated", "Clear", "RemoveAt"];

        public const string RefListDeallocItemsAndSelf =
            // language=cs
            """

            /// <summary>
            /// Deallocate the list and all items.
            /// After deallocation the list becomes zeroed.
            /// </summary>
            /// <param name="self">List to deallocate</param>
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
            /// Deallocate all list items.
            /// After deallocation list is not cleared.
            /// </summary>
            /// <param name="self">List to deallocate items</param>
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
            /// <para>Deallocate the list and returns it.</para>
            /// <para>Does not add any segments to <c>RefPath</c>.</para>
            /// </summary>
            /// <param name="self">List to deallocate</param>
            /// <returns>The list as an assignable reference.</returns>
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
            /// Clears the list and deallocate all items. 
            /// </summary>
            /// <param name="self">List to clear</param>
            public static void Clear(this ref {0}<{1}> self)
            {{
                foreach(ref var item in self.RefIter())
                    item.Dealloc();

                #pragma warning disable REFLIST03
                self.Clear<{1}>();
                #pragma warning restore REFLIST03
            }}

            /// <summary>
            /// Removes an item from the list and deallocate it.
            /// </summary>
            /// <param name="self">List to remove an item</param>
            /// <param name="index">Index of the item to remove</param>
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
            /// <para>Deallocate the structure and returns it.</para>
            /// <para>Does not add any segments to <c>RefPath</c>.</para>
            /// </summary>
            /// <param name="self">Structure to deallocate</param>
            /// <returns>The structure as an assignable reference</returns>
            [RefPath("self", "!"), NonAllocatedResult]
            public static ref {0} Deallocated(this ref {0} self)
            {{
                self.Dealloc();
                return ref self;
            }}
            """;
    }
}