using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using AnotherECS.Converter;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    using NodeCaller = Caller<
                uint, Node, uint, TOData<uint>, uint,
                UintNumber,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                IncrementStorageFeature<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                NonSparseFeature<Node, TOData<uint>, uint>,
                ArchetypeDenseFeature<uint, Node, TOData<uint>>,
                Nothing,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                Nothing<uint, Node, uint, TOData<uint>, uint>,
                BBSerialize<uint, Node, uint, TOData<uint>>,
                Nothing<uint, Node, uint, TOData<uint>, uint>
                >;

    using CollectionCaller = Caller<
                uint, IdCollection, uint, TIOData<uint>, uint,
                UintNumber,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
                IncrementStorageFeature<uint, IdCollection, uint, TIOData<uint>, uint>,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
                NonSparseFeature<IdCollection, TIOData<uint>, uint>,
                ArchetypeDenseFeature<uint, IdCollection, TIOData<uint>>,
                Nothing,
                CopyableFeature<IdCollection>,
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
#if ANOTHERECS_HISTORY_DISABLE
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>,
#else
                ByManualSegmentHistoryFeature<uint, IdCollection, uint, uint>,
#endif
                SBSerialize<uint, IdCollection, uint, TIOData<uint>>,
#if ANOTHERECS_HISTORY_DISABLE
                Nothing<uint, IdCollection, uint, TIOData<uint>, uint>
#else
                ByManualSegmentHistoryFeature<uint, IdCollection, uint, uint>
#endif
                >;

    using TemporaryCaller = Caller<
                uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection,
                UintNumber,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                IncrementStorageFeature<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                NonSparseFeature<MoveCollection, TOData<MoveCollection>, MoveCollection>,
                UintDenseFeature<uint, MoveCollection, TOData<MoveCollection>>,
                Nothing,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>,
                BBSerialize<uint, MoveCollection, uint, TOData<MoveCollection>>,
                Nothing<uint, MoveCollection, uint, TOData<MoveCollection>, MoveCollection>
                >;

#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct ArchetypeCaller : ICaller, ITickFinishedCaller
    {
        private GlobalDepencies* _depencies;

        private NodeCaller _node;
        private CollectionCaller _collection;
        private TemporaryCaller _temporary;

        private NDictionary<ulong, uint, U8U8HashProvider> _transitionAddCache;
        private NDictionary<ulong, uint, U8U8HashProvider> _transitionRemoveCache;
        private NBuffer<BufferEntry> _bufferChange;

        private Context _context;
        private int locked;

        
        public bool IsLocked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => locked != 0; 
        }

        public ushort ElementId => 0;
        public bool IsValide => _node.IsValide && _collection.IsValide;
        public bool IsSingle => false;
        public bool IsRevert => _collection.IsRevert;
        public bool IsTickFinished => true;
        public bool IsSerialize => false;
        public bool IsResizable => false;
        public bool IsAttach => false;
        public bool IsDetach => false;
        public bool IsInject => false;
        public bool IsTemporary => false;



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            _depencies = depencies;

            _node = default;
            var nodeLayout = (UnmanagedLayout<uint, Node, uint, TOData<uint>>*) layout;
            CallerWrapper.Config<NodeCaller, Node>(ref _node, layout, depencies, id, state);

            _transitionAddCache = new NDictionary<ulong, uint, U8U8HashProvider>(32);
            _transitionRemoveCache = new NDictionary<ulong, uint, U8U8HashProvider>(32);
            _bufferChange = new NBuffer<BufferEntry>(32);

            _context = new Context()
            {
                collection = _collection,
                temporary = _temporary,

                archetypes = &nodeLayout->storage.dense,
                archetypeCount = &_collection.GetLayout()->storage.denseIndex,
                isTemporary = new NHashSet<ushort, U2U4HashProvider>(state.GetTemporary()),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            CallerWrapper.AllocateLayout<NodeCaller, Node>(ref _node);
            ArchetypeActions.SetupRootArchetype(ref *_context.archetypes);
            ArchetypeActions.SetupCollection(ref _context.collection.GetLayout()->storage.dense, _context.collection.GetLayout()->storage.denseIndex, _depencies->config.general.filterCapacity);
            ArchetypeActions.SetupTemporary(ref *_context.archetypes, ref _context.temporary, ref _context.isTemporary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id)
            => ArchetypeActions.Add(ref _collection, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref FilterUpdater filterUpdater, uint id, ushort itemId, bool isTemporary)
        {
            if (IsLocked)
            {
                _bufferChange.Push(new BufferEntry() { isAdd = true, isTemporary = isTemporary, id = id, itemId = itemId });
            }
            else
            {
                AddInternal(ref filterUpdater, id, itemId, isTemporary);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ref FilterUpdater filterUpdater, uint id, ushort itemId)
        {
            if (IsLocked)
            {
                _bufferChange.Push(new BufferEntry() { isAdd = false, id = id, itemId = itemId });
            }
            else
            {
                RemoveInternal(ref filterUpdater, id, itemId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint archetypeId, uint id)
        {
            ArchetypeActions.Remove(ref *_context.archetypes, ref _collection, archetypeId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create(ref FilterUpdater filterUpdater, Span<ushort> itemIds)
        {
            ArchetypeActions.Create(ref _context, ref filterUpdater, itemIds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(uint archetypeId)
            => ArchetypeActions.GetCount(ref *_context.archetypes, archetypeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemIds(uint archetypeId, uint* result, uint resultCount) 
            => ArchetypeActions.GetItemIds(ref *_context.archetypes, archetypeId, result, resultCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasItem(uint archetypeId, ushort itemId)
            => ArchetypeActions.IsHasItem(ref *_context.archetypes, archetypeId, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemId(uint archetypeId, uint index)
            => ArchetypeActions.GetItemId(ref *_context.archetypes, archetypeId, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly IdCollection GetIdCollection(uint archetypeId)
            => ref _collection.Read(_context.archetypes->GetRef(archetypeId).collectionId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<uint> Filter(Span<ushort> includes, Span<ushort> excludes)
            => ArchetypeActions.Filter(ref *_context.archetypes, includes, excludes);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Filter(Span<ushort> includes, Span<ushort> excludes, int itemCount, Span<uint> result)
            => ArchetypeActions.Filter(ref *_context.archetypes, includes, excludes, result);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetFilterZeroCount()
            => ArchetypeActions.GetFilterZeroCount(ref _collection);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FilterZero(uint* result, int count)
            => ArchetypeActions.FilterZero(ref _collection, result, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllTemporary()
        {
            ArchetypeActions.RemoveAllTemporary(ref _depencies->entities, ref _context.collection, ref _context.temporary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            RemoveAllTemporary();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _node.Dispose();
            _transitionAddCache.Dispose();
            _transitionRemoveCache.Dispose();
            _bufferChange.Dispose();
            _context.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lock()
        {
            ++locked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Unlock(ref FilterUpdater filterUpdater)
        {
            --locked;
            if (locked == 0)
            {
                PushData(ref filterUpdater);
            }
        }

        internal void PushData(ref FilterUpdater filterUpdater)
        {
            while(!_bufferChange.IsEmpty)
            {
                var element = _bufferChange.Pop();
                if (element.isAdd)
                {
                    AddInternal(ref filterUpdater, element.id, element.itemId, element.isTemporary);
                }
                else
                {
                    RemoveInternal(ref filterUpdater, element.id, element.itemId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(ref FilterUpdater filterUpdater, uint id, ushort itemId, bool isTemporary)
        {
            ref uint archetypeId = ref _depencies->entities.Get(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionAddCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                ArchetypeActions.Move(ref *_context.archetypes, ref _collection, archetypeId, newArchetypeId, id);
            }
            else
            {
                newArchetypeId = ArchetypeActions.Add(ref _context, ref filterUpdater, archetypeId, id, itemId, isTemporary);
                _transitionAddCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveInternal(ref FilterUpdater filterUpdater, uint id, ushort itemId)
        {
            ref uint archetypeId = ref _depencies->entities.Get(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionRemoveCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                ArchetypeActions.Move(ref *_context.archetypes, ref _collection, archetypeId, newArchetypeId, id);
            }
            else
            {
                newArchetypeId = ArchetypeActions.Remove(ref _context, ref filterUpdater, archetypeId, id, itemId);
                _transitionRemoveCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        public Node Create()
        {
            throw new NotSupportedException();
        }

        public Type GetElementType()
        {
            throw new NotSupportedException();
        }

        public void Remove(uint id)
        {
            throw new NotSupportedException();
        }

        public void RemoveRaw(uint id)
        {
            throw new NotSupportedException();
        }

        public IComponent GetCopy(uint id)
        {
            throw new NotSupportedException();
        }

        public void Set(uint id, IComponent data)
        {
            throw new NotSupportedException();
        }

        public static class LayoutInstaller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ArchetypeCaller Install(State state)
            {
                var collectionCaller = state.AddLayout<CollectionCaller>();
                var temporaryCaller = state.AddLayout<TemporaryCaller>();

                var archetypeCaller = new ArchetypeCaller
                {
                    _collection = collectionCaller,
                    _temporary = temporaryCaller,
                };
                
                return state.AddLayout(archetypeCaller);
            }
        }

        private struct BufferEntry
        {
            public bool isAdd;
            public bool isTemporary;
            public uint id;
            public ushort itemId;
        }
    }

    [IgnoreCompile]
    internal unsafe struct Node : IComponent
    {
        public const int ChildenMax = 16;

        public uint parent;
        public uint archetypeId;
        public ushort itemId;
        public uint collectionId;
        public uint hash;
        public int childenCount;
        public fixed uint childen[Node.ChildenMax];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(uint nodeId)
        {
#if !ANOTHERECS_RELEASE
            if (childenCount == ChildenMax)
            {
                throw new InvalidOperationException();
            }
#endif
            childenCount = CapacityChildenAsSpan().TryAddSort(childenCount, nodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<uint> CapacityChildenAsSpan()
            => new(UnsafeUtils.AddressOf(ref childen[0]), ChildenMax);
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [IgnoreCompile]
    internal unsafe struct IdCollection : IComponent, IManualRevert<uint>, ISerialize, IDisposable, ICopyable<IdCollection>, IEnumerable<uint>
    {
        private NHashSetUintId _data;
        private uint _componentId;

        public uint Count
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IdCollection(uint capacity, uint componentId)
        {
            _data = new NHashSetUintId(32);
            _componentId = componentId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id, ref CollectionCaller collection)
        {
            _data.Add(id, ref collection, _componentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id, ref CollectionCaller collection)
        {
            _data.Remove(id, ref collection, _componentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRevert(uint index, uint segment)
        {
            _data.OnRevert(index, segment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.Unpack(ref reader);
        }

        public void CopyFrom(in IdCollection other)
        {
            throw new NotSupportedException();
        }

        public void OnRecycle()
        {
            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSetUintId.Enumerator GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => _data.GetEnumerator();
    }


#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal static unsafe class ArchetypeActions
    {
        public const int FIND_DEEP = 1024;
        public const int ARCHETYPE_COUNT = 1024;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupRootArchetype(ref NArray<Node> archetypes)
        {
            for (uint i = 0, iMax = archetypes.Length; i < iMax; ++i)
            {
                ref var archetype = ref archetypes.GetRef(i);
                archetype.archetypeId = i;
                archetype.itemId = (ushort)i;
                archetype.collectionId = i;
                archetype.hash = GetHash(ref archetypes, i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupTemporary(ref NArray<Node> archetypes, ref TemporaryCaller temporary, ref NHashSet<ushort, U2U4HashProvider> isTemporary)
        {
            for (ushort i = 0, iMax = (ushort)archetypes.Length; i < iMax; ++i)
            {
                if (isTemporary.Contains(i))
                {
                    temporary.Set(temporary.UnsafeAllocate(), new MoveCollection() { fromCollectionId = archetypes.GetRef(i).collectionId, toCollectionId = 0 });
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupCollection(ref NArray<IdCollection> items, uint backetCapacity, uint itemCapacity)
        {
            for (uint i = 0; i < backetCapacity; ++i)
            {
                items.Set(i, new IdCollection(itemCapacity, i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref CollectionCaller collection, uint id)
        {
            collection.UnsafeDirectRead(0).Add(id, ref collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add(ref Context context, ref FilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId, bool isTemporary)
            => (archetypeId == 0)
               ? AddInternal(ref context, id, itemId)
               : AddInternal(ref context, ref filterUpdater, archetypeId, id, itemId, isTemporary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remove(ref Context context, ref FilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId)
            => RemoveInternal(ref context, ref filterUpdater, archetypeId, id, itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref NArray<Node> archetypes, ref CollectionCaller collection, uint archetypeId, uint id)
        {
            collection.UnsafeDirectRead(archetypes.GetRef(archetypeId).collectionId).Remove(id, ref collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAllTemporary(ref EntitiesCaller entities, ref CollectionCaller collection, ref TemporaryCaller temporary)
        {
            var storage = temporary.GetLayout()->storage;
            var dense = temporary.GetLayout()->storage.dense;
            for (uint i = 1, iMax = storage.denseIndex; i < iMax; ++i)
            {
                ref var element = ref dense.GetRef(i);
                ref var from = ref collection.UnsafeDirectRead(element.fromCollectionId);

                if (from.Count != 0)
                {
                    ref var to = ref collection.UnsafeDirectRead(element.toCollectionId);
                    foreach (var id in from)
                    {
                        to.Add(id, ref collection);
                        entities.Get(id).archetypeId = element.toArchetypeId;
                    }
                    from.Clear();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveCollection(ref CollectionCaller collection, uint fromCollectionId, uint toCollectionId, uint id)
        {
            collection.UnsafeDirectRead(fromCollectionId).Remove(id, ref collection);
            collection.UnsafeDirectRead(toCollectionId).Add(id, ref collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(ref NArray<Node> archetypes, ref CollectionCaller collection, uint fromArchetypeId, uint toArchetypeId, uint id)
        {
            MoveCollection(ref collection, archetypes.GetRef(fromArchetypeId).collectionId, archetypes.GetRef(toArchetypeId).collectionId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount(ref NArray<Node> archetypes, uint archetypeId)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ++count;
                archetypeId = archetypes.GetRef(archetypeId).parent;
            }
            while (archetypeId != 0);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetItemId(ref NArray<Node> archetypes, uint archetypeId, uint index)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ref var node = ref archetypes.GetRef(archetypeId);
                if (count == index)
                {
                    return node.itemId;
                }
                ++count;
                archetypeId = node.parent;
            }
            while (archetypeId != 0);

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHasItem(ref NArray<Node> archetypes, uint archetypeId, ushort itemId)
        {
            if (archetypeId != 0)
            {
                do
                {
                    ref var node = ref archetypes.GetRef(archetypeId);

                    if (node.itemId == itemId)
                    {
                        return true;
                    }
                    archetypeId = node.parent;
                }
                while (archetypeId != 0);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetItemIds(ref NArray<Node> archetypes, uint archetypeId, uint* result, uint resultLength)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ref var node = ref archetypes.GetRef(archetypeId);
#if !ANOTHERECS_RELEASE
                if (count == resultLength)
                {
                    throw new Exceptions.FindIdsException(resultLength);
                }
#endif
                result[count++] = node.itemId;
                archetypeId = node.parent;
            }
            while (archetypeId != 0);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NArray<uint> Filter(ref NArray<Node> archetypes, Span<ushort> includes, Span<ushort> excludes)
        {
            Span<uint> archetypeIds = stackalloc uint[ARCHETYPE_COUNT];
            var count = Filter(ref archetypes, includes, excludes, archetypeIds);
            return archetypeIds[..count].ToNArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Filter(ref NArray<Node> archetypes, Span<ushort> includes, Span<ushort> excludes, Span<uint> result)
        {
            int resultCount = 0;
            var items0 = includes[0];
            for (uint i = 1; i <= items0; ++i)
            {
                FindPattern(ref archetypes, ref archetypes.GetRef(i), 0, includes, excludes, result, ref resultCount);
            }

            PatternDownExtend(ref archetypes, result, ref resultCount, excludes);

            result.Sort(0, resultCount);
            return resultCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetFilterZeroCount(ref CollectionCaller collection)
            => collection.UnsafeDirectRead(0).Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FilterZero(ref CollectionCaller collection, uint* result, int countMax)
        {
#if !ANOTHERECS_RELEASE
            if (countMax == 0)
            {
                throw new ArgumentException(nameof(countMax));
            }
#endif
            ref var idSet = ref collection.UnsafeDirectRead(0);
            
            int i = 0;
            foreach (var item in idSet)
            {
                result[i] = item;
                if (++i == countMax)
                    break;
            }
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Create(ref Context context, ref FilterUpdater filterUpdater, Span<ushort> itemIds)
        {
            if (itemIds.Length > 1)
            {
                uint currentId = itemIds[0];
                for (int i = 1; i < itemIds.Length; ++i)
                {
                    currentId = GetChildNode(ref context,
                        ref filterUpdater,
                        ref context.archetypes->GetRef(currentId),
                        itemIds[i],
                        context.isTemporary.Contains(itemIds[i])).archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PatternDownExtend(ref NArray<Node> archetypes, Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            var count = resultCount;
            for (int i = 0; i < count; ++i)
            {
                PatternFindInChild(ref archetypes, ref archetypes.GetRef(result[i]), result, ref resultCount, excludes);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PatternFindInChild(ref NArray<Node> archetypes, ref Node node, Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            for (int i = 0, iMax = node.childenCount; i < iMax; ++i)
            {
                ref var childNode = ref archetypes.GetRef(node.childen[i]);
                if (!excludes.SortContains(childNode.itemId))
                {
                    PatternDownExtend(ref archetypes, ref childNode, result, ref resultCount, excludes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PatternDownExtend(ref NArray<Node> archetypes, ref Node node, Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            if (resultCount == result.Length)
            {
                throw new Exceptions.FilterPatternException(result.Length);
            }
            result[resultCount++] = node.archetypeId;
            PatternFindInChild(ref archetypes, ref node, result, ref resultCount, excludes);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FindPattern(ref NArray<Node> archetypes, ref Node node, int itemIndex, Span<ushort> includes, Span<ushort> excludes, Span<uint> result, ref int resultCount)
        {
            var itemId = includes[itemIndex];
            if (node.itemId <= itemId)
            {
                if (node.itemId == itemId)
                {
                    if (itemIndex == includes.Length - 1)
                    {
                        if (resultCount == result.Length)
                        {
                            throw new Exceptions.FilterPatternException(result.Length);
                        }
                        result[resultCount++] = node.archetypeId;
                    }
                    else
                    {
                        
                        for (int i = 0, iMax = node.childenCount; i < iMax; ++i)
                        {
                            FindPattern(ref archetypes, ref archetypes.GetRef(node.childen[i]), itemIndex + 1, includes, excludes, result, ref resultCount);
                        }
                    }
                }
                else if (!excludes.SortContains(node.itemId))
                {
                    for (int i = 0, iMax = node.childenCount; i < iMax; ++i)
                    {
                        FindPattern(ref archetypes, ref archetypes.GetRef(node.childen[i]), itemIndex, includes, excludes, result, ref resultCount);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RemoveInternal(ref Context context, ref FilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId)
        {
            ref var node = ref context.archetypes->GetRef(archetypeId);
            context.collection.UnsafeDirectRead(node.collectionId).Remove(id, ref context.collection);

            if (node.itemId == itemId)
            {
                ref var parent = ref context.archetypes->GetRef(node.parent);
                context.collection.UnsafeDirectRead(parent.collectionId).Add(id, ref context.collection);
                return parent.archetypeId;
            }
            else
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                var itemNode = MoveUpToItemId(ref *context.archetypes, ref node, itemId, itemDeep, ref deep);
                if (itemNode.parent == 0)
                {
                    ref var rootNode = ref context.archetypes->GetRef(itemDeep[deep - 1]);
                    ref var childNode = ref DeepAttachNewNode(ref context, ref filterUpdater, ref rootNode, itemDeep, deep - 1, false);
                    context.collection.UnsafeDirectRead(childNode.collectionId).Add(id, ref context.collection);
                    return childNode.archetypeId;
                }
                else
                {
                    ref var rootNode = ref context.archetypes->GetRef(itemNode.parent);
                    ref var childNode = ref DeepAttachNewNode(ref context, ref filterUpdater, ref rootNode, itemDeep, deep, false);
                    context.collection.UnsafeDirectRead(childNode.collectionId).Add(id, ref context.collection);
                    return childNode.archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AddInternal(ref Context context, ref FilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId, bool isTemporary)
        {
#if !ANOTHERECS_RELEASE
            if (itemId == context.archetypes->GetRef(archetypeId).itemId)
            {
                throw new ArgumentException($"Item already added to {nameof(ArchetypeCaller)} '{itemId}'.");
            }
#endif
            ref var node = ref context.archetypes->GetRef(archetypeId);
            context.collection.UnsafeDirectRead(node.collectionId).Remove(id, ref context.collection);

            if (itemId > node.itemId)     //Add as node child
            {
                ref var childNode = ref GetChildNode(ref context, ref filterUpdater, ref node, itemId, isTemporary);
                context.collection.UnsafeDirectRead(childNode.collectionId).Add(id, ref context.collection);
                return childNode.archetypeId;
            }
            else     //Finding right node
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                ref var rootNode = ref MoveUpToLocalRoot(ref *context.archetypes, ref node, itemId, itemDeep, ref deep);
                ref var childNode = ref DeepAttachNewNode(ref context, ref filterUpdater, ref rootNode, itemDeep, deep, isTemporary);

                context.collection.UnsafeDirectRead(childNode.collectionId).Add(id, ref context.collection);
                return childNode.archetypeId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node MoveUpToItemId(ref NArray<Node> archetypes, ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
        {
            ref var node = ref startNode;

            do
            {
                if (deep == FIND_DEEP)
                {
                    throw new Exception();
                }

                itemDeep[deep++] = node.itemId;

                node = ref archetypes.GetRef(node.parent);
            }
            while (node.itemId != itemId);

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node MoveUpToLocalRoot(ref NArray<Node> archetypes, ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
        {
            ref var node = ref startNode;

            do
            {
                if (deep == FIND_DEEP)
                {
                    throw new Exception();
                }

                itemDeep[deep++] = node.itemId;

                if (node.parent == 0)
                {
                    return ref archetypes.GetRef(itemId);
                }

                node = ref archetypes.GetRef(node.parent);
            }
            while (node.itemId > itemId);

            itemDeep[deep++] = itemId;

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node DeepAttachNewNode(ref Context context, ref FilterUpdater filterUpdater, ref Node startNode, ushort* itemIds, int itemCount, bool isTemporary)
        {
            ref Node node = ref startNode;
            for (int i = itemCount - 1; i >= 0; --i)
            {
                node = ref GetChildNode(ref context, ref filterUpdater, ref node, itemIds[i], isTemporary);
            }
            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AddInternal(ref Context context, uint id, ushort itemId)
        {
            context.collection.UnsafeDirectRead(0).Remove(id, ref context.collection);
            context.collection.UnsafeDirectRead(context.archetypes->GetRef(itemId).collectionId).Add(id, ref context.collection);
            return itemId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node GetChildNode(ref Context context, ref FilterUpdater filterUpdater, ref Node node, ushort itemId, bool isTemporary)
        {
            ref var childNode = ref FindChildNode(ref *context.archetypes, ref node, itemId);
            if (childNode.archetypeId != 0)
            {
                return ref childNode;
            }

            return ref AttachNewNode(ref context, ref filterUpdater, ref node, itemId, isTemporary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node FindChildNode(ref NArray<Node> archetypes, ref Node node, ushort itemId)
        {
            for (uint i = 0; i < node.childenCount; ++i)
            {
                ref var childNode = ref archetypes.GetRef(node.childen[i]);
                if (childNode.itemId == itemId)
                {
                    return ref childNode;
                }
            }

            return ref archetypes.GetRef(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref Node AttachNewNode(ref Context context, ref FilterUpdater filterUpdater, ref Node parent, ushort itemId, bool isTemporary)
        {
            var archetypeCount = *context.archetypeCount;
            parent.AddChild(archetypeCount);
            var parentArchetypeId = parent.archetypeId;

            if (archetypeCount == context.archetypes->Length)
            {
                context.archetypes->Resize(*context.archetypeCount << 1);
            }

            ref var newNode = ref context.archetypes->GetRef(archetypeCount);
            newNode.archetypeId = archetypeCount;
            newNode.itemId = itemId;
            newNode.parent = parentArchetypeId;
            newNode.collectionId = context.collection.Add();
            newNode.hash = GetHash(ref *context.archetypes, archetypeCount);

            context.collection.UnsafeDirectRead(newNode.collectionId) = new IdCollection(32, newNode.collectionId); //TODO SER

            ref var upNode = ref newNode;
            while (upNode.parent != 0)
            {
                upNode = ref context.archetypes->GetRef(upNode.parent);
                filterUpdater.AddToFilterData(ref *context.archetypes, upNode.archetypeId, archetypeCount, itemId);
            }
            filterUpdater.EndToFilterData();

            ++*context.archetypeCount;

            if (isTemporary)
            {
                var collectionId = newNode.collectionId;
                var toId = FindUpNonTemporary(ref *context.archetypes, ref newNode, ref context.isTemporary);
                context.temporary.Set(context.temporary.UnsafeAllocate(), 
                    new MoveCollection()
                    { 
                        fromCollectionId = collectionId,
                        toCollectionId = toId.collectionId,
                        toArchetypeId = toId.archetypeId,
                    }
                    );

            }

            return ref newNode;
        }

        private static ref Node FindUpNonTemporary(ref NArray<Node> archetypes, ref Node startNode, ref NHashSet<ushort, U2U4HashProvider> isTemporary)
        {
            var currentId = startNode.parent;
            while (currentId != 0)
            {
                ref var node = ref archetypes.GetRef(currentId);
                if (isTemporary.Contains(node.itemId))
                {
                    currentId = archetypes.GetRef(currentId).parent;
                }
                else
                {
                    break;
                }
            }
            return ref archetypes.GetRef(currentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetHash(ref NArray<Node> archetypes, uint archetypeId)
        {
            ref var node = ref archetypes.GetRef(archetypeId);
            uint hash = (node.parent != 0) ? archetypes.GetRef(node.parent).hash : 0;
            return unchecked(hash * 314159 + node.itemId);
        }
    }

    [IgnoreCompile]
    internal struct MoveCollection : IComponent
    {
        public uint fromCollectionId;
        public uint toCollectionId;
        public uint toArchetypeId;
    }

    internal unsafe struct Context : IDisposable
    {
        public CollectionCaller collection;
        public TemporaryCaller temporary;

        public NArray<Node>* archetypes;
        public uint* archetypeCount;
        public NHashSet<ushort, U2U4HashProvider> isTemporary;

        public void Dispose()
        {
            isTemporary.Dispose();
        }
    }

}