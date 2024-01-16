﻿using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using AnotherECS.Core.Collection;
using AnotherECS.Core.Threading;
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

        private uint _id;

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
            => _id;

        public BAllocator(uint id)
        {
            _id = id;
            _rawAllocator = UnsafeMemory.Allocate<RawAllocator>();
#if !ANOTHERECS_RELEASE
            _memoryChecker = new MemoryChecker<RawAllocator>(_rawAllocator);
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

            var c = (ushort)(_counter & 0xffff);
            var s = (ushort)(_counter >> 16);
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
            writer.Write(_id);
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
            var id = reader.ReadUInt32();
            this = new BAllocator(id);

            _counter = reader.ReadUInt32();
            var count = reader.ReadUInt32();
            for(uint i = 0; i < count; ++i)
            {
                var pointerToSizeId = reader.ReadUInt32();
                var pointerToSizeSize = reader.ReadUInt32();
                var ptr = _rawAllocator->Allocate(pointerToSizeSize).GetPtr();
                reader.Read(ptr, pointerToSizeSize);

                _pointerToSize.Add((ulong)ptr, new MemEntry() { id = id, size = pointerToSizeSize });
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

