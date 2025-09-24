using System.Diagnostics.CodeAnalysis;

namespace Ksi
{
    public static class MoveExtensions
    {
        [ExplicitCopyReturn]
        [SuppressMessage("ExplicitCopy", "EXPCOPY07:Copied by Assignment")]
        public static T Move<[ExplicitCopy]T>(this ref T self) where T : struct
        {
            var moved = self;
            self = default;
            return moved;
        }
    }
}