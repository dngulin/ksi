namespace Ksi
{
    public static class MoveExtensions
    {
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