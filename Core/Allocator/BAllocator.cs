using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    public unsafe struct BAllocator : IAllocator, IDisposable, ISerialize
    {
#if !ANOTHERECS_RELEASE
        private MemoryChecker<RawAllocator> _memoryChecker;
#endif
        private RawAllocator* _rawAllocator;
        private NDictionary<RawAllocator, ulong, MemEntry, U8U4HashProvider> _pointerToSize;
        private NDictionary<RawAllocator, uint, ulong, U4U4HashProvider> _idToPointer;
        private uint _counter;

        public ulong TotalBytesAllocated
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId()
            => 1;

        public static BAllocator Create()
        {
            BAllocator allocator;
            allocator._rawAllocator = UnsafeMemory.Allocate<RawAllocator>();
#if !ANOTHERECS_RELEASE
            allocator._memoryChecker = new MemoryChecker<RawAllocator>(allocator._rawAllocator);
#endif
            allocator._idToPointer = new(allocator._rawAllocator, 128);
            allocator._pointerToSize = new(allocator._rawAllocator, 128);
            allocator._counter = 0;

            return allocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryHandle Allocate(uint size)
        {
            ++_counter;
            var c = (ushort)(_counter & 0xffff);
            var s = (ushort)(_counter >> 16);

            var pointer = UnsafeMemory.Allocate(size);
            _pointerToSize.Add((ulong)pointer, new MemEntry() { id = _counter, size = size });
            _idToPointer.Add(_counter, (ulong)pointer);

            return new() { pointer = pointer, chunk = c, segment = s };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(ref MemoryHandle memoryHandle)
        {
            _pointerToSize.Remove((ulong)memoryHandle.pointer);
            _idToPointer.Remove(GetId(ref memoryHandle));

            UnsafeMemory.Deallocate(ref memoryHandle.pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetId(ref MemoryHandle memoryHandle)
            => memoryHandle.chunk | (uint)(memoryHandle.segment << 16);

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
#if !ANOTHERECS_RELEASE
            _memoryChecker.Dispose();
#endif
            RawAllocator rawAllocator = default;
            foreach (var pointer in _pointerToSize)
            {
                rawAllocator.Deallocate((void*)pointer.key);
            }

            _pointerToSize.Dispose();
            _idToPointer.Dispose();
            UnsafeMemory.Deallocate(ref _rawAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MemoryRebinder GetMemoryRebinder()
            => new(0, default, default);

        public void Repair(ref MemoryHandle memoryHandle)
        {
            memoryHandle.pointer = (void*)_idToPointer[GetId(ref memoryHandle)];
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_counter);
            writer.Write(_pointerToSize.Count);
            foreach (var element in _pointerToSize)
            {
                writer.Write(element.value.id);
                writer.Write(element.value.size);
                writer.Write((void*)element.key, element.value.size);
            }
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            this = Create();

            _counter = reader.ReadUInt32();
            var count = reader.ReadUInt32();
            for(uint i = 0; i < count; ++i)
            {
                var id = reader.ReadUInt32();
                var size = reader.ReadUInt32();
                var ptr = _rawAllocator->Allocate(size).GetPtr();
                reader.Read(ptr, size);

                _pointerToSize.Add((ulong)ptr, new MemEntry() { id = id, size = size });
                _idToPointer.Add(id, (ulong)ptr);
            }
        }

        private struct MemEntry
        {
            public uint id;
            public uint size;
        }
    }
}

