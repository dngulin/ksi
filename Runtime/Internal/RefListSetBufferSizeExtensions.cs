using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

#pragma warning disable REFLIST01

namespace Ksi
{
    internal static class RefListSetBufferSizeExtensions
    {
        public static void SetBufferSize<T>(this ref RefList<T> self, int size) where T : struct
        {
            if (size == 0)
            {
                self.Buffer = null;
                self.Count = 0;
                return;
            }

            Array.Resize(ref self.Buffer, size);
            self.Count = Math.Min(self.Count, size);
        }

        public static unsafe void SetBufferSize<T>(this ref TempRefList<T> self, int size) where T : unmanaged
        {
            ResizeBuffer(ref self.Buffer, ref self.Capacity, size, ref self.Count, Allocator.Temp);
        }

        public static unsafe void SetBufferSize<T>(this ref NativeRefList<T> self, int size) where T : unmanaged
        {
            ResizeBuffer(ref self.Buffer, ref self.Capacity, size, ref self.Count, Allocator.Persistent);
        }

        private static unsafe void ResizeBuffer<T>(
            ref T* buffer, ref int capacity, int newCapacity, ref int count, Allocator allocator
        )
            where T : unmanaged
        {
            Debug.Assert(buffer == null || capacity != 0, "Buffer ptr and capacity are not aligned");
            Debug.Assert(capacity >= 0, "Negative capacity");
            Debug.Assert(newCapacity >= 0, "Negative new capacity");
            Debug.Assert(count >= 0, "Negative item count");

            if (capacity == newCapacity)
                return;

            var newCount = Math.Min(count, newCapacity);

            T* newBuffer = null;

            if (newCapacity != 0)
            {
                newBuffer = (T*)UnsafeUtility.Malloc(sizeof(T) * newCapacity, UnsafeUtility.AlignOf<T>(), allocator);
                UnsafeUtility.MemCpy(newBuffer, buffer, sizeof(T) * newCount);
                UnsafeUtility.MemSet(&newBuffer[newCount], 0, sizeof(T) * (newCapacity - newCount));
            }

            if (buffer != null)
            {
                UnsafeUtility.Free(buffer, allocator);
            }

            buffer = newBuffer;
            capacity = newCapacity;
            count = newCount;
        }
    }
}