using System;
using Unity.Collections.LowLevel.Unsafe;

#pragma warning disable REFLIST01

namespace Ksi
{
    internal static class RefListClearBufferExtensions
    {
        public static void ClearBuffer<T>(this ref RefList<T> self) where T : struct
        {
            Array.Clear(self.Buffer, 0, self.Count);
        }

        public static unsafe void ClearBuffer<T>(this ref TempRefList<T> self) where T : unmanaged
        {
            UnsafeUtility.MemSet(self.Buffer, 0, sizeof(T) * self.Count);
        }

        public static unsafe void ClearBuffer<T>(this ref NativeRefList<T> self) where T : unmanaged
        {
            UnsafeUtility.MemSet(self.Buffer, 0, sizeof(T) * self.Count);
        }
    }
}