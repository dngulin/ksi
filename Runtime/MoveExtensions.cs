namespace Ksi
{
    public static class MoveExtensions
    {
        [ExplicitCopyReturn]
        public static T Move<T>(this ref T self) where T : struct
        {
            var moved = self;
            self = default;
            return moved;
        }
    }
}