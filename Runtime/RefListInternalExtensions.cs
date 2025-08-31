using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Ksi
{
    internal static class RefListBufferSizeExtensions
    {
        public static int GetBufferSize<T>(this in RefList<T> self) where T : struct => self.Buffer?.Length ?? 0;

        public static int GetBufferSize<T>(this in TempRefList<T> self) where T : unmanaged => self.Capacity;

        public static int GetBufferSize<T>(this in NativeRefList<T> self) where T : unmanaged => self.Capacity;
    }

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

    internal static class RefListIndexBufferExtensions
    {
        public static ref readonly T IndexBuffer<T>(this in RefList<T> self, int index) where T : struct
        {
            return ref self.Buffer[index];
        }

        public static ref T IndexBufferMut<T>(this ref RefList<T> self, int index) where T : struct
        {
            return ref self.Buffer[index];
        }

        public static unsafe ref readonly T IndexBuffer<T>(this in TempRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        public static unsafe ref T IndexBufferMut<T>(this ref TempRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        public static unsafe ref readonly T IndexBuffer<T>(this in NativeRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        public static unsafe ref T IndexBufferMut<T>(this ref NativeRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }
    }

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