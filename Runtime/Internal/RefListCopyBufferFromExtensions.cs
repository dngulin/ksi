using System;
using Unity.Collections.LowLevel.Unsafe;

#pragma warning disable REFLIST01

namespace Ksi
{
    internal static class RefListCopyBufferFromExtensions
    {
        public static void CopyBufferFrom<T>(this ref RefList<T> self, in RefList<T> other)
            where T : struct
        {
            Array.Copy(other.Buffer, self.Buffer, other.Count);
        }

        public static unsafe void CopyBufferFrom<T>(this ref TempRefList<T> self, in TempRefList<T> other)
            where T : unmanaged
        {
            UnsafeUtility.MemCpy(self.Buffer, other.Buffer, sizeof(T) * other.Count);
        }

        public static unsafe void CopyBufferFrom<T>(this ref NativeRefList<T> self, in NativeRefList<T> other)
            where T : unmanaged
        {
            UnsafeUtility.MemCpy(self.Buffer, other.Buffer, sizeof(T) * other.Count);
        }
    }
}