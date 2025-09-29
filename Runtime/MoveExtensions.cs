using System.Diagnostics.CodeAnalysis;

namespace Ksi
{
    public static class MoveExtensions
    {
        [SuppressMessage("ExplicitCopy", "EXPCOPY04:Assignment copy of the [ExplicitCopy] instance")]
        [SuppressMessage("Dealloc", "DEALLOC04:Dealloc Instance Overwrite")]
        public static T Move<[ExplicitCopy, Dealloc] T>(this ref T self) where T : struct
        {
            var moved = self;
            self = default;
            return moved;
        }
    }
}