namespace Ksi
{
    internal unsafe struct UnsafeArray<T> where T : unmanaged
    {
        public T* Buffer;
        public int Length;
    }
}