using EntityId = System.Int32;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class Filters : ISerialize
    {

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
        }

    }

}