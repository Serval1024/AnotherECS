using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public unsafe struct WArray<T>
        where T : unmanaged
    {
        private WPtr<T> _data;
        private uint _length;

        public bool IsValid
            => _data.IsValid;

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public WArray(T* ptr, uint length)
        {
            this = new WArray<T>(new WPtr<T>(ptr), length);
        }

        public WArray(WPtr<T> ptr, uint length)
        {
            _data = ptr;
            _length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
            => _data.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            if (index >= _length)
            {
                throw new System.ArgumentOutOfRangeException(nameof(index));
            }
#endif
            return ref *(_data.Value + index);
        }
    }
}
