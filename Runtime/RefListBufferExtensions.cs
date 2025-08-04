using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace DnDev
{
    internal static class RefListBufferSizeExtensions
    {
        public static int GetBufferSize<T>(this in RefList<T> list) where T : struct => list.Buffer?.Length ?? 0;

        public static int GetBufferSize<T>(this in TempRefList<T> list) where T : unmanaged => list.Capacity;

        public static int GetBufferSize<T>(this in NativeRefList<T> list) where T : unmanaged => list.Capacity;
    }

    internal static class RefListSetBufferSizeExtensions
    {
        public static void SetBufferSize<T>(this ref RefList<T> list, int size) where T : struct
        {
            if (size == 0)
            {
                list.Buffer = null;
                return;
            }

            Array.Resize(ref list.Buffer, size);
        }

        public static unsafe void SetBufferSize<T>(this ref TempRefList<T> list, int size) where T : unmanaged
        {
            ResizeBuffer(ref list.Buffer, ref list.Capacity, size, ref list.Count, Allocator.Temp);
        }

        public static unsafe void SetBufferSize<T>(this ref NativeRefList<T> list, int size) where T : unmanaged
        {
            ResizeBuffer(ref list.Buffer, ref list.Capacity, size, ref list.Count, Allocator.Persistent);
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
        public static ref readonly T IndexBuffer<T>(this in RefList<T> list, int index) where T : struct
        {
            return ref list.Buffer[index];
        }

        public static ref T IndexBufferMut<T>(this ref RefList<T> list, int index) where T : struct
        {
            return ref list.Buffer[index];
        }

        public static unsafe ref readonly T IndexBuffer<T>(this in TempRefList<T> list, int index) where T : unmanaged
        {
            return ref list.Buffer[index];
        }

        public static unsafe ref T IndexBufferMut<T>(this ref TempRefList<T> list, int index) where T : unmanaged
        {
            return ref list.Buffer[index];
        }

        public static unsafe ref readonly T IndexBuffer<T>(this in NativeRefList<T> list, int index) where T : unmanaged
        {
            return ref list.Buffer[index];
        }

        public static unsafe ref T IndexBufferMut<T>(this ref NativeRefList<T> list, int index) where T : unmanaged
        {
            return ref list.Buffer[index];
        }
    }

    internal static class RefListClearBufferExtensions
    {
        public static void ClearBuffer<T>(this ref RefList<T> list) where T : struct
        {
            Array.Clear(list.Buffer, 0, list.Count);
        }

        public static unsafe void ClearBuffer<T>(this ref TempRefList<T> list) where T : unmanaged
        {
            UnsafeUtility.MemSet(list.Buffer, 0, sizeof(T) * list.Count);
        }

        public static unsafe void ClearBuffer<T>(this ref NativeRefList<T> list) where T : unmanaged
        {
            UnsafeUtility.MemSet(list.Buffer, 0, sizeof(T) * list.Count);
        }
    }

    internal static class RefListCopyBufferRangeExtensions
    {
        public static void CopyBufferRange<T>(this ref RefList<T> list, int srcIdx, int dstIdx, int count)
            where T : struct
        {
            Array.Copy(list.Buffer, srcIdx, list.Buffer, dstIdx, count);
        }

        public static unsafe void CopyBufferRange<T>(
            this ref TempRefList<T> list, int srcIdx, int dstIdx, int count
        )
            where T : unmanaged
        {
            UnsafeUtility.MemMove(&list.Buffer[dstIdx], &list.Buffer[srcIdx], sizeof(T) * count);
        }

        public static unsafe void CopyBufferRange<T>(
            this ref NativeRefList<T> list, int srcIdx, int dstIdx, int count
        )
            where T : unmanaged
        {
            UnsafeUtility.MemMove(&list.Buffer[dstIdx], &list.Buffer[srcIdx], sizeof(T) * count);
        }
    }
}