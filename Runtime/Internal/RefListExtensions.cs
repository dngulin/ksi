using System;
using Unity.Collections;

#pragma warning disable REFLIST01, REFPATH03

namespace Ksi
{
    internal static class RefListExtensions
    {
        public static int GetBufferSize<T>(this in RefList<T> self)
            where T : struct => self.Array?.Length ?? 0;
        public static int GetBufferSize<T>(this in TempRefList<T> self)
            where T : unmanaged => self.Array.Length;
        public static int GetBufferSize<T>(this in NativeRefList<T> self)
            where T : unmanaged => self.Array.Length;

        public static void ClearBuffer<T>(this ref RefList<T> self)
            where T : struct => self.Array.Clear(self.Count);
        public static void ClearBuffer<T>(this ref TempRefList<T> self)
            where T : unmanaged => self.Array.Clear(self.Count);
        public static void ClearBuffer<T>(this ref NativeRefList<T> self)
            where T : unmanaged => self.Array.Clear(self.Count);

        public static void CopyBufferFrom<T>(this ref RefList<T> self, in RefList<T> other)
            where T : struct => self.Array.CopyFrom(other.Array, other.Count);
        public static void CopyBufferFrom<T>(this ref TempRefList<T> self, in TempRefList<T> other)
            where T : unmanaged => self.Array.CopyFrom(other.Array, other.Count);
        public static void CopyBufferFrom<T>(this ref NativeRefList<T> self, in NativeRefList<T> other)
            where T : unmanaged => self.Array.CopyFrom(other.Array, other.Count);

        public static void CopyWithinBuffer<T>(this ref RefList<T> self, int srcIdx, int dstIdx, int count)
            where T : struct => self.Array.CopyInner(srcIdx, dstIdx, count);
        public static void CopyWithinBuffer<T>(this ref TempRefList<T> self, int srcIdx, int dstIdx, int count)
            where T : unmanaged => self.Array.CopyInner(srcIdx, dstIdx, count);
        public static void CopyWithinBuffer<T>(this ref NativeRefList<T> self, int srcIdx, int dstIdx, int count)
            where T : unmanaged => self.Array.CopyInner(srcIdx, dstIdx, count);


        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in RefList<T> self, int index)
            where T : struct => ref self.Array.Index(index);
        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in TempRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref readonly T IndexBuffer<T>(this in NativeRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);

        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref RefList<T> self, int index)
            where T : struct => ref self.Array.Index(index);
        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref TempRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);
        [RefListIndexer] public static ref T IndexBufferMut<T>([DynNoResize] this ref NativeRefList<T> self, int index)
            where T : unmanaged => ref self.Array.Index(index);

        public static void SetBufferSize<T>(this ref RefList<T> self, int size) where T : struct
        {
            UnifiedArrayApi.Resize(ref self.Array, size);
            self.Count = Math.Min(self.Count, size);
        }

        public static void SetBufferSize<T>(this ref TempRefList<T> self, int size) where T : unmanaged
        {
            self.Array.Resize(size, Allocator.Temp);
            self.Count = Math.Min(self.Count, size);
        }

        public static void SetBufferSize<T>(this ref NativeRefList<T> self, int size) where T : unmanaged
        {
            self.Array.Resize(size, Allocator.Persistent);
            self.Count = Math.Min(self.Count, size);
        }
    }
}