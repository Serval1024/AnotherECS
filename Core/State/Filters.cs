using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public unsafe sealed class Archetypes : ISerialize //TODO internal
    {
        private const int FIND_DEEP = 1024;
        private const int ARCHETYPE_COUNT = 1024;

        private int _archetypeCount;
        private Node[] _archetypes;

        private BacketCollection _items;

        public Archetypes(uint rootItemCount, uint totalItemCapacity)
        {
            _archetypeCount = (int)rootItemCount + 1;
            _archetypes = new Node[_archetypeCount << 1];
            for (uint i = 0, iMax = (uint)_archetypeCount; i < iMax; i++)
            {
                ref var archetype = ref _archetypes[i];
                archetype.archetypeId = i;
                archetype.itemId = (ushort)i;
            }

            _items = new BacketCollection((uint)_archetypeCount, totalItemCapacity);
        }

        public uint Add(uint archetypeId, uint id, ushort itemId)
            => (archetypeId == 0)
                ? AddInternal(id, itemId)
                : AddInternal(archetypeId, id, itemId);
        
        public uint Remove(uint archetypeId, uint id, ushort itemId)
            => (archetypeId == 0)
                ? RemoveInternal(id, itemId)
                : RemoveInternal(archetypeId, id, itemId);

        public void Remove(uint archetypeId, uint id)
        {
            _items.Remove(_archetypes[archetypeId].itemsCollectionId, id);
        }

        public uint[] Filter(ushort[] items)
        {
            var archetypeIds = stackalloc uint[ARCHETYPE_COUNT];
            var count = Filter(items, items.Length, archetypeIds);
            var result = new uint[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = archetypeIds[i];
            }
            return result;
        }

        public int Filter(ushort[] items, int itemCount, uint* result)
        {
            int resultCount = 0;
            var items0 = (int)items[0];
            for(int i = 1; i <= items0; ++i)
            {
                FindPattern(ref _archetypes[i], 0, items, itemCount, result, ref resultCount);
            }

            PatternExtend(result, ref resultCount);

            return resultCount;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            throw new NotImplementedException();
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            throw new NotImplementedException();
        }


        private void PatternExtend(uint* result, ref int resultCount)
        {
            var count = resultCount;
            for (int i = 0; i < count; ++i)
            {
                ref var node = ref _archetypes[result[i]];
                int jMax = node.childenCount;
                for (int j = 0; j < jMax; ++j)
                {
                    PatternExtend(ref _archetypes[node.childen[j]], result, ref resultCount);
                }
            }
        }

        private void PatternExtend(ref Node node, uint* result, ref int resultCount)
        {
            result[resultCount++] = node.archetypeId;
            int iMax = node.childenCount;
            for (int i = 0; i < iMax; ++i)
            {
                PatternExtend(ref _archetypes[node.childen[i]], result, ref resultCount);
            }
        }


        private void FindPattern(ref Node node, int itemIndex, ushort[] items, int itemCount, uint* result, ref int resultCount)
        {            
            var itemId = items[itemIndex];
            if (node.itemId <= itemId)
            {
                if (node.itemId == itemId)
                {
                    if (itemIndex == itemCount - 1)
                    {
                        result[resultCount++] = node.archetypeId;
                        return;
                    }

                    int iMax = node.childenCount;
                    for (int i = 0; i < iMax; ++i)
                    {
                        FindPattern(ref _archetypes[node.childen[i]], itemIndex + 1, items, itemCount, result, ref resultCount);
                    }
                }
                else
                {
                    int iMax = node.childenCount;
                    for (int i = 0; i < iMax; ++i)
                    {
                        FindPattern(ref _archetypes[node.childen[i]], itemIndex, items, itemCount, result, ref resultCount);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint RemoveInternal(uint archetypeId, uint id, ushort itemId)
        {
            ref var node = ref _archetypes[archetypeId];
            _items.Remove(node.itemsCollectionId, id);

            ref var parent = ref _archetypes[node.parent];
            if (node.itemId == itemId)
            {
                _items.Add(parent.itemsCollectionId, id);
                return parent.archetypeId;
            }
            else
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                var itemNode = MoveUpToItemId(ref node, itemId, itemDeep, ref deep);
                if (itemNode.parent == 0)
                {
                    ref var rootNode = ref _archetypes[itemDeep[deep - 1]];
                    ref var childNode = ref DeepAttachNewNode(ref rootNode, itemDeep, deep - 1);
                    _items.Add(childNode.itemsCollectionId, id);
                    return childNode.archetypeId;
                }
                else
                {
                    ref var rootNode = ref _archetypes[itemNode.parent];
                    ref var childNode = ref DeepAttachNewNode(ref rootNode, itemDeep, deep);
                    _items.Add(childNode.itemsCollectionId, id);
                    return childNode.archetypeId;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint RemoveInternal(uint id, ushort itemId)
        {
            _items.Remove(itemId, id);
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint AddInternal(uint archetypeId, uint id, ushort itemId)
        {
#if ANOTHERECS_DEBUG
            if (itemId == _archetypes[archetypeId].itemId)
            {
                throw new ArgumentException($"Item already added to {nameof(Archetypes)} '{itemId}'.");
            }
#endif
            ref var node = ref _archetypes[archetypeId];
            _items.Remove(node.itemsCollectionId, id);

            if (itemId > node.itemId)     //Add as node child
            {
                ref var childNode = ref GetChildNode(ref node, itemId);
                _items.Add(childNode.itemsCollectionId, id);
                return childNode.archetypeId;
            }
            else     //Finding right node
            {
                int deep = 0;
                ushort* itemDeep = stackalloc ushort[FIND_DEEP];

                ref var rootNode = ref MoveUpToLocalRoot(ref node, itemId, itemDeep, ref deep);
                ref var childNode = ref DeepAttachNewNode(ref rootNode, itemDeep, deep);

                _items.Add(childNode.itemsCollectionId, id);
                return childNode.archetypeId;
            }
        }

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

                node = ref _archetypes[node.parent];
            }
            while (node.itemId != itemId);

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
                    return ref _archetypes[itemId];
                }

                node = ref _archetypes[node.parent];
            }
            while (node.itemId > itemId);

            itemDeep[deep++] = itemId;

            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node DeepAttachNewNode(ref Node startNode, ushort* itemIds, int itemCount)
        {
            ref Node node = ref startNode;
            for (int i = itemCount - 1; i >= 0; --i)
            {
                node = ref GetChildNode(ref node, itemIds[i]);
            }
            return ref node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint AddInternal(uint id, ushort itemId)
        {
            _items.Add(_archetypes[itemId].itemsCollectionId, id);
            
            return itemId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node GetChildNode(ref Node node, ushort itemId)
        {
            ref var childNode = ref FindChildNode(ref node, itemId);
            if (childNode.archetypeId != 0)
            {
                return ref childNode;
            }

            return ref AttachNewNode(ref node, itemId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node FindChildNode(ref Node node, ushort itemId)
        {
            for (int i = 0; i < node.childenCount; ++i)       //TODO SER OPTIMIZATE
            {
                ref var childNode = ref _archetypes[node.childen[i]];
                if (childNode.itemId == itemId)
                {
                    return ref childNode;
                }
            }
            
            return ref _archetypes[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Node AttachNewNode(ref Node parent, ushort itemId)
        {
#if ANOTHERECS_DEBUG
            if (parent.childenCount == Node.ChildenMax)
            {
                throw new InvalidOperationException();       //TODO SER
            }
#endif
            if (_archetypeCount == _archetypes.Length)
            {
                Array.Resize(ref _archetypes, _archetypeCount << 1);
            }

            var id = (uint)_archetypeCount;
            ref var newNode = ref _archetypes[id];
            newNode.archetypeId = id;
            newNode.itemId = itemId;
            newNode.parent = parent.archetypeId;
            newNode.itemsCollectionId = _items.Allocate();

            parent.childen[parent.childenCount++] = id;
            ++_archetypeCount;

            return ref newNode;
        }


        private unsafe struct Node
        {
            public const int ChildenMax = 16;

            public uint parent;
            public uint archetypeId;
            public ushort itemId;
            public byte childenCount;
            public fixed uint childen[Node.ChildenMax];
            public uint itemsCollectionId;
        }

        

        private struct BacketCollection
        {
            private int _backetCount;
            private Backet[] _backets;
            private uint[] _items;

            public BacketCollection(uint backetCapacity, uint itemCapacity)
            {
                if (itemCapacity < backetCapacity)
                {
                    itemCapacity = backetCapacity;
                }

                _backetCount = 1;
                _backets = new Backet[backetCapacity];
                _items = new uint[itemCapacity];


            }

            public void Add(uint backetId, uint item)
            {

            }

            public void Remove(uint backetId, uint item)
            {

            }

            public uint Allocate()
            {
                if (_backets.Length == _backetCount)
                {
                    Array.Resize(ref _backets, _backetCount << 1);
                }

                return (uint)_backetCount++;
            }


            private struct Backet
            {
                public uint id;
                public uint count;
            }
        }
    }


#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class Filters //: ISerialize
    {

    }



    /*
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
        internal sealed class Filters //: ISerialize
    {*/
        /*

        private readonly State _state;
        private readonly Entities _entities;
#if !ANOTHERECS_HISTORY_DISABLE
        private readonly FilterHistoryFactory _historyFactory;
#endif
        private int filterCapacity;

        private Dictionary<long, Filter> _cached;
        private List<Filter> _filters;
        private List<Filter> _autoClearFilters;
        private List<Filter>[] _includes;
        private List<Filter>[] _excludes;

#if ANOTHERECS_HISTORY_DISABLE
        internal Filters(State state, Entities entities)
#else
        internal Filters(State state, Entities entities, in FilterHistoryFactory historyFactory)
#endif
        {
            _entities = entities;
            _state = state;
#if !ANOTHERECS_HISTORY_DISABLE
            _historyFactory = historyFactory;
#endif
        }

#if ANOTHERECS_HISTORY_DISABLE
        public Filters(in GeneralConfig config, State state, Entities entities)
        : this(state, entities)
#else
        public Filters(in GeneralConfig config, State state, Entities entities, in FilterHistoryFactory historyFactory)
            : this(state, entities, historyFactory)
#endif
        {
            _filters = new();
            filterCapacity = (int)config.filterCapacity;
        }

        internal unsafe void Init(int componentCount)
        {
            _includes = new List<Filter>[componentCount];
            _excludes = new List<Filter>[componentCount];

            for (int i = 0; i < _includes.Length; ++i)
            {
                _includes[i] = new List<Filter>();
            }

            for (int i = 0; i < _excludes.Length; ++i)
            {
                _excludes[i] = new List<Filter>();
            }

            _cached ??= new();
            _autoClearFilters ??= new();

            foreach (Filter filter in _filters)
            {
                AddFilter(filter);
            }
        }

#if ANOTHERECS_HISTORY_DISABLE
        internal void AllFilterRebind(State state, Func<Type, Mask> maskProvider)
#else
        internal void AllFilterRebind(State state, Func<Type, Mask> maskProvider, Func<Filter, FilterHistory> historyProvider)
#endif
        {
            foreach (Filter filter in _filters)
            {
#if ANOTHERECS_HISTORY_DISABLE
                filter.CacheDataInit(state, maskProvider(filter.GetType()));
#else
                filter.CacheDataInit(state, maskProvider(filter.GetType()), historyProvider(filter));
#endif
            }
        }

        public T Create<T>(in Mask mask)
            where T : Filter, new()
        {
            var hash = mask.hash;
            var exists = _cached.TryGetValue(hash, out var filter);
            if (exists)
            {
                return (T)filter;
            }

            return CreateFilter<T>(mask);
        }

        private unsafe T CreateFilter<T>(in Mask mask)
            where T : Filter, new()
        {
            var filter = new T();
#if ANOTHERECS_HISTORY_DISABLE
            filter.Init(_state, mask, filterCapacity, _entities.GetEntityCapacity());
#else
            filter.Init(_state, mask, filterCapacity, _entities.GetEntityCapacity(), _historyFactory.Create(filter));
#endif
            AddFilter(filter);

            return filter;
        }

        public void Destroy(Filter filter)
        {
            var includes = filter.GetIncludeTypes();
            var excludes = filter.GetExcludeTypes();

            for (int i = 0, iMax = includes.Length; i < iMax; i++)
            {
                _includes[includes[i]].Remove(filter);
            }

            for (int i = 0, iMax = excludes.Length; i < iMax; i++)
            {
                _excludes[excludes[i]].Remove(filter);
            }

            _cached.Remove(filter.GetHash());
            _filters.Remove(filter);
        }

        public void ResizeSparseIndex(int capacity)
        {
            for (int i = 0, iMax = _filters.Count; i < iMax; i++)
            {
                _filters[i].ResizeSparseIndex(capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentAdd(EntityId id, int componentType)
        {
            ComponentAddInclude(id, componentType);
            ComponentAddExclude(id, componentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentRemove(EntityId id, int componentType)
        {
            ComponentRemoveInclude(id, componentType);
            ComponentRemoveExclude(id, componentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentAddInclude(EntityId id, int componentType)
        {
            var includeList = _includes[componentType];

            for (int i = 0; i < includeList.Count; ++i)
            {
                var filter = includeList[i];
                if (filter.IsMaskCompatible(id))
                {
                    filter.Add(id);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentRemoveInclude(EntityId id, int componentType)
        {
            var includeList = _includes[componentType];

            for (int i = 0; i < includeList.Count; ++i)
            {
                includeList[i].Remove(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentAddExclude(EntityId id, int componentType)
        {
            var excludeList = _excludes[componentType];

            for (int i = 0; i < excludeList.Count; ++i)
            {
                excludeList[i].Remove(id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ComponentRemoveExclude(EntityId id, int componentType)
        {
            var excludeList = _excludes[componentType];

            for (int i = 0; i < excludeList.Count; ++i)
            {
                var filter = excludeList[i];
                if (filter.IsMaskCompatible(id))
                {
                    filter.Add(id);
                }
            }
        }

        public void TickFinished()
        {
            for(int i = 0; i < _autoClearFilters.Count; ++i)
            {
                _autoClearFilters[i].Clear();
            }
        }

        public void Clear()
        {
            _cached.Clear();
            _filters.Clear();
            _includes = Array.Empty<List<Filter>>();
            _excludes = Array.Empty<List<Filter>>();
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(filterCapacity);
            writer.Pack(_filters);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            filterCapacity = reader.ReadInt32();
            _filters = reader.Unpack<List<Filter>>();
        }

        private void AddFilter(Filter filter)
        {
            _filters.Add(filter);
            _cached.Add(filter.GetHash(), filter);

            var includes = filter.GetIncludeTypes();
            var excludes = filter.GetExcludeTypes();

            if (filter.IsAutoClear)
            {
                _autoClearFilters.Add(filter);
            }
            
            for (int i = 0, iMax = includes.Length; i < iMax; i++)
            {
                _includes[includes[i]].Add(filter);
            }

            for (int i = 0, iMax = excludes.Length; i < iMax; i++)
            {
                _excludes[excludes[i]].Add(filter);
            }

            for (int i = 0, count = _entities.GetRawCount(); i < count; i++)
            {
                if (_entities.IsHasRaw(i) && filter.IsMaskCompatible(i))
                {
                    filter.Add(i);
                }
            }
        }*/

    //}

}