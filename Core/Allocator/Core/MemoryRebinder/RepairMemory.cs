using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public unsafe struct RepairMemory<TAllocator> : IRepairMemory
        where TAllocator : IAllocator
    {
        private TAllocator _allocatorCopy;
        private bool _isValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RepairMemory(TAllocator hAllocator)
        {
            _allocatorCopy = hAllocator;
            _isValid = _allocatorCopy.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Repair(ref MemoryHandle memoryHandle)
        {
            if (_isValid)
            {
                _allocatorCopy.Repair(ref memoryHandle);
            }
        }
    }
}
