using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Unity.Collections
{
    using System;

    internal enum Allocator { Persistent, Temp, }

    namespace LowLevel.Unsafe {
        internal static class UnsafeUtility
        {
            public static unsafe void* Malloc(long size, int alignment, Allocator allocator)
            {
                return (void*)Marshal.AllocHGlobal((int)size);
            }

            public static unsafe void Free(void* memory, Allocator allocator)
            {
                Marshal.FreeHGlobal((IntPtr)memory);
            }

            public static unsafe void MemCpy(void* destination, void* source, long size)
            {
                var len = checked((int)size);
                new ReadOnlySpan<byte>(source, len).CopyTo(new Span<byte>(destination, len));
            }

            public static unsafe void MemMove(void* destination, void* source, long size)
            {
                var len = checked((int)size);
                new ReadOnlySpan<byte>(source, len).CopyTo(new Span<byte>(destination, len));
            }

            public static unsafe void MemClear(void* destination, long size)
            {
                var len = checked((int)size);
                new Span<byte>(destination, len).Clear();
            }

#pragma warning disable CS0649
            private struct BytePad<T> where T : struct { public byte Padding; public T Data; }
#pragma warning restore CS0649
            public static int AlignOf<T>() where T : struct
            {
                return (int)Marshal.OffsetOf<BytePad<T>>(nameof(BytePad<T>.Data));
            }
        }
    }
}