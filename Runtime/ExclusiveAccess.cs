using System;

namespace Ksi
{
    public class ExclusiveAccess<T> where T: struct
    {
        private T _value;

        private ulong _nextAccessId;
        private ulong _activeAccessId;

        public MutableAccessScope<T> Mutable => new(this, GetNextAccessId());

        public ReadOnlyAccessScope<T> ReadOnly => new(this, GetNextAccessId());

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

    public readonly ref struct MutableAccessScope<T> where T : struct
    {
        private readonly ExclusiveAccess<T> _exclusive;
        private readonly ulong _accessId;

        internal MutableAccessScope(ExclusiveAccess<T> exclusive, ulong accessId)
        {
            _exclusive = exclusive;
            _accessId = accessId;
        }

        public ref T Value => ref _exclusive.Access(_accessId);
        public void Dispose() => _exclusive?.Unlock(_accessId);
    }

    public readonly ref struct ReadOnlyAccessScope<T> where T : struct
    {
        private readonly ExclusiveAccess<T> _exclusive;
        private readonly ulong _accessId;

        internal ReadOnlyAccessScope(ExclusiveAccess<T> exclusive, ulong accessId)
        {
            _exclusive = exclusive;
            _accessId = accessId;
        }

        public ref readonly T Value => ref _exclusive.Access(_accessId);
        public void Dispose() => _exclusive?.Unlock(_accessId);
    }
}