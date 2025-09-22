using System;
using Unity.Collections;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

#pragma warning disable REFLIST01

namespace Ksi
{
    internal static class UnsafeArrayExtensions
    {
        public static unsafe void Clear<T>(this ref UnsafeArray<T> self, int len)
            where T : unmanaged => MemSet(self.Buffer, 0, sizeof(T) * len);

        public static unsafe void CopyFrom<T>(this ref UnsafeArray<T> self, in UnsafeArray<T> other, int len)
            where T : unmanaged => MemCpy(self.Buffer, other.Buffer, sizeof(T) * len);

        public static unsafe void CopyInner<T>(this ref UnsafeArray<T> self, int srcIdx, int dstIdx, int count)
            where T : unmanaged => MemMove(&self.Buffer[dstIdx], &self.Buffer[srcIdx], sizeof(T) * count);

        public static unsafe ref T Index<T>(this in UnsafeArray<T> self, int idx)
            where T : unmanaged => ref self.Buffer[idx];

        public static unsafe void Resize<T>(this ref UnsafeArray<T> self, int len, Allocator allocator) where T : unmanaged
        {
            if (self.Length == len)
                return;

            T* buffer = null;

            if (len != 0)
            {
                buffer = (T*)Malloc(sizeof(T) * len, AlignOf<T>(), allocator);

                var copyLen = Math.Min(self.Length, len);
                MemCpy(buffer, self.Buffer, sizeof(T) * copyLen);

                var reminder = len - copyLen;
                MemSet(&buffer[copyLen], 0, sizeof(T) * reminder);
            }

            if (self.Buffer != null)
            {
                Free(self.Buffer, allocator);
            }

            self.Buffer = buffer;
            self.Length = len;
        }
    }
}