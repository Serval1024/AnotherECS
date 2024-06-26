﻿using AnotherECS.Core.Allocators;
using AnotherECS.Core.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    [System.Diagnostics.DebuggerTypeProxy(typeof(NContainer<,>.NContainerDebugView))]
    public unsafe struct NContainer<TAllocator, T> : INative, ISerialize, IRepairMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        private TAllocator* _allocator;
        private MemoryHandle _data;

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }

        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        public NContainer(TAllocator* allocator)
        {
            _allocator = allocator;
            _data = default;
        }

        public NContainer(TAllocator* allocator, T data)
        {
            _allocator = allocator;
            _data = _allocator->Allocate((uint)sizeof(T));
            *(T*)_data.pointer = data;
        }

        public NContainer(TAllocator* allocator, ref MemoryHandle memoryHandle)
        {
            _allocator = allocator;
            _data = memoryHandle;
            allocator->Repair(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate()
        {
            Deallocate();
            _data = _allocator->Allocate((uint)sizeof(T));
            *(T*)_data.pointer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(ref T data)
        {
            Allocate();
            Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(T data)
        {
            Allocate(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate()
        {
            _allocator->Deallocate(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() 
        {
            if (_data.IsValid)
            {
                Deallocate();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNContainerBroken(this);
#endif
            return (T*)_data.pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef()
           => ref *ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
          => *ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef()
            => ref *GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNContainerBroken(this);
#endif
            Dirty();
            return ReadPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ref T data)
        {
            GetRef() = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T data)
        {
            Set(ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty()
        {
            _allocator->Dirty(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            CompoundMeta _compound = default;

            _data.Pack(ref writer);

            if (_data.IsValid)
            {
                writer.Write(_allocator->GetId());

                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    ((ISerialize)ReadRef()).Pack(ref writer);
                }
                else
                {
                    if (writer.GetSerializer(typeof(T), out var serializer))
                    {
                        serializer.Pack(ref writer, ReadRef());
                    }
                    else
                    {
                        _compound.Pack(ref writer, ReadRef());
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            CompoundMeta _compound = default;
            MemoryHandle memoryHandle = default;
            memoryHandle.Unpack(ref reader);

            if (memoryHandle.id != 0)
            {
                uint allocatorId = reader.ReadUInt32();
                this = new NContainer<TAllocator, T>(reader.Dependency.DirectGet<WPtr<TAllocator>>(allocatorId).Value, ref memoryHandle);

                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    var serialize = new T() as ISerialize;
                    serialize.Unpack(ref reader);
                    Set((T)serialize);
                }
                else
                {
                    if (reader.GetSerializer(typeof(T), out var serializer))
                    {
                        Set((T)serializer.Unpack(ref reader, null)); 
                    }
                    else
                    {
                        Set((T)_compound.Unpack(ref reader, typeof(T)));
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            if (IsValid)
            {
                repairMemoryContext.Repair(_allocator->GetId(), ref _data);
                RepairMemoryHandleElement(ref repairMemoryContext);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RepairMemoryHandleElement(ref RepairMemoryContext repairMemoryContext)
        {
            if (typeof(T) is IRepairMemoryHandle)
            {
                var rmh = (IRepairMemoryHandle)ReadRef();
                rmh.RepairMemoryHandle(ref repairMemoryContext);
                ReadRef() = (T)rmh;
            }
        }


        private class NContainerDebugView
        {
            private NContainer<TAllocator, T> container;
            public NContainerDebugView(NContainer<TAllocator, T> container)
            {
                this.container = container;
            }

            public bool IsValid
                => container.IsValid;

            public T Data
                => IsValid ? container.ReadRef() : default;
        }
    }
}
