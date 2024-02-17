using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct Archetype : IRepairMemoryHandle, ISerialize
    {
        public const int ARCHETYPE_COUNT = 1024;

        private const uint TRANSITION_INIT_CAPACITY = 32;
        private const uint CHANGE_INIT_CAPACITY = 32;

        private Dependencies* _dependencies;
        private Set<BAllocator, HAllocator> _set;

        private NList<BAllocator, MoveCollection> _temporaries;

        private NDictionary<BAllocator, ulong, uint, U8U4HashProvider> _transitionAddCache;
        private NDictionary<BAllocator, ulong, uint, U8U4HashProvider> _transitionRemoveCache;
        private NBuffer<BAllocator, BufferEntry> _changesBuffer;
        private NHashSetZero<BAllocator, uint, U4U4HashProvider> _isTemporaries;

        private int locked;

        public bool IsLocked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => locked != 0;
        }

        public Archetype(Dependencies* dependencies, INArray<uint> isTemporaries)
        {
            _dependencies = dependencies;

            var commonAllocator = &dependencies->bAllocator;
            var collectionAllocator = &dependencies->stage1HAllocator;
            _transitionAddCache = new NDictionary<BAllocator, ulong, uint, U8U4HashProvider>(commonAllocator, TRANSITION_INIT_CAPACITY);
            _transitionRemoveCache = new NDictionary<BAllocator, ulong, uint, U8U4HashProvider>(commonAllocator, TRANSITION_INIT_CAPACITY);
            _changesBuffer = new NBuffer<BAllocator, BufferEntry>(commonAllocator, CHANGE_INIT_CAPACITY);

            _isTemporaries = new NHashSetZero<BAllocator, uint, U4U4HashProvider>(commonAllocator, isTemporaries);
            _temporaries = new NList<BAllocator, MoveCollection>(commonAllocator, _isTemporaries.Count);

            locked = 0;

            _set = new Set<BAllocator, HAllocator>(commonAllocator, collectionAllocator, _dependencies->componentTypesCount + 1, _dependencies->config.general.archetypeCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id)
        {
            Add(0, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint archetypeId, uint id)
        {
            _set.Add(archetypeId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, uint itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            if (IsLocked)
            {
                _changesBuffer.Push(
                    new BufferEntry()
                    {
                        isAdd = true,
                        isTemporary = isTemporary,
                        entityId = id,
                        itemId = itemId
                    }
                );
            }
            else
            {
                AddInternal(ref filterUpdater, id, itemId, isTemporary);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint archetypeId, uint id)
        {
            _set.Remove(archetypeId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, uint itemId)
            where TFilterUpdater : struct, IFilterUpdater
        {
            if (IsLocked)
            {
                _changesBuffer.Push(new BufferEntry() { isAdd = false, entityId = id, itemId = itemId });
            }
            else
            {
                RemoveInternal(ref filterUpdater, id, itemId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create<TFilterUpdater>(ref TFilterUpdater filterUpdater, Span<uint> itemIds)
            where TFilterUpdater : struct, IFilterUpdater
        {
            SetObserver<TFilterUpdater> observer;
            observer.filterUpdater = filterUpdater;
            observer.isTemporaries = _isTemporaries;
            observer.temporaries = _temporaries;

            _set.Create(ref observer, itemIds);

            observer.Apply(ref _temporaries);
        }

        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(uint archetypeId)
            => _set.GetCount(archetypeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachItem<TIterator>(uint startArchetypeId, TIterator iterator)
            where TIterator : struct, IIterator<uint>
        {
            _set.ForEachItem(startArchetypeId, iterator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasItem(uint archetypeId, uint itemId)
            => _set.IsHasItem(archetypeId, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemIds(uint startArchetypeId, uint* result, uint resultLength)
            => _set.GetItemIds(startArchetypeId, result, resultLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemId(uint archetypeId, uint index)
            => _set.GetItemId(archetypeId, index);

        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, uint> Filter(BAllocator* allocator, Span<uint> includes, Span<uint> excludes)
        {
            Span<uint> archetypeIds = stackalloc uint[ARCHETYPE_COUNT];
            var count = _set.Filter(includes, excludes, archetypeIds);
            return archetypeIds[..count].ToNArray(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetFilterZeroCount()
            => _set.GetFilterZeroCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FilterZero(uint* result, int count)
            => _set.FilterZero(result, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllTemporary()
        {
            var dense = _temporaries.GetPtr();
            for (uint i = 1, iMax = _temporaries.Count; i < iMax; ++i)
            {
                ref var element = ref _temporaries.GetRef(i);
                ref var from = ref _set.GetIdCollection(element.fromCollectionId);

                if (from.Count != 0)
                {
                    ref var to = ref _set.GetIdCollection(element.toCollectionId);
                    foreach (var id in from)
                    {
                        to.Add(id);
                        _dependencies->entities.GetRef(id).archetypeId = element.toArchetypeId;
                    }
                    from.Clear();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            RemoveAllTemporary();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly IdCollection<HAllocator> ReadIdCollection(uint archetypeId)
            => ref _set.ReadIdCollection(archetypeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _set.Dispose();
            _temporaries.Dispose();

            _transitionAddCache.Dispose();
            _transitionRemoveCache.Dispose();
            _changesBuffer.Dispose();
            _isTemporaries.Dispose();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _set.Pack(ref writer);
            _temporaries.PackBlittable(ref writer);

            _transitionAddCache.PackBlittable(ref writer);
            _transitionRemoveCache.PackBlittable(ref writer);
            _changesBuffer.PackBlittable(ref writer);
            _isTemporaries.PackBlittable(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _dependencies = reader.Dependency.Get<WPtr<Dependencies>>().Value;
            
            _set.Unpack(ref reader);
            _temporaries.UnpackBlittable(ref reader);

            _transitionAddCache.UnpackBlittable(ref reader);
            _transitionRemoveCache.UnpackBlittable(ref reader);
            _changesBuffer.UnpackBlittable(ref reader);
            _isTemporaries.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lock()
        {
            ++locked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Unlock<TFilterUpdater>(ref TFilterUpdater filterUpdater)
            where TFilterUpdater : struct, IFilterUpdater
        {
            --locked;
            if (locked == 0)
            {
                PushData(ref filterUpdater);
            }
        }

        internal void PushData<TFilterUpdater>(ref TFilterUpdater filterUpdater)
            where TFilterUpdater : struct, IFilterUpdater
        {
            while (!_changesBuffer.IsEmpty)
            {
                var element = _changesBuffer.Pop();
                if (element.isAdd)
                {
                    AddInternal(ref filterUpdater, element.entityId, element.itemId, element.isTemporary);
                }
                else
                {
                    RemoveInternal(ref filterUpdater, element.entityId, element.itemId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, uint itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref uint archetypeId = ref _dependencies->entities.GetRef(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionAddCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                _set.Move(archetypeId, newArchetypeId, id);
            }
            else
            {
                SetObserver<TFilterUpdater> observer;
                observer.filterUpdater = filterUpdater;
                observer.isTemporaries = _isTemporaries;
                observer.temporaries = _temporaries;

                newArchetypeId = _set.Add(ref observer, archetypeId, id, itemId);

                observer.Apply(ref _temporaries);

                _transitionAddCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, uint itemId)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref uint archetypeId = ref _dependencies->entities.GetRef(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionRemoveCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                _set.Move(archetypeId, newArchetypeId, id);
            }
            else
            {
                SetObserver<TFilterUpdater> observer;
                observer.filterUpdater = filterUpdater;
                observer.isTemporaries = _isTemporaries;
                observer.temporaries = _temporaries;

                newArchetypeId = _set.Remove(ref observer, archetypeId, id, itemId);

                observer.Apply(ref _temporaries);

                _transitionRemoveCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _set, ref repairMemoryContext);
        }

        private struct SetObserver<TFilterUpdater> : Set<BAllocator, HAllocator>.IObserver
            where TFilterUpdater : struct, IFilterUpdater
        {
            public TFilterUpdater filterUpdater;
            public NHashSetZero<BAllocator, uint, U4U4HashProvider> isTemporaries;
            public NList<BAllocator, MoveCollection> temporaries;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, uint itemId)
                where TNArray : struct, INArray<Node>
            {
                filterUpdater.AddToFilterData(ref archetypes, archetypeId, toAddArchetypeId, itemId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddFinished(WArray<Node> nodes, ref Node node, uint itemId)
            {
                filterUpdater.EndToFilterData();

                if (isTemporaries.Contains(itemId))
                {
                    var collectionId = node.collectionId;
                    var toId = FindUpNonTemporary(ref nodes, ref node);
                    temporaries.Add(
                        new MoveCollection()
                        {
                            fromCollectionId = collectionId,
                            toCollectionId = toId.collectionId,
                            toArchetypeId = toId.archetypeId,
                        }
                        );
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Apply(ref NList<BAllocator, MoveCollection> temporaries)
            {
                temporaries = this.temporaries;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ref Node FindUpNonTemporary(ref WArray<Node> nodes, ref Node startNode)
            {
                var currentId = startNode.parent;
                while (currentId != 0)
                {
                    ref var node = ref nodes.GetRef(currentId);
                    if (isTemporaries.Contains(node.itemId))
                    {
                        currentId = nodes.GetRef(currentId).parent;
                    }
                    else
                    {
                        break;
                    }
                }
                return ref nodes.GetRef(currentId);
            }
        }


        private struct BufferEntry
        {
            public bool isAdd;
            public bool isTemporary;
            public uint entityId;
            public uint itemId;
        }

        internal struct MoveCollection
        {
            public uint fromCollectionId;
            public uint toCollectionId;
            public uint toArchetypeId;
        }
    }
}