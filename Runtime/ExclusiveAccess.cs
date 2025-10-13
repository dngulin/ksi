using System;

namespace Ksi
{
    /// <summary>
    /// Container designed to provide exclusive access to inner data.
    /// It is achieved by maintaining only one active <see cref="MutableAccessScope{T}"/> or <see cref="ReadOnlyAccessScope{T}"/> wrapping inner data.
    /// Supposed to wrap <see cref="DynSizedAttribute">DynSized</see> structures.
    /// </summary>
    public sealed class ExclusiveAccess<T> where T: struct
    {
        private T _value;

        private ulong _nextAccessId;
        private ulong _activeAccessId;

        /// <summary>
        /// Creates a new instance of <see cref="MutableAccessScope{T}"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If an active instance of <see cref="MutableAccessScope{T}"/> or <see cref="ReadOnlyAccessScope{T}"/> already exists.
        /// </exception>
        public MutableAccessScope<T> Mutable => new MutableAccessScope<T>(this, GetNextAccessId());

        /// <summary>
        /// Creates a new instance of `<see cref="ReadOnlyAccessScope{T}"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If an active instance of <see cref="MutableAccessScope{T}"/> or <see cref="ReadOnlyAccessScope{T}"/> already exists.
        /// </exception>
        public ReadOnlyAccessScope<T> ReadOnly => new ReadOnlyAccessScope<T>(this, GetNextAccessId());

        private ulong GetNextAccessId()
        {
            if (_activeAccessId != 0)
                throw new InvalidOperationException();

            _activeAccessId = ++_nextAccessId;

            if (_activeAccessId == 0)
                _activeAccessId = ++_nextAccessId;

            return _activeAccessId;
        }

        internal ref T Access(ulong id)
        {
            if (_activeAccessId == id)
                return ref _value;

            throw new InvalidOperationException();
        }

        internal void Unlock(ulong id)
        {
            if (_activeAccessId == id)
                _activeAccessId = 0;
        }
    }

    /// <summary>
    /// Structure that provides mutable exclusive access to wrapped data.
    /// Should be disposed after usage to release access lock from the parent <see cref="ExclusiveAccess{T}"/> instance.
    /// </summary>
    public readonly ref struct MutableAccessScope<T> where T : struct
    {
        private readonly ExclusiveAccess<T> _exclusive;
        private readonly ulong _accessId;

        internal MutableAccessScope(ExclusiveAccess<T> exclusive, ulong accessId)
        {
            _exclusive = exclusive;
            _accessId = accessId;
        }

        /// <summary>
        /// <para>Returns a mutable reference to the wrapped data.</para>
        /// <para>Does not add any segments to <c>RefPath</c>.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the data is not available to this access scope (e.g. usage after disposing).
        /// </exception>
        public ref T Value => ref _exclusive.Access(_accessId);

        /// <summary>
        /// Deactivates the access scope and allows creating a new one from the parent <see cref="ExclusiveAccess{T}"/> instance.
        /// </summary>
        public void Dispose() => _exclusive?.Unlock(_accessId);
    }

    /// <summary>
    /// Structure that provides readonly exclusive access to wrapped data.
    /// Should be disposed after usage to release access lock from the parent <see cref="ExclusiveAccess{T}"/> instance.
    /// </summary>
    public readonly ref struct ReadOnlyAccessScope<T> where T : struct
    {
        private readonly ExclusiveAccess<T> _exclusive;
        private readonly ulong _accessId;

        internal ReadOnlyAccessScope(ExclusiveAccess<T> exclusive, ulong accessId)
        {
            _exclusive = exclusive;
            _accessId = accessId;
        }

        /// <summary>
        /// <para>Returns a readonly reference to the wrapped data.</para>
        /// <para>Does not add any segments to <c>RefPath</c>.</para>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If the data is not available to this access scope (e.g. usage after disposing).
        /// </exception>
        public ref readonly T Value => ref _exclusive.Access(_accessId);

        /// <summary>
        /// Deactivates the access scope and allows creating a new one from the parent <see cref="ExclusiveAccess{T}"/> instance.
        /// </summary>
        public void Dispose() => _exclusive?.Unlock(_accessId);
    }
}