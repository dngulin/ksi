namespace Ksi
{
    internal unsafe struct UnsafeArray<[ExplicitCopy, Dealloc, TempAlloc] T> where T : unmanaged
    {
        public T* Buffer;
        public int Length;
    }
}