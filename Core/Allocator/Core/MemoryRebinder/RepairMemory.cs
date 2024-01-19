using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public unsafe struct RepairMemory<TAllocator> : IRepairMemory
        where TAllocator : IAllocator
    {
        private TAllocator _hAllocatorCopy;
        private bool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RepairMemory(TAllocator hAllocator)
        {
            _hAllocatorCopy = hAllocator;
            _isValid = _hAllocatorCopy.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repair(ref MemoryHandle memoryHandle)
        {
            if (_isValid)
            {
                _hAllocatorCopy.Repair(ref memoryHandle);
            }
        }
    }
}
