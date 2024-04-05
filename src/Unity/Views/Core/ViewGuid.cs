using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Views.Core
{
    public readonly struct ViewGuid : IEquatable<ViewGuid>
    {
        private readonly string _data;

        public ViewGuid(string guid)
        {
            _data = guid;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !string.IsNullOrEmpty(_data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ViewGuid p0, ViewGuid p1)
            => p0._data == p1._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ViewGuid p0, ViewGuid p1)
            => !(p0 == p1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ViewGuid other)
            => this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => (obj is ViewId entity) && Equals(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => _data.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(ViewGuid other)
            => _data.CompareTo(other._data);

        public override string ToString()
            => _data.ToString();
    }
}
