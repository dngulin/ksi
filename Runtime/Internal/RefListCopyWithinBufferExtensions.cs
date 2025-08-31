using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Ksi
{
    internal static class RefListCopyWithinBufferExtensions
    {
        public static void CopyWithinBuffer<T>(this ref RefList<T> self, int srcIdx, int dstIdx, int count)
            where T : struct
        {
            Array.Copy(self.Buffer, srcIdx, self.Buffer, dstIdx, count);
        }

        public static unsafe void CopyWithinBuffer<T>(
            this ref TempRefList<T> self, int srcIdx, int dstIdx, int count
        )
            where T : unmanaged
        {
            UnsafeUtility.MemMove(&self.Buffer[dstIdx], &self.Buffer[srcIdx], sizeof(T) * count);
        }

        public static unsafe void CopyWithinBuffer<T>(
            this ref NativeRefList<T> self, int srcIdx, int dstIdx, int count
        )
            where T : unmanaged
        {
            UnsafeUtility.MemMove(&self.Buffer[dstIdx], &self.Buffer[srcIdx], sizeof(T) * count);
        }
    }
}