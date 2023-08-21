using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using AnotherECS.Serializer;
using EntityId = System.Int32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public unsafe class Filter : IFilter, ISerialize
    {
        protected State _state;

        private delegate*<State, int, bool> _selector;

        protected ushort[] _includes;
        protected ushort[] _excludes;

        private uint _id;
        private long _hash;
        private bool _isAutoClear;

        private int[] _sparseEntities;
        private EntityId[] _denseEntities;
        private int _entityCount;

        private int _lockForeach;
        private DelayedOp[] _delayedOps;
        private int _delayedOpsCount;


#if ANOTHERECS_HISTORY_DISABLE
        internal void Init(State state, in Mask mask, int denseCapacity, int sparseCapacity)
#else
        private FilterHistory _history;

        internal void Init(State state, in Mask mask, int denseCapacity, int sparseCapacity, FilterHistory history)
#endif
        {
            _sparseEntities = new int[sparseCapacity];
            _denseEntities = new EntityId[denseCapacity];
            _entityCount = 0;

#if ANOTHERECS_HISTORY_DISABLE
            CacheDataInit(state, mask);
#else
            CacheDataInit(state, mask, history);
#endif
        }

#if ANOTHERECS_HISTORY_DISABLE
        internal void CacheDataInit(State state, in Mask mask)
#else
        internal virtual void CacheDataInit(State state, in Mask mask, FilterHistory history)
#endif
        {
            _lockForeach = 0;
            _delayedOps = new DelayedOp[64];
            _delayedOpsCount = 0;

            _state = state;

            _selector = mask.selector;

            _id = mask.id;
            _hash = mask.hash;
            _isAutoClear = mask.isAutoClear;

            _includes = mask.includes;
            _excludes = mask.excludes;
#if !ANOTHERECS_HISTORY_DISABLE
            _history = history;
#endif
        }

        internal uint Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id;
        }

        internal bool IsAutoClear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isAutoClear;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entityCount;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId GetEntity(int index)
        {
#if ANOTHERECS_DEBUG
            if (index < 1 || index >= Count)
            {
                throw new Exceptions.EntityNotFoundByIndexException(index);
            }
#endif
            return _denseEntities[index];
        }

        public IEnumerator<int> GetEnumerator()
            => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_id);
            writer.Pack(_sparseEntities);
            writer.Pack(_denseEntities);
            writer.Write(_entityCount);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _id = reader.ReadUInt32();
            _sparseEntities = reader.Unpack<int[]>();
            _denseEntities = reader.Unpack<EntityId[]>();
            _entityCount = reader.ReadInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ResizeSparseIndex(int capacity)
            => Array.Resize(ref _sparseEntities, capacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(EntityId id)
        {
            if (AddDelayedOp(true, id))
            {
                return;
            }

            if (_entityCount == _denseEntities.Length)
            {
                Array.Resize(ref _denseEntities, _entityCount << 1);
            }
#if !ANOTHERECS_HISTORY_DISABLE
            _history.Push(_denseEntities[_entityCount], _entityCount, _sparseEntities[id], id);
#endif
            _denseEntities[_entityCount++] = id;
            _sparseEntities[id] = _entityCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Remove(EntityId id)
        {
            if (AddDelayedOp(false, id))
            {
                return;
            }
#if !ANOTHERECS_HISTORY_DISABLE
            _history.Push(_denseEntities[_entityCount], _entityCount, _sparseEntities[id], id);
#endif
            var idx = _sparseEntities[id] - 1;
            _sparseEntities[id] = 0;
            _entityCount--;
            if (idx < _entityCount)
            {
                _denseEntities[idx] = _denseEntities[_entityCount];
                _sparseEntities[_denseEntities[idx]] = idx + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort[] GetIncludeTypes()
            => _includes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ushort[] GetExcludeTypes()
            => _excludes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long GetHash()
            => _hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Clear()
        { 
            _entityCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LockForeach()
        {
            ++_lockForeach;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlockForeach()
        {
#if ANOTHERECS_DEBUG
            if (_lockForeach <= 0)
            {
                throw new Exceptions.FilterForeachException();
            }
#endif
            --_lockForeach;
            if (_lockForeach == 0 && _delayedOpsCount > 0)
            {
                for (int i = 0; i < _delayedOpsCount; i++)
                {
                    ref var op = ref _delayedOps[i];
                    if (op.isAddedOrRemoved)
                    {
                        Add(op.id);
                    }
                    else
                    {
                        Remove(op.id);
                    }
                }
                _delayedOpsCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsMaskCompatible(EntityId id)
        {
#if ANOTHERECS_DEBUG
            if (_selector != null)
            {
                return _selector(_state, id);
            }
            throw new Exceptions.FilterNoInitializedException(GetType().Name);
#else
            return _selector(_state, id);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetRaw(int dense, int count, int sparse, int id)
        {
            _denseEntities[count] = dense;
            _entityCount = count;
            _sparseEntities[id] = sparse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntityId[] GetDenseRaw()
            => _denseEntities;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int[] GetSparseRaw()
            => _sparseEntities;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AddDelayedOp(bool isAdded, EntityId id)
        {
            if (_lockForeach > 0)
            {
                if (_delayedOpsCount == _delayedOps.Length)
                {
                    Array.Resize(ref _delayedOps, _delayedOpsCount << 1);
                }

                ref var op = ref _delayedOps[_delayedOpsCount++];
                op.isAddedOrRemoved = isAdded;
                op.id = id;

                return true;
            }

            return false;
        }



        private struct DelayedOp
        {
            public bool isAddedOrRemoved;
            public EntityId id;
        }

        public struct Enumerator : IEnumerator<int>
        {
            private readonly Filter _filter;

            private readonly EntityId[] _entities;
            private readonly int _count;

            private int _current;

            public Enumerator(Filter filter)
            {
                _filter = filter;

                _entities = filter.GetDenseRaw();
                _count = filter.Count;

                _current = -1;

                filter.LockForeach();
            }

            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _entities[_current];
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => ++_current < _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _filter.UnlockForeach();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = -1;
            }
        }
    }
}