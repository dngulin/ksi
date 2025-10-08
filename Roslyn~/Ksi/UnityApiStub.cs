#pragma warning disable CS1591

// ReSharper disable once CheckNamespace
namespace Unity.Collections
{
    using System;

    public enum Allocator { Persistent, Temp, }

    namespace LowLevel.Unsafe {
        public static class UnsafeUtility
        {
            public static unsafe void* Malloc(long size, int alignment, Allocator allocator) => throw new NotSupportedException();
            public static unsafe void Free(void* memory, Allocator allocator) => throw new NotSupportedException();
            public static unsafe void MemCpy(void* destination, void* source, long size) => throw new NotSupportedException();
            public static unsafe void MemMove(void* destination, void* source, long size) => throw new NotSupportedException();
            public static unsafe void MemSet(void* destination, byte value, long size) => throw new NotSupportedException();

            // ReSharper disable once UnusedTypeParameter
            public static int AlignOf<T>() where T : struct => throw new NotSupportedException();
        }
    }
}