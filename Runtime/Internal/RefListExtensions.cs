using System;
using Unity.Collections;

#pragma warning disable REFLIST01

namespace Ksi
{
    internal static class RefListExtensions
    {
        public static int GetBufferSize<T>(this in RefList<T> self)
            where T : unmanaged => self.Array.Length;
        public static int GetBufferSize<T>(this in TempRefList<T> self)
            where T : unmanaged => self.Array.Length;
        public static int GetBufferSize<T>(this in ManagedRefList<T> self)
            where T : struct => self.Array?.Length ?? 0;

        public static void ClearBuffer<T>(this ref RefList<T> self)
            where T : unmanaged => self.Array.Clear(self.Count);
        public static void ClearBuffer<T>(this ref TempRefList<T> self)
            where T : unmanaged => self.Array.Clear(self.Count);
        public static void ClearBuffer<T>(this ref ManagedRefList<T> self)
            where T : struct => Array.Clear(self.Array, 0, self.Count);

        public static void CopyBufferFrom<T>(this ref RefList<T> self, in RefList<T> other)
            where T : unmanaged => self.Array.CopyFrom(other.Array, other.Count);
        public static void CopyBufferFrom<T>(this ref TempRefList<T> self, in TempRefList<T> other)
            where T : unmanaged => self.Array.CopyFrom(other.Array, other.Count);
        public static void CopyBufferFrom<T>(this ref ManagedRefList<T> self, in ManagedRefList<T> other)
            where T : struct => Array.Copy(other.Array, self.Array, other.Count);

        public static void CopyWithinBuffer<T>(this ref RefList<T> self, int srcIdx, int dstIdx, int count)
            where T : unmanaged => self.Array.CopyInner(srcIdx, dstIdx, count);
        public static void CopyWithinBuffer<T>(this ref TempRefList<T> self, int srcIdx, int dstIdx, int count)
            where T : unmanaged => self.Array.CopyInner(srcIdx, dstIdx, count);
        public static void CopyWithinBuffer<T>(this ref ManagedRefList<T> self, int srcIdx, int dstIdx, int count)
            where T : struct => Array.Copy(self.Array, srcIdx, self.Array, dstIdx, count);

        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in RefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in TempRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in ManagedRefList<T> self, int index)
            where T : struct => ref self.Array[index];

        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref RefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref TempRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref ManagedRefList<T> self, int index)
            where T : struct => ref self.Array[index];

        public static void SetBufferSize<T>(this ref RefList<T> self, int size) where T : unmanaged
        {
            self.Array.Resize(size, Allocator.Persistent);
            self.Count = Math.Min(self.Count, size);
        }

        public static void SetBufferSize<T>(this ref TempRefList<T> self, int size) where T : unmanaged
        {
            self.Array.Resize(size, Allocator.Temp);
            self.Count = Math.Min(self.Count, size);
        }

        public static void SetBufferSize<T>(this ref ManagedRefList<T> self, int size) where T : struct
        {
            if (size == 0)
                self.Array = null;
            else
                Array.Resize(ref self.Array, size);

            self.Count = Math.Min(self.Count, size);
        }
    }
}