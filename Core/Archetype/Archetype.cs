using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct Archetype : IRebindMemoryHandle, ISerialize
    {
        public const int FIND_DEEP = 1024;
        public const int ARCHETYPE_COUNT = 1024;

        private const uint TRANSITION_INIT_CAPACITY = 32;
        private const uint CHANGE_INIT_CAPACITY = 32;

        private GlobalDependencies* _dependencies;

        private NList<BAllocator, Node> _nodes;
        private NContainerList<BAllocator, IdCollection<HAllocator>> _collections;
        private NList<BAllocator, MoveCollection> _temporaries;

        private NDictionary<BAllocator, ulong, uint, U8U4HashProvider> _transitionAddCache;
        private NDictionary<BAllocator, ulong, uint, U8U4HashProvider> _transitionRemoveCache;
        private NBuffer<BAllocator, BufferEntry> _changesBuffer;
        private NHashSet<BAllocator, ushort, U2U4HashProvider> _isTemporaries;

        private int locked;

        public bool IsLocked
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => locked != 0;
        }

        public Archetype(GlobalDependencies* dependencies, INArray<ushort> isTemporaries)
        {
            _dependencies = dependencies;

            _nodes = new NList<BAllocator, Node>(&_dependencies->bAllocator, _dependencies->componentTypesCount + 1);
            _collections = new NContainerList<BAllocator, IdCollection<HAllocator>>(&_dependencies->bAllocator, _nodes.Length);

            _transitionAddCache = new NDictionary<BAllocator, ulong, uint, U8U4HashProvider>(&_dependencies->bAllocator, TRANSITION_INIT_CAPACITY);
            _transitionRemoveCache = new NDictionary<BAllocator, ulong, uint, U8U4HashProvider>(&_dependencies->bAllocator, TRANSITION_INIT_CAPACITY);
            _changesBuffer = new NBuffer<BAllocator, BufferEntry>(&_dependencies->bAllocator, CHANGE_INIT_CAPACITY);

            _isTemporaries = new NHashSet<BAllocator, ushort, U2U4HashProvider>(&_dependencies->bAllocator, isTemporaries);
            _temporaries = new NList<BAllocator, MoveCollection>(&_dependencies->bAllocator, _isTemporaries.Count);

            locked = default;

            Init();
        }

        private void Init()
        {
            _nodes.ExtendToCapacity();

            for (uint i = 0; i < _nodes.Count; ++i)
            {
                ref var archetype = ref _nodes.ReadRef(i);
                archetype.archetypeId = i;
                archetype.itemId = (ushort)i;
                archetype.collectionId = i;
                archetype.hash = GetHash(i);
            }

            _collections.ExtendToCapacity();

            for (uint i = 0; i < _collections.Count; ++i)
            {
                _collections.Set(i, CreateIdCollection());
            }

            _temporaries.ExtendToCapacity();

            foreach (var temporaryId in _isTemporaries)
            {
                _temporaries.Add(
                    new MoveCollection()
                    {
                        fromCollectionId = _nodes.ReadRef(temporaryId).collectionId,
                        toCollectionId = 0
                    }
                    );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id)
        {
            _collections.GetRef(0).Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
            => (archetypeId == 0)
               ? AddInternal(id, itemId)
               : AddInternal(ref filterUpdater, archetypeId, id, itemId, isTemporary);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            if (IsLocked)
            {
                _changesBuffer.Push(        //TODO SER threading?
                    new BufferEntry()
                    {
                        sortKey = Thread.CurrentThread.ManagedThreadId,
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
        public void Remove<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, ushort itemId)
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
        public void Remove(uint archetypeId, uint id)
        {
            _collections.GetRef(_nodes.ReadRef(archetypeId).collectionId).Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create<TFilterUpdater>(ref TFilterUpdater filterUpdater, Span<ushort> itemIds)
            where TFilterUpdater : struct, IFilterUpdater
        {
            if (itemIds.Length > 1)
            {
                uint currentId = itemIds[0];
                for (int i = 1; i < itemIds.Length; ++i)
                {
                    currentId = GetChildNode(
                        ref filterUpdater,
                        ref _nodes.ReadRef(currentId),
                        itemIds[i],
                        _isTemporaries.Contains(itemIds[i])
                        ).archetypeId;
                }
            }
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(uint archetypeId)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ++count;
                archetypeId = _nodes.ReadRef(archetypeId).parent;
            }
            while (archetypeId != 0);

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemIds(uint archetypeId, uint* result, uint resultLength)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ref var node = ref _nodes.ReadRef(archetypeId);
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
        public bool IsHasItem(uint archetypeId, ushort itemId)
        {
            if (archetypeId != 0)
            {
                do
                {
                    ref var node = ref _nodes.ReadRef(archetypeId);

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
        public uint GetItemId(uint archetypeId, uint index)
        {
            if (archetypeId == 0)
            {
                return 0;
            }

            uint count = 0;
            do
            {
                ref var node = ref _nodes.ReadRef(archetypeId);
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
        public ref readonly IdCollection<HAllocator> ReadIdCollection(uint archetypeId)
            => ref _collections.ReadRef(_nodes.ReadRef(archetypeId).collectionId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NArray<BAllocator, uint> Filter(BAllocator* allocator, Span<ushort> includes, Span<ushort> excludes)
        {
            Span<uint> archetypeIds = stackalloc uint[ARCHETYPE_COUNT];
            var count = Filter(includes, excludes, archetypeIds);
            return archetypeIds[..count].ToNArray(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Filter(Span<ushort> includes, Span<ushort> excludes, Span<uint> result)
        {
            int resultCount = 0;
            var items0 = includes[0];
            for (uint i = 1; i <= items0; ++i)
            {
                FindPattern(ref _nodes.ReadRef(i), 0, includes, excludes, result, ref resultCount);
            }

            PatternDownExtend(result, ref resultCount, excludes);

            result.Sort(0, resultCount);
            return resultCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetFilterZeroCount()
            => _collections.ReadRef(0).Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FilterZero(uint* result, int count)
        {
#if !ANOTHERECS_RELEASE
            if (count == 0)
            {
                throw new ArgumentException(nameof(count));
            }
#endif
            ref var idSet = ref _collections.ReadRef(0);

            int i = 0;
            foreach (var item in idSet)
            {
                result[i] = item;
                if (++i == count)
                    break;
            }
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAllTemporary()
        {
            var dense = _temporaries.GetPtr();
            for (uint i = 1, iMax = _temporaries.Count; i < iMax; ++i)
            {
                ref var element = ref _temporaries.GetRef(i);
                ref var from = ref _collections.GetRef(element.fromCollectionId);

                if (from.Count != 0)
                {
                    ref var to = ref _collections.GetRef(element.toCollectionId);
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
        public void Dispose()
        {
            _nodes.Dispose();
            _collections.Dispose();
            _temporaries.Dispose();

            _transitionAddCache.Dispose();
            _transitionRemoveCache.Dispose();
            _changesBuffer.Dispose();
            _isTemporaries.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lock()
        {
            ++locked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Unlock<TFilterUpdater>(ref TFilterUpdater filterUpdater, bool isSorting)
            where TFilterUpdater : struct, IFilterUpdater
        {
            --locked;
            if (locked == 0)
            {
                PushData(ref filterUpdater, isSorting);
            }
        }

        internal void PushData<TFilterUpdater>(ref TFilterUpdater filterUpdater, bool isSorting)
            where TFilterUpdater : struct, IFilterUpdater
        {
            if (isSorting)
            {
                _changesBuffer.Sort();
            }

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
        private IdCollection<HAllocator> CreateIdCollection()
            => new(&_dependencies->hAllocator, _dependencies->config.general.archetypeCapacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint AddInternal(uint id, ushort itemId)
        {
            _collections.GetRef(0).Remove(id);
            _collections.GetRef(_nodes.ReadRef(itemId).collectionId).Add(id);
            return itemId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref uint archetypeId = ref _dependencies->entities.GetRef(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionAddCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                Move(archetypeId, newArchetypeId, id);
            }
            else
            {
                newArchetypeId = Add(ref filterUpdater, archetypeId, id, itemId, isTemporary);
                _transitionAddCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint AddInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
#if !ANOTHERECS_RELEASE
            if (itemId == _nodes.ReadRef(archetypeId).itemId)
            {
                throw new ArgumentException($"Item already added to {nameof(Archetype)} '{itemId}'.");
            }
#endif
            ref var node = ref _nodes.GetRef(archetypeId);
            _collections.GetRef(node.collectionId).Remove(id);

            if (itemId > node.itemId)     //Add as node child
            {
                ref var childNode = ref GetChildNode(ref filterUpdater, ref node, itemId, isTemporary);
                _collections.GetRef(childNode.collectionId).Add(id);
                return childNode.archetypeId;
            }
            else     //Finding right node
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                ref var rootNode = ref MoveUpToLocalRoot(ref node, itemId, itemDeep, ref deep);
                ref var childNode = ref DeepAttachNewNode(ref filterUpdater, ref rootNode, itemDeep, deep, isTemporary);

                _collections.GetRef(childNode.collectionId).Add(id);
                return childNode.archetypeId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint RemoveInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint archetypeId, uint id, ushort itemId)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref var node = ref _nodes.ReadRef(archetypeId);
            _collections.GetRef(node.collectionId).Remove(id);

            if (node.itemId == itemId)
            {
                ref var parent = ref _nodes.ReadRef(node.parent);
                _collections.GetRef(parent.collectionId).Add(id);
                return parent.archetypeId;
            }
            else
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                var itemNode = MoveUpToItemId(ref node, itemId, itemDeep, ref deep);
                if (itemNode.parent == 0)
                {
                    ref var rootNode = ref _nodes.ReadRef(itemDeep[deep - 1]);
                    ref var childNode = ref DeepAttachNewNode(ref filterUpdater, ref rootNode, itemDeep, deep - 1, false);
                    _collections.GetRef(childNode.collectionId).Add(id);
                    return childNode.archetypeId;
                }
                else
                {
                    ref var rootNode = ref _nodes.ReadRef(itemNode.parent);
                    ref var childNode = ref DeepAttachNewNode(ref filterUpdater, ref rootNode, itemDeep, deep, false);
                    _collections.GetRef(childNode.collectionId).Add(id);
                    return childNode.archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveInternal<TFilterUpdater>(ref TFilterUpdater filterUpdater, uint id, ushort itemId)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref uint archetypeId = ref _dependencies->entities.GetRef(id).archetypeId;
            var transitionId = ((ulong)archetypeId) << 32 | itemId;
            if (_transitionRemoveCache.TryGetValue(transitionId, out uint newArchetypeId))
            {
                Move(archetypeId, newArchetypeId, id);
            }
            else
            {
                newArchetypeId = RemoveInternal(ref filterUpdater, archetypeId, id, itemId);
                _transitionRemoveCache.Add(transitionId, newArchetypeId);
            }
            archetypeId = newArchetypeId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node GetChildNode<TFilterUpdater>(ref TFilterUpdater filterUpdater, ref Node node, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref var childNode = ref FindChildNode(ref node, itemId);
            if (childNode.archetypeId != 0)
            {
                return ref childNode;
            }

            return ref AttachNewNode(ref filterUpdater, ref node, itemId, isTemporary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node FindChildNode(ref Node node, ushort itemId)
        {
            for (uint i = 0; i < node.childrenCount; ++i)
            {
                ref var childNode = ref _nodes.ReadRef(node.children[i]);
                if (childNode.itemId == itemId)
                {
                    return ref childNode;
                }
            }

            return ref _nodes.ReadRef(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node AttachNewNode<TFilterUpdater>(ref TFilterUpdater filterUpdater, ref Node parent, ushort itemId, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            _nodes.Dirty();

            var archetypeNewId = _nodes.Count;
            parent.AddChild(archetypeNewId);
            var parentArchetypeId = parent.archetypeId;

            _nodes.Add(default);
            ref var newNode = ref _nodes.GetRef(archetypeNewId);
            newNode.archetypeId = archetypeNewId;
            newNode.itemId = itemId;
            newNode.parent = parentArchetypeId;
            newNode.collectionId = _collections.Count;
            _collections.Add(default);
            newNode.hash = GetHash(archetypeNewId);

            _collections.GetRef(newNode.collectionId) = CreateIdCollection();

            ref var upNode = ref newNode;
            while (upNode.parent != 0)
            {
                upNode = ref _nodes.ReadRef(upNode.parent);
                filterUpdater.AddToFilterData(ref _nodes, upNode.archetypeId, archetypeNewId, itemId);
            }
            filterUpdater.EndToFilterData();

            if (isTemporary)
            {
                var collectionId = newNode.collectionId;
                var toId = FindUpNonTemporary(ref newNode);
                _temporaries.Add(
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node FindUpNonTemporary(ref Node startNode)
        {
            var currentId = startNode.parent;
            while (currentId != 0)
            {
                ref var node = ref _nodes.ReadRef(currentId);
                if (_isTemporaries.Contains(node.itemId))
                {
                    currentId = _nodes.ReadRef(currentId).parent;
                }
                else
                {
                    break;
                }
            }
            return ref _nodes.ReadRef(currentId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node MoveUpToItemId(ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
        {
            ref var node = ref startNode;

            do
            {
                if (deep == FIND_DEEP)
                {
                    throw new Exception();
                }

                itemDeep[deep++] = node.itemId;

                node = ref _nodes.ReadRef(node.parent);
            }
            while (node.itemId != itemId);

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node DeepAttachNewNode<TFilterUpdater>(ref TFilterUpdater filterUpdater, ref Node startNode, ushort* itemIds, int itemCount, bool isTemporary)
            where TFilterUpdater : struct, IFilterUpdater
        {
            ref Node node = ref startNode;
            for (int i = itemCount - 1; i >= 0; --i)
            {
                node = ref GetChildNode(ref filterUpdater, ref node, itemIds[i], isTemporary);
            }
            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node MoveUpToLocalRoot(ref Node startNode, ushort itemId, ushort* itemDeep, ref int deep)
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
                    return ref _nodes.ReadRef(itemId);
                }

                node = ref _nodes.ReadRef(node.parent);
            }
            while (node.itemId > itemId);

            itemDeep[deep++] = itemId;

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternDownExtend(Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            var count = resultCount;
            for (int i = 0; i < count; ++i)
            {
                PatternFindInChild(ref _nodes.ReadRef(result[i]), result, ref resultCount, excludes);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternFindInChild(ref Node node, Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            for (int i = 0, iMax = node.childrenCount; i < iMax; ++i)
            {
                ref var childNode = ref _nodes.ReadRef(node.children[i]);
                if (!excludes.SortContains(childNode.itemId))
                {
                    PatternDownExtend(ref childNode, result, ref resultCount, excludes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternDownExtend(ref Node node, Span<uint> result, ref int resultCount, Span<ushort> excludes)
        {
            if (resultCount == result.Length)
            {
                throw new Exceptions.ArchetypePatternException(result.Length);
            }
            result[resultCount++] = node.archetypeId;
            PatternFindInChild(ref node, result, ref resultCount, excludes);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindPattern(ref Node node, int itemIndex, Span<ushort> includes, Span<ushort> excludes, Span<uint> result, ref int resultCount)
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
                            throw new Exceptions.ArchetypePatternException(result.Length);
                        }
                        result[resultCount++] = node.archetypeId;
                    }
                    else
                    {

                        for (int i = 0, iMax = node.childrenCount; i < iMax; ++i)
                        {
                            FindPattern(ref _nodes.ReadRef(node.children[i]), itemIndex + 1, includes, excludes, result, ref resultCount);
                        }
                    }
                }
                else if (!excludes.SortContains(node.itemId))
                {
                    for (int i = 0, iMax = node.childrenCount; i < iMax; ++i)
                    {
                        FindPattern(ref _nodes.ReadRef(node.children[i]), itemIndex, includes, excludes, result, ref resultCount);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveCollection(uint fromCollectionId, uint toCollectionId, uint id)
        {
            _collections.GetRef(fromCollectionId).Remove(id);
            _collections.GetRef(toCollectionId).Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Move(uint fromArchetypeId, uint toArchetypeId, uint id)
        {
            MoveCollection(_nodes.ReadRef(fromArchetypeId).collectionId, _nodes.ReadRef(toArchetypeId).collectionId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetHash(uint archetypeId)
        {
            ref var node = ref _nodes.ReadRef(archetypeId);
            uint hash = (node.parent != 0) ? _nodes.ReadRef(node.parent).hash : 0;
            return unchecked(hash * 314159 + node.itemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _collections, ref rebinder);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            _nodes.PackBlittable(ref writer);
            _collections.Pack(ref writer);
            _temporaries.PackBlittable(ref writer);

            _transitionAddCache.PackBlittable(ref writer);
            _transitionRemoveCache.PackBlittable(ref writer);
            _changesBuffer.PackBlittable(ref writer);
            _isTemporaries.PackBlittable(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _dependencies = reader.GetDepency<WPtr<GlobalDependencies>>().Value;
            _nodes.UnpackBlittable(ref reader);
            _collections.Unpack(ref reader);
            _temporaries.UnpackBlittable(ref reader);

            _transitionAddCache.UnpackBlittable(ref reader);
            _transitionRemoveCache.UnpackBlittable(ref reader);
            _changesBuffer.UnpackBlittable(ref reader);
            _isTemporaries.UnpackBlittable(ref reader);
        }

        private struct BufferEntry : IComparable<BufferEntry>
        {
            public int sortKey;

            public bool isAdd;
            public bool isTemporary;
            public uint entityId;
            public ushort itemId;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(BufferEntry other)
                => sortKey - other.sortKey;
        }
    }

    internal unsafe struct Node
    {
        public const int ChildrenMax = 16;

        public uint parent;
        public uint archetypeId;
        public ushort itemId;
        public uint collectionId;
        public uint hash;
        public int childrenCount;
        public fixed uint children[Node.ChildrenMax];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(uint nodeId)
        {
#if !ANOTHERECS_RELEASE
            if (childrenCount == ChildrenMax)
            {
                throw new InvalidOperationException();
            }
#endif
            childrenCount = CapacityChildrenAsSpan().TryAddSort(childrenCount, nodeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<uint> CapacityChildrenAsSpan()
            => new(UnsafeUtils.AddressOf(ref children[0]), ChildrenMax);
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct IdCollection<TAllocator> : ISerialize, IDisposable, IEnumerable<uint>
        where TAllocator : unmanaged, IAllocator
    {
        private NHashSet<TAllocator, uint, U4U4HashProvider> _data;

        public uint Count
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IdCollection(TAllocator* allocator, uint capacity)
        {
            _data = new NHashSet<TAllocator, uint, U4U4HashProvider>(allocator, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint id)
        {
            _data.Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint id)
        {
            _data.Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NHashSet<TAllocator, uint, U4U4HashProvider>.Enumerator GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
            => _data.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => _data.GetEnumerator();
    }

    internal struct MoveCollection
    {
        public uint fromCollectionId;
        public uint toCollectionId;
        public uint toArchetypeId;
    }
}