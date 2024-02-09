using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Allocators
{
    public unsafe struct BAllocator : IAllocator, IDisposable, ISerialize
    {
#if !ANOTHERECS_RELEASE
        private MemoryChecker<RawAllocator> _memoryChecker;
        private NDictionary<RawAllocator, uint, ulong, U4U4HashProvider> _idToDirtyPointer;
#endif
        private RawAllocator* _rawAllocator;
        private NDictionary<RawAllocator, ulong, MemEntry, U8U4HashProvider> _pointerToSize;
        private NDictionary<RawAllocator, uint, ulong, U4U4HashProvider> _idToPointer;

        private uint _counter;

        private uint _id;

        public ulong BytesAllocatedTotal
        {
            get
            {
                ulong result = 0;
                foreach (var pointer in _pointerToSize)
                {
                    result += pointer.value.size;
                }
                return result;
            }
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rawAllocator != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId()
            => _id;

        public BAllocator(uint id)
        {
            _id = id;
            _rawAllocator = UnsafeMemory.Allocate<RawAllocator>();
#if !ANOTHERECS_RELEASE
            _memoryChecker = new MemoryChecker<RawAllocator>(_rawAllocator);
            _idToDirtyPointer = new NDictionary<RawAllocator, uint, ulong, U4U4HashProvider>(_rawAllocator, 128);
#endif
            _idToPointer = new(_rawAllocator, 128);
            _pointerToSize = new(_rawAllocator, 128);
            _counter = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryHandle Allocate(uint size)
        {
            var pointer = UnsafeMemory.Allocate(size);

            ++_counter;
            _pointerToSize.Add((ulong)pointer, new MemEntry() { id = _counter, size = size });
            _idToPointer.Add(_counter, (ulong)pointer);
#if ANOTHERECS_RELEASE
            return new() { pointer = pointer, id = _counter };
#else
            return new() { pointer = pointer, id = _counter, isNotDirty = AllocateIsDirty(_counter) };
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(ref MemoryHandle memoryHandle)
        {
            _pointerToSize.Remove((ulong)memoryHandle.pointer);
            _idToPointer.Remove(memoryHandle.id);

            UnsafeMemory.Deallocate(ref memoryHandle.pointer);

#if !ANOTHERECS_RELEASE
            DeallocateIsDirty(memoryHandle.id);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reuse(ref MemoryHandle memoryHandle, uint size)
        {
            Dirty(ref memoryHandle);
            UnsafeMemory.Clear(memoryHandle.pointer, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(ref MemoryHandle memoryHandle) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResize(ref MemoryHandle memoryHandle, uint size)
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges(ref MemoryHandle memoryHandle)
        {
#if !ANOTHERECS_RELEASE
            _memoryChecker.EnterCheckChanges(ref memoryHandle);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges(ref MemoryHandle memoryHandle)
#if !ANOTHERECS_RELEASE
            => _memoryChecker.ExitCheckChanges(ref memoryHandle);
#else
            => false;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            foreach (var pointer in _pointerToSize)
            {
                UnsafeMemory.Deallocate((void*)pointer.key);
            }

#if !ANOTHERECS_RELEASE
            foreach (var pointer in _idToDirtyPointer)
            {
                UnsafeMemory.Deallocate((void*)pointer.value);
            }

            _memoryChecker.Dispose();
            _idToDirtyPointer.Dispose();
#endif
            _pointerToSize.Dispose();
            _idToPointer.Dispose();

            UnsafeMemory.Deallocate(ref _rawAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RepairMemory<BAllocator> GetRepairMemory()
            => new(this);

        public void Repair(ref MemoryHandle memoryHandle)
        {
            memoryHandle.pointer = (void*)_idToPointer[memoryHandle.id];
#if !ANOTHERECS_RELEASE
            memoryHandle.isNotDirty = (bool*)_idToDirtyPointer[memoryHandle.id];
#endif
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_id);
            writer.Write(_counter);
            writer.Write(_pointerToSize.Count);

            foreach (var element in _pointerToSize)
            {
                writer.Write(element.value.id);
                writer.Write(element.value.size);
                writer.Write((void*)element.key, element.value.size);
            }

#if !ANOTHERECS_RELEASE
            writer.Write(_idToDirtyPointer.Count);
            foreach (var element in _idToDirtyPointer)
            {
                writer.Write(element.key);
            }
#endif
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            var id = reader.ReadUInt32();
            this = new BAllocator(id);

            _counter = reader.ReadUInt32();
            var count = reader.ReadUInt32();

            for(uint i = 0; i < count; ++i)
            {
                var pointerToSizeId = reader.ReadUInt32();
                var pointerToSizeSize = reader.ReadUInt32();
                var ptr = UnsafeMemory.Allocate(pointerToSizeSize);
                reader.Read(ptr, pointerToSizeSize);

                _pointerToSize.Add((ulong)ptr, new MemEntry() { id = pointerToSizeId, size = pointerToSizeSize });
                _idToPointer.Add(pointerToSizeId, (ulong)ptr);
            }

            count = reader.ReadUInt32();

#if !ANOTHERECS_RELEASE
            for (uint i = 0; i < count; ++i)
            {
                var ptr = UnsafeMemory.Allocate(sizeof(bool));
                _idToDirtyPointer.Add(reader.ReadUInt32(), (ulong)ptr);
            }
#endif
        }

#if !ANOTHERECS_RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool* AllocateIsDirty(uint primaryMemoryId)
        {
            var pointer = UnsafeMemory.Allocate(sizeof(bool));
            _idToDirtyPointer.Add(primaryMemoryId, (ulong)pointer);
            return (bool*)pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeallocateIsDirty(uint primaryMemoryId)
        {
            UnsafeMemory.Deallocate((void*)_idToDirtyPointer[primaryMemoryId]);
            _idToDirtyPointer.Remove(primaryMemoryId);
        }
#endif

        private struct MemEntry
        {
            public uint id;
            public uint size;
        }
    }
}

