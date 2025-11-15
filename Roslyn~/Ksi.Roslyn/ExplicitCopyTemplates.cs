namespace Ksi.Roslyn
{
    public static class ExplicitCopyTemplates
    {
        public const string RefListExtensions =
            // language=cs
            """
            
            /// <summary>
            /// Copies all items from another list using explicit copy extension methods.
            /// All items existing before copying are removed.
            /// </summary>
            /// <param name="self">Destination list</param>
            /// <param name="other">Source list</param>
            public static void CopyFrom(this ref {0}<{1}> self, in {0}<{1}> other)
            {{
                self.Clear();
                self.AppendDefault(other.Count());
                
                for (var i = 0; i < other.Count(); i++)
                    self.RefAt(i).CopyFrom(other.RefReadonlyAt(i));
            }}
            
            /// <summary>
            /// Copies all items to another list using explicit copy extension methods.
            /// All items existing before copying are removed.
            /// </summary>
            /// <param name="self">Destination list</param>
            /// <param name="other">Source list</param>
            public static void CopyTo(this in {0}<{1}> self, ref {0}<{1}> other)
            {{
                other.CopyFrom(self);
            }}
            """;

        public const string RefListExtensionsForDeallocItems =
            // language=cs
            """
            
            /// <summary>
            /// Copies all items from another list using explicit copy extension methods.
            /// All items that are not updated by copying are removed and deallcoated.
            /// </summary>
            /// <param name="self">Destination list</param>
            /// <param name="other">Source list</param>
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
            /// Copies all items to another list using explicit copy extension methods.
            /// All items that are not updated by copying are removed and deallcoated.
            /// </summary>
            /// <param name="self">Source list</param>
            /// <param name="other">Destination list</param>
            public static void CopyTo(this in {0}<{1}> self, ref {0}<{1}> other)
            {{
                other.CopyFrom(self);
            }}
            """;
    }
}