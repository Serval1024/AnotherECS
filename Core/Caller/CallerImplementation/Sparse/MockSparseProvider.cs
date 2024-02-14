using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Caller
{
    public unsafe struct MockSparseProvider : IDisposable
    {
        private readonly BAllocator* _allocator;
        private MemoryHandle _data;
        private uint _byteSize;

        public MockSparseProvider(BAllocator* allocator)
        {
            _allocator = allocator;
            _data = default;
            _byteSize = 0;
        }

        public WArray<T> Get<T>(uint size)
            where T : unmanaged
        {
            var byteSize = size * (uint)sizeof(T);
            if (_data.IsValid)
            {
                if (_byteSize < byteSize)
                {
                    _byteSize = byteSize;
                    _allocator->Deallocate(ref _data);
                    _data = _allocator->Allocate(_byteSize);
                }
            }
            else
            {
                _byteSize = byteSize;
                _data = _allocator->Allocate(_byteSize);
            }
            return CreateWrapper<T>(ref _data, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_data.IsValid)
            {
                _allocator->Deallocate(ref _data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WArray<T> CreateWrapper<T>(ref MemoryHandle data, uint size)
            where T : unmanaged
            => new((T*)data.GetPtr(), size);
    }
}
