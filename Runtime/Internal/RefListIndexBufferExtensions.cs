#pragma warning disable REFLIST01, REFPATH03

namespace Ksi
{
    internal static class RefListIndexBufferExtensions
    {
        [RefListIndexer]
        public static ref readonly T IndexBuffer<T>(this in RefList<T> self, int index) where T : struct
        {
            return ref self.Buffer[index];
        }

        [RefListIndexer]
        public static ref T IndexBufferMut<T>([DynNoResize] this ref RefList<T> self, int index) where T : struct
        {
            return ref self.Buffer[index];
        }

        [RefListIndexer]
        public static unsafe ref readonly T IndexBuffer<T>(this in TempRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        [RefListIndexer]
        public static unsafe ref T IndexBufferMut<T>([DynNoResize] this ref TempRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        [RefListIndexer]
        public static unsafe ref readonly T IndexBuffer<T>(this in NativeRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }

        [RefListIndexer]
        public static unsafe ref T IndexBufferMut<T>([DynNoResize] this ref NativeRefList<T> self, int index) where T : unmanaged
        {
            return ref self.Buffer[index];
        }
    }
}