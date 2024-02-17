using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct Set<TCommonAllocator, TCollectionAllocator> : IDisposable, ISerialize, IRepairMemoryHandle
        where TCommonAllocator : unmanaged, IAllocator
        where TCollectionAllocator : unmanaged, IAllocator
    {
        public const int FIND_DEEP = 1024;
        public const int CHILD_PER_NODE_CAPACITY = 32;

        private readonly uint _collectionCapacity;
        private readonly TCollectionAllocator* _collectionAllocator;
        private NList<TCommonAllocator, Node> _nodes;
        private NContainerList<TCommonAllocator, TCollectionAllocator, IdCollection<TCollectionAllocator>> _collections;
        private RangeAllocator<TCommonAllocator> _rangeAllocator;
        private NArray<TCommonAllocator, uint> _nodeChildren;

        public Set(TCommonAllocator* commonAllocator, TCollectionAllocator* collectionAllocator, uint combinationCapacity, uint collectionCapacity)
        {
            _nodes = new NList<TCommonAllocator, Node>(commonAllocator, combinationCapacity);
            _collections = new NContainerList<TCommonAllocator, TCollectionAllocator, IdCollection<TCollectionAllocator>>(commonAllocator, collectionAllocator, _nodes.Length);

            _collectionCapacity = collectionCapacity;
            _collectionAllocator = collectionAllocator;

            _rangeAllocator = new RangeAllocator<TCommonAllocator>(commonAllocator, 2);
            _nodeChildren = new NArray<TCommonAllocator, uint>(commonAllocator, _nodes.Length * CHILD_PER_NODE_CAPACITY);

            Init();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Init()
        {
            const uint zeroArchetypeId = 0;
            var zeroArchetype = new Node()
            {
                archetypeId = zeroArchetypeId,
                itemId = zeroArchetypeId,
                collectionId = zeroArchetypeId,
                hash = 0,
            };
            _nodes.Add(zeroArchetype);

            _collections.Add(CreateIdCollection());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Create<TObserver>(ref TObserver observer, Span<uint> itemIds)
           where TObserver : struct, IObserver
        {
            if (itemIds.Length != 0)
            {
                ref var currentNode = ref GetRoot();

                for (int i = 0; i < itemIds.Length; ++i)
                {
                    var archetypeId = GetChildNode(
                        ref observer,
                        ref currentNode,
                        itemIds[i]
                        ).archetypeId;
                    currentNode = ref _nodes.ReadRef(archetypeId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount(uint archetypeId)
        {
            uint count = 0;
            while (archetypeId != 0)
            {
                ++count;
                archetypeId = _nodes.ReadRef(archetypeId).parent;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint archetypeId, uint id)
        {
            _collections.GetRef(archetypeId).Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add<TObserver>(ref TObserver observer, uint archetypeId, uint id, uint itemId)
            where TObserver : struct, IObserver
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
                ref var childNode = ref GetChildNode(ref observer, ref node, itemId);
                _collections.GetRef(childNode.collectionId).Add(id);
                return childNode.archetypeId;
            }
            else     //Finding right node
            {
                int deep = 0;
                uint* itemDeep = stackalloc uint[FIND_DEEP];

                ref var rootNode = ref MoveUpToLocalRoot(ref node, itemId, itemDeep, ref deep);
                ref var childNode = ref DeepAttachNewNode(ref observer, ref rootNode, itemDeep, deep);

                _collections.GetRef(childNode.collectionId).Add(id);
                return childNode.archetypeId;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(uint archetypeId, uint id)
        {
            _collections.GetRef(_nodes.ReadRef(archetypeId).collectionId).Remove(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Remove<TObserver>(ref TObserver observer, uint archetypeId, uint id, uint itemId)
           where TObserver : struct, IObserver
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
                uint* itemDeep = stackalloc uint[FIND_DEEP];

                var itemNode = MoveUpToItemId(ref node, itemId, itemDeep, ref deep);
                if (itemNode.parent == 0)
                {
                    ref var rootNode = ref _nodes.ReadRef(itemDeep[deep - 1]);
                    ref var childNode = ref DeepAttachNewNode(ref observer, ref rootNode, itemDeep, deep - 1);
                    _collections.GetRef(childNode.collectionId).Add(id);
                    return childNode.archetypeId;
                }
                else
                {
                    ref var rootNode = ref _nodes.ReadRef(itemNode.parent);
                    ref var childNode = ref DeepAttachNewNode(ref observer, ref rootNode, itemDeep, deep);
                    _collections.GetRef(childNode.collectionId).Add(id);
                    return childNode.archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEachItem<TIterator>(uint startArchetypeId, TIterator iterator)
            where TIterator : struct, IIterator<uint>
        {
            while (startArchetypeId != 0)
            {
                ref var node = ref _nodes.ReadRef(startArchetypeId);

                iterator.Each(ref node.itemId);
                startArchetypeId = node.parent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasItem(uint archetypeId, uint itemId)
        {
            while (archetypeId != 0)
            {
                ref var node = ref _nodes.ReadRef(archetypeId);

                if (node.itemId == itemId)
                {
                    return true;
                }
                archetypeId = node.parent;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemIds(uint startArchetypeId, uint* result, uint resultLength)
        {
            uint count = 0;
            while (startArchetypeId != 0)
            {
                ref var node = ref _nodes.ReadRef(startArchetypeId);
#if !ANOTHERECS_RELEASE
                if (count == resultLength)
                {
                    throw new Exceptions.FindIdsException(resultLength);
                }
#endif
                result[count++] = node.itemId;
                startArchetypeId = node.parent;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetItemId(uint archetypeId, uint index)
        {
            uint count = 0;
            while (archetypeId != 0)
            {
                ref var node = ref _nodes.ReadRef(archetypeId);
                if (count == index)
                {
                    return node.itemId;
                }
                ++count;
                archetypeId = node.parent;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly IdCollection<TCollectionAllocator> ReadIdCollection(uint archetypeId)
            => ref _collections.ReadRef(_nodes.ReadRef(archetypeId).collectionId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref IdCollection<TCollectionAllocator> GetIdCollection(uint archetypeId)
            => ref _collections.GetRef(_nodes.ReadRef(archetypeId).collectionId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetFilterZeroCount()
            => ReadIdCollection(0).Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Filter(Span<uint> includes, Span<uint> excludes, Span<uint> result)
        {
            if (includes.Length == 0)
            {
                return Filter(excludes, result);
            }

            int resultCount = 0;

            ref var node = ref GetRoot();
            FindPattern(ref node, 0, includes, excludes, result, ref resultCount);

            PatternDownExtend(result, ref resultCount, excludes);

            result.Sort(0, resultCount);
            return resultCount;
        }

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
                {
                    break;
                }
            }
            return i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveCollection(uint fromCollectionId, uint toCollectionId, uint id)
        {
            _collections.GetRef(fromCollectionId).Remove(id);
            _collections.GetRef(toCollectionId).Add(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(uint fromArchetypeId, uint toArchetypeId, uint id)
        {
            MoveCollection(_nodes.ReadRef(fromArchetypeId).collectionId, _nodes.ReadRef(toArchetypeId).collectionId, id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _nodes.PackBlittable(ref writer);
            _collections.Pack(ref writer);
            _rangeAllocator.Pack(ref writer);
            _nodeChildren.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _nodes.UnpackBlittable(ref reader);
            _collections.Unpack(ref reader);
            _rangeAllocator.Unpack(ref reader);
            _nodeChildren.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _nodes.Dispose();
            _collections.Dispose();
            _rangeAllocator.Dispose();
            _nodeChildren.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _collections, ref repairMemoryContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Filter(Span<uint> excludes, Span<uint> result)
        {
            int resultCount = 0;

            ref var node = ref GetRoot();
            result[resultCount++] = node.archetypeId;
            PatternDownExtend(result, ref resultCount, excludes);
            return resultCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node GetChildNode<TObserver>(ref TObserver observer, ref Node node, uint itemId)
            where TObserver : struct, IObserver
        {
            ref var childNode = ref FindChildNode(ref node, itemId);
            if (childNode.archetypeId != 0)
            {
                return ref childNode;
            }

            return ref AttachNewNode(ref observer, ref node, itemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node FindChildNode(ref Node node, uint itemId)
        {
            var index = BinarySearch(ref _nodes, ref _nodeChildren, ref node, node.childrenCount, itemId);

            return ref (index >= 0)
                ? ref _nodes.ReadRef(GetChild(ref node, (uint)index))
                : ref _nodes.ReadRef(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetChild(ref Node node, uint index)
            => GetChild(ref _nodeChildren, ref node, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetChild(ref NArray<TCommonAllocator, uint> nodeChildren, ref Node node, uint index)
            => nodeChildren.ReadRef(node.childrenId + index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node AttachNewNode<TObserver>(ref TObserver observer, ref Node parent, uint itemId)
            where TObserver : struct, IObserver
        {
            var archetypeNewId = _nodes.Count;
            AddChild(ref parent, archetypeNewId);
            var parentArchetypeId = parent.archetypeId;

            _nodes.Add(default);
            ref var newNode = ref _nodes.GetRef(archetypeNewId);
            newNode.itemId = itemId;
            newNode.archetypeId = archetypeNewId;

            newNode.parent = parentArchetypeId;
            newNode.collectionId = _collections.Count;
            _collections.Add(default);

            newNode.hash = GetHash(archetypeNewId);

            newNode.childrenId = AllocateChildNodeCollection(CHILD_PER_NODE_CAPACITY);
            newNode.childrenCapacity = CHILD_PER_NODE_CAPACITY;

            _collections.GetRef(newNode.collectionId) = CreateIdCollection();

            ref var upNode = ref newNode;
            do
            {
                upNode = ref _nodes.ReadRef(upNode.parent);
                observer.Add(ref _nodes, upNode.archetypeId, archetypeNewId, itemId);
            }
            while (upNode.archetypeId != 0);

            observer.AddFinished(new WArray<Node>(_nodes.ReadPtr(), _nodes.Length), ref newNode, itemId);

            return ref newNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint AllocateChildNodeCollection(uint capacity)
        {
            var index = _rangeAllocator.Allocate(capacity);
            if (index + capacity >= _nodeChildren.Length)
            {
                _nodeChildren.Resize(index + capacity);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeallocateChildNodeCollection(uint id)
        {
            _rangeAllocator.Deallocate(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChild(ref Node node, uint nodeId)
        {
            if (node.childrenCount == node.childrenCapacity)
            {
                var oldChildrenId = node.childrenId;
                var oldChildrenCapacity = node.childrenCapacity;
                if (node.childrenCapacity == 0)
                {
                    node.childrenCapacity = 1;
                }
                node.childrenCapacity <<= 1;
                var newChildrenId = AllocateChildNodeCollection(node.childrenCapacity);

                if (oldChildrenCapacity != 0)
                {
                    var ptr = _nodeChildren.GetPtr();
                    UnsafeMemory.MemCopy(ptr + newChildrenId, ptr + oldChildrenId, oldChildrenCapacity);
                }
                DeallocateChildNodeCollection(oldChildrenId);
            }

            var count = (int)node.childrenCount;
            NArrayExtensions.AsSpan<NArray<TCommonAllocator, uint>, uint>(ref _nodeChildren, (int)node.childrenCapacity)
                .TryAddSort(count, nodeId);

            ++node.childrenCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node MoveUpToItemId(ref Node startNode, uint itemId, uint* itemDeep, ref int deep)
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
        private ref Node DeepAttachNewNode<TObserver>(ref TObserver observer, ref Node startNode, uint* itemIds, int itemCount)
            where TObserver : struct, IObserver
        {
            ref Node node = ref startNode;
            for (int i = itemCount - 1; i >= 0; --i)
            {
                node = ref GetChildNode(ref observer, ref node, itemIds[i]);
            }
            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node MoveUpToLocalRoot(ref Node startNode, uint itemId, uint* itemDeep, ref int deep)
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
            while (node.itemId > itemId);

            itemDeep[deep++] = itemId;

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternDownExtend(Span<uint> result, ref int resultCount, Span<uint> excludes)
        {
            var count = resultCount;
            for (int i = 0; i < count; ++i)
            {
                PatternFindInChild(ref _nodes.ReadRef(result[i]), result, ref resultCount, excludes);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternFindInChild(ref Node node, Span<uint> result, ref int resultCount, Span<uint> excludes)
        {
            for (uint i = 0, iMax = node.childrenCount; i < iMax; ++i)
            {
                ref var childNode = ref _nodes.ReadRef(GetChild(ref node, i));
                if (!excludes.SortContains(childNode.itemId))
                {
                    PatternDownExtend(ref childNode, result, ref resultCount, excludes);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PatternDownExtend(ref Node node, Span<uint> result, ref int resultCount, Span<uint> excludes)
        {
            if (resultCount == result.Length)
            {
                throw new Exceptions.ArchetypePatternException(result.Length);
            }
            result[resultCount++] = node.archetypeId;
            PatternFindInChild(ref node, result, ref resultCount, excludes);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FindPattern(ref Node node, int itemIndex, Span<uint> includes, Span<uint> excludes, Span<uint> result, ref int resultCount)
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
                        for (uint i = 0, iMax = node.childrenCount; i < iMax; ++i)
                        {
                            FindPattern(ref _nodes.ReadRef(GetChild(ref node, i)), itemIndex + 1, includes, excludes, result, ref resultCount);
                        }
                    }
                }
                else if (!excludes.SortContains(node.itemId))
                {
                    for (uint i = 0, iMax = node.childrenCount; i < iMax; ++i)       //TODO SER OPT
                    {
                        FindPattern(ref _nodes.ReadRef(GetChild(ref node, i)), itemIndex, includes, excludes, result, ref resultCount);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IdCollection<TCollectionAllocator> CreateIdCollection()
            => new(_collectionAllocator, _collectionCapacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node GetRoot()
            => ref _nodes.ReadRef(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetHash(uint archetypeId)
        {
            ref var node = ref _nodes.ReadRef(archetypeId);
            uint hash = (node.parent != 0) ? _nodes.ReadRef(node.parent).hash : 0;
            return unchecked(hash * 314159 + node.itemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearch(
            ref NList<TCommonAllocator, Node> nodes,
            ref NArray<TCommonAllocator, uint> nodeChildren,
            ref Node node,
            uint count,
            uint item)
        {
            if (count <= 4)
            {
                for (uint i = 0; i < count; ++i)
                {
                    if (nodes.ReadRef(GetChild(ref nodeChildren, ref node, i)).itemId == item)
                    {
                        return (int)i;
                    }
                }
                return -1;
            }
            else
            {
                int lo = 0;
                int hi = lo + (int)count - 1;

                while (lo <= hi)
                {
                    int i = GetMedian(lo, hi);

                    uint c = nodes.ReadRef(GetChild(ref nodeChildren, ref node, (uint)i)).itemId;
                    if (c == item)
                    {
                        return i;
                    }
                    else if (c < item)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
                return ~lo;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static int GetMedian(int low, int hi)
                    => low + ((hi - low) >> 1);
            }
        }

        public interface IObserver
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Add<TNArray>(ref TNArray archetypes, uint archetypeId, uint toAddArchetypeId, uint itemId)
               where TNArray : struct, INArray<Node>;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AddFinished(WArray<Node> nodes, ref Node collection, uint itemId);
        }
    }
}