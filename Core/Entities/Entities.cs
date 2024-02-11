using AnotherECS.Core.Allocators;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct Entities : ISerialize, IDisposable, IRepairMemoryHandle
    {
        internal const ushort AllocateGeneration = 32768;

        private Dependencies* _dependencies;
        private NContainer<HAllocator, NArray<HAllocator, EntityData>> _data;
        private NContainer<HAllocator, URecycle<uint, UintNumber>> _recycle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entities(Dependencies* dependencies)
        {
            _dependencies = dependencies;
            _data = new(&_dependencies->stage1HAllocator, new NArray<HAllocator, EntityData>(&_dependencies->stage1HAllocator, _dependencies->config.general.entityCapacity));
            _recycle = new(&_dependencies->stage1HAllocator, new URecycle<EntityId, UintNumber>(&_dependencies->stage1HAllocator, _dependencies->config.general.recycleCapacity));
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
            head.archetypeId = 0;
            _recycle.GetRef().Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            if (_dependencies->archetype.GetFilterZeroCount() != 0)
            {
                const int idsLength = 32;
                var ids = stackalloc EntityId[idsLength];
                var count = _dependencies->archetype.FilterZero(ids, idsLength);

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
            _dependencies = reader.Dependency.Get<WPtr<Dependencies>>().Value;
            _data.Unpack(ref reader);
            _recycle.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _data, ref repairMemoryContext);
            RepairMemoryCaller.Repair(ref _recycle, ref repairMemoryContext);
    }
    }
}