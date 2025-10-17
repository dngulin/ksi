namespace Ksi.Roslyn
{
    public static class KsiCompTemplates
    {
        public const string KsiHandle =
            // language=cs
            """
            {0} partial struct {1}
            {{
                /// <summary>
                /// Contains entries corresponding to <c>Domain</c> fields.
                /// Default value is invalid section.
                /// </summary>
                public enum KsiSection
                {{
                    {2}
                }}
                
                /// <summary>
                /// Represents an <c>Entity</c> address within the <c>Domain</c>.
                /// Default value is invalid handle.
                /// </summary>
                public struct KsiHandle
                {{
                    /// <summary>Section within the <c>Domain</c>.</summary>
                    public KsiSection Section;
                    
                    /// <summary>Index of <c>Entity</c> in the <c>Section</c>.</summary>
                    public int Index;
                
                    /// <summary>Creates a new instance pointing to an <c>Entity</c> in the <c>Section</c>.</summary>
                    public KsiHandle(KsiSection section, int index)
                    {{
                        Section = section;
                        Index = index;
                    }}
                }}
            }}
            """;

        public const string ArchetypeExtensions =
            // language=cs
            """
            /// <summary>
            /// Returns entity count in the archetype.
            /// Internally relies on item count of the first list of components. 
            /// </summary>
            /// <param name="self">Archetype to gen entity count</param>
            /// <returns>Entity count in the list.</returns>
            public static int Count(this in {0} self)
            {{
                {1}
            }}
            
            /// <summary>
            /// Adds a new entity to the archetype.
            /// Inernally adds a new item into each component list.
            /// </summary>
            /// <param name="self">Archetype to add an item</param>
            public static void Add(this ref {0} self)
            {{
                {2}
            }}
            
            /// <summary>
            /// Removes an entity from the archetype at the given index.
            /// </summary>
            /// <param name="self">Archetype to remove the entity</param>
            /// <param name="index">An index to remove the entity.</param>
            /// <exception cref="System.IndexOutOfRangeException">If the index is out of bounds for any component list</exception>
            public static void RemoveAt(this ref {0} self, int index)
            {{
                {3}
            }}
            
            /// <summary>
            /// Removes all enities from the archetype.
            /// </summary>
            /// <param name="self">Archetype to clear</param>
            public static void Clear(this ref {0} self)
            {{
                {4}
            }}
            """;
    }
}