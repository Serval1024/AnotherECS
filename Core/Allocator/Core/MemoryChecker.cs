using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;

namespace AnotherECS.Core
{
    internal unsafe struct MemoryChecker<TAllocator> : IDisposable
        where TAllocator : unmanaged, IAllocator
    {
        private const uint INIT_CAPACITY = 32;

        private NDictionary<TAllocator, ulong, CheckEntry, U8U4HashProvider> _memoryChecks;


        public MemoryChecker(TAllocator* allocator)
        {
            _memoryChecks = new NDictionary<TAllocator, ulong, CheckEntry, U8U4HashProvider>(allocator, INIT_CAPACITY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _memoryChecks.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges(ref MemoryHandle memoryHandle)
        {
            if (memoryHandle.IsValid)
            {
                var key = (ulong)memoryHandle.pointer;
                if (_memoryChecks.ContainsKey(key))
                {
                    var value = _memoryChecks[key];
                    ++value.blockCounter;
                    _memoryChecks[key] = value;
                }
                else
                {
                    _memoryChecks.Add(key, new CheckEntry() { blockCounter = 0, restoreValueIsDirty = memoryHandle.IsDirty });
                    *memoryHandle.isNotDirty = false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges(ref MemoryHandle memoryHandle)
        {
            if (memoryHandle.IsValid)
            {
                var key = (ulong)memoryHandle.pointer;
                if (_memoryChecks.TryGetValue(key, out CheckEntry checkEntry))
                {
                    if (checkEntry.blockCounter == 0)
                    {
                        _memoryChecks.Remove(key);

                        var currentIsDirty = *memoryHandle.isNotDirty;
                        *memoryHandle.isNotDirty = checkEntry.restoreValueIsDirty;

                        return currentIsDirty;
                    }
                    else
                    {
                        var value = _memoryChecks[key];
                        --value.blockCounter;
                        _memoryChecks[key] = value;
                    }
                }
            }
            return false;
        }

        private struct CheckEntry
        {
            public uint blockCounter;
            public bool restoreValueIsDirty;
        }
    }

}

