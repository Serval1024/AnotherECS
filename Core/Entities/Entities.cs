using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct Entities : ISerialize, IDisposable, IRebindMemoryHandle
    {
        internal const ushort AllocateGeneration = 32768;

        private GlobalDepencies* _depencies;
        private NContainer<HAllocator, NArray<HAllocator, EntityData>> _data;
        private NContainer<HAllocator, Recycle<uint, UintNumber>> _recycle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entities(GlobalDepencies* depencies)
        {
            _depencies = depencies;
            _data = new(&_depencies->hAllocator, new NArray<HAllocator, EntityData>(&_depencies->hAllocator, _depencies->config.general.entityCapacity));
            _recycle = new(&_depencies->hAllocator, new Recycle<EntityId, UintNumber>(&_depencies->hAllocator, _depencies->config.general.recycleCapacity));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint ReadArchetypeId(EntityId id)
            => ref _data.ReadRef().ReadRef(id).archetypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint GetArchetypeId(EntityId id)
            => ref _data.ReadRef().GetRef(id).archetypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id < GetAllocated() && IsHasRaw(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id, ushort generation)
            => IsHas(id) && ReadGeneration(id) == generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetAllocated()
            => _recycle.ReadRef().GetAllocated();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => ReadGeneration(id) >= AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadGeneration(EntityId id)
            => _data.ReadRef().ReadRef(id).generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => _recycle.ReadRef().GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => _data.ReadRef().Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeedResizeDense()
            => _data.ReadRef().Length == _recycle.ReadRef().GetAllocated();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
        {
            if (IsNeedResizeDense())
            {
                _data.GetRef().Resize(_data.ReadRef().Length << 1);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate()
        {
            var id = _recycle.GetRef().Allocate();
            ref var head = ref _data.ReadRef().GetRef(id);

            head.generation += AllocateGeneration + 1;
            if (head.generation == ushort.MaxValue)
            {
                head.generation = AllocateGeneration;
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
        {
            ref var head = ref _data.ReadRef().GetRef(id);
            head.generation -= AllocateGeneration;
            _recycle.GetRef().Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            if (_depencies->archetype.GetFilterZeroCount() != 0)
            {
                const int idsLength = 32;
                var ids = stackalloc EntityId[idsLength];
                var count = _depencies->archetype.FilterZero(ids, idsLength);

                for (int i = 0; i < count; ++i)
                {
                    var id = ids[i];
                    ref var head = ref _data.ReadRef().GetRef(id);
                    head.generation -= AllocateGeneration;
                    _recycle.GetRef().Deallocate(id);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly EntityData ReadRef(uint id)
            => ref _data.ReadRef().ReadRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EntityData GetRef(uint id)
            => ref _data.ReadRef().GetRef(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
            _recycle.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
            _recycle.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _depencies = reader.GetDepency<NPtr<GlobalDepencies>>().Value;
            _data.Unpack(ref reader);
            _recycle.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
            MemoryRebinderCaller.Rebind(ref _recycle, ref rebinder);
    }
    }
}