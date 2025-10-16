namespace Ksi.Roslyn
{
    public static class KsiCompTemplates
    {
        public const string KsiHandle =
            // language=cs
            """
            public partial struct {0}
            {{
                /// <summary>
                /// Contains entries corresponding to <c>Domain</c> fields.
                /// Default value is invalid section.
                /// </summary>
                public enum KsiSection
                {{
                    {1}
                }}
                
                /// <summary>
                /// Represents an <c>Entity</c> address within the <c>Domain</c>.
                /// Default value is invalid handle.
                /// </summary>
                public readonly struct KsiHandle
                {{
                    /// <summary>Section within the <c>Domain</c>.</summary>
                    public readonly KsiSection Section;
                    
                    /// <summary>Index of <c>Entity</c> in the <c>Section</c>.</summary>
                    public readonly int Index;
                
                    /// <summary>Creates a new instance pointing to an <c>Entity</c> in the <c>Section</c>.</summary>
                    public KsiHandle(KsiSection section, int index)
                    {{
                        Section = section;
                        Index = index;
                    }}
                }}
            }}
            """;
    }
}