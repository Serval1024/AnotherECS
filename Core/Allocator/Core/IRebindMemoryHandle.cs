using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal interface IRebindMemoryHandle
    {
        void RebindMemoryHandle(ref MemoryRebinderContext rebinder);
    }

    public unsafe struct MemoryRebinderContext : IDisposable
    {
        private NArray<BAllocator, MemoryRebinder> _rebinders;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryRebinderContext(NArray<BAllocator, MemoryRebinder> rebinders)
        {
            _rebinders = rebinders;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rebind(uint allocatorId, ref MemoryHandle memoryHandle)
        {
            _rebinders.GetRef(allocatorId).Rebind(ref memoryHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _rebinders.Dispose();
        }
    }
    
    public unsafe struct MemoryRebinder : IDisposable
    {
        private bool _isValide;
        private int _segmentSizePower2;
        private NArray<BAllocator, WPtr<int>> _isDirties;
        private NArray<BAllocator, WPtr<byte>> _memories;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryRebinder(int segmentSizePower2, NArray<BAllocator, WPtr<int>> isDirties, NArray<BAllocator, WPtr<byte>> memories)
        {
            _segmentSizePower2 = segmentSizePower2;

            _isDirties = isDirties;
            _memories = memories;
            _isValide = _segmentSizePower2 != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rebind(ref MemoryHandle memoryHandle)
        {
            if (_isValide)
            {
                var memPtr = _memories.Get(memoryHandle.chunk).Value;
                var dPtr = _isDirties.Get(memoryHandle.chunk).Value;
                memoryHandle.pointer = memPtr + (memoryHandle.segment << _segmentSizePower2);
                memoryHandle.isNotDirty = dPtr + memoryHandle.segment;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _memories.Dispose();
            _isDirties.Dispose();
        }
    }

    internal static class MemoryRebinderUtils
    {
        public const uint ALLOCATOR_COUNT = 3;

        public unsafe static MemoryRebinderContext Create(BAllocator* bAllocator, HAllocator* hAllocator)
        {
            var array = new NArray<BAllocator, MemoryRebinder>(bAllocator, ALLOCATOR_COUNT);
            array.GetRef(bAllocator->GetId()) = bAllocator->GetMemoryRebinder();
            array.GetRef(hAllocator->GetId()) = hAllocator->GetMemoryRebinder();
            return new MemoryRebinderContext(array);
        }
    }

    internal static class MemoryRebinderCaller
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rebind<T>(ref T data, ref MemoryRebinderContext rebinder)
            where T : struct, IRebindMemoryHandle
        {
            data.RebindMemoryHandle(ref rebinder);
        }
    }
}
