namespace Ksi
{
    /// <summary>
    /// General purpose extension methods provided by the ѯ-Framework.
    /// </summary>
    public static class KsiExtensions
    {
        /// <summary>
        /// Move structure ownership. After invocation the original value is set to `default` (zeroed).
        /// Can be required to work with `[ExplicitCopy]` types.
        /// </summary>
        /// <param name="self">insatnce to be moved</param>
        /// <returns>A new instance crated from the `self` parameter</returns>
        public static T Move<[ExplicitCopy, Dealloc] T>(this ref T self) where T : struct
        {
#pragma warning disable EXPCOPY04
            var moved = self;
#pragma warning restore EXPCOPY04
#pragma warning disable DEALLOC04
            self = default;
#pragma warning restore DEALLOC04
            return moved;
        }
    }
}