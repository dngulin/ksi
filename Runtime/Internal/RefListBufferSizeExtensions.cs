namespace Ksi
{
    internal static class RefListBufferSizeExtensions
    {
        public static int GetBufferSize<T>(this in RefList<T> self) where T : struct => self.Buffer?.Length ?? 0;

        public static int GetBufferSize<T>(this in TempRefList<T> self) where T : unmanaged => self.Capacity;

        public static int GetBufferSize<T>(this in NativeRefList<T> self) where T : unmanaged => self.Capacity;
    }
}