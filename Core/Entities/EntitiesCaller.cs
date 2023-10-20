using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    using ImplCaller = Caller<
                uint, EntityHead, uint, TickOffsetData<ushort>, ushort,
                UintNumber,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                RecycleStorageFeature<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                NonSparseFeature<EntityHead, TickOffsetData<ushort>, ushort>,
                UintDenseFeature<uint, EntityHead, TickOffsetData<ushort>>,
                Nothing,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                Nothing<uint, EntityHead, uint, TickOffsetData<ushort>, ushort>,
                BySegmentHistoryFeature<uint, EntityHead, uint, ushort>,
                BBSerialize<uint, EntityHead, uint, TickOffsetData<ushort>>,
                BySegmentHistoryFeature<uint, EntityHead, uint, ushort>
                >;

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct EntitiesCaller : ICaller<EntityHead>, ITickFinishedCaller, IRevertCaller
    {
        private ImplCaller _impl;
        private int _gcEntityCheckPerTick;

        public bool IsValide => _impl.IsValide;
        public bool IRevert => true;
        public bool IsTickFinished => true;
        public bool IsSerialize => false;
        public bool IsResizable => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            CallerWrapper.Config<ImplCaller, EntityHead>(ref _impl, layout, depencies, 0, null);
            _gcEntityCheckPerTick = (int)depencies->config.general.gcEntityCheckPerTick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            CallerWrapper.AllocateLayout<ImplCaller, EntityHead>(ref _impl);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityHead ICaller<EntityHead>.Create()
           => _impl.Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => _impl.GetElementType();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id < _impl.GetAllocated() && IsHasRaw(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => GetGeneration(id) >= EntitiesActions.AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponentCount(EntityId id)
            => EntitiesActions.GetComponentCount(ref _impl, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponents(EntityId id, ushort[] buffer)
            => EntitiesActions.GetComponents(ref _impl, id, buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponent(EntityId id, int index)
            => EntitiesActions.GetComponent(ref _impl, id, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetGeneration(EntityId id)
            => EntitiesActions.GetGeneration(ref _impl, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => EntitiesActions.GetCount(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => EntitiesActions.GetCapacity(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
            => EntitiesActions.TryResizeDense(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ushort componentType)
            => EntitiesActions.Add(ref _impl, id, componentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id, ushort componentType)
            => EntitiesActions.Remove(ref _impl, id, componentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate()
            => EntitiesActions.Allocate(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
            => EntitiesActions.Deallocate(ref _impl, id);

        public void TickFinished()
        {
            _impl.TickFinished();

            if (_gcEntityCheckPerTick != 0)
            {
                InterationGarbageCollect(_impl.GetDepencies()->tickProvider.tick, _gcEntityCheckPerTick);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            _impl.RevertTo(tick, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InterationGarbageCollect(uint tick, int checkCount)
        {
            EntitiesActions.InterationGarbageCollect(ref _impl, tick, checkCount);
        }

        public static class LayoutInstaller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EntitiesCaller Install(State state)
                => state.AddLayout<EntitiesCaller, int, EntityHead, uint, TickOffsetData<EntityHead>>();
        }
    }


    internal static unsafe class EntitiesActions
    {
        internal const ushort AllocateGeneration = 32768;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static EntityId Allocate(ref ImplCaller caller)
        {
            var id = caller.UnsafeAllocateForId();
            var head = caller.UnsafeDirectReadPtr(id);

            caller.DirectPush(&head->generation);
            
            head->generation += AllocateGeneration + 1;
            if (head->generation == ushort.MaxValue)
            {
                head->generation = AllocateGeneration;
            }

            head->count = 0;

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deallocate(ref ImplCaller caller, EntityId id)
        {
            var head = caller.UnsafeDirectReadPtr(id);

            caller.DirectPush(&head->generation);
            head->generation -= AllocateGeneration;

            var next = head->next;
            var componentsPtr = head->components;
            var count = head->count;
            var iMax = Math.Min(count, EntityHead.ComponentMax);
            for (int i = 0; i < iMax; ++i)
            {
                caller.DirectPush(componentsPtr + i);
            }

            count -= iMax;

            Remove(ref caller, id);

            if (next != 0)
            {
                var idTail = next;
                var tail = ReadAs<EntityTail>(ref caller, idTail);
                next = tail->next;

                iMax = Math.Min(count, EntityTail.ComponentMax);
                for (int i = 0; i < iMax; ++i)
                {
                    caller.DirectPush(componentsPtr + i);
                }
                count -= EntityTail.ComponentMax;   //TODO SER CHECK ushort < 0

                Remove(ref caller, idTail);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref ImplCaller caller)
            => caller.TryResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryIncResizeDense(ref ImplCaller caller)
            => caller.TryIncResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetComponentCount(ref ImplCaller caller, EntityId id)
            => caller.UnsafeDirectReadPtr(id)->count;

        public static ushort GetComponents(ref ImplCaller caller, EntityId id, ushort[] buffer)
        {
            var head = caller.UnsafeDirectReadPtr(id);
            var count = head->count;
#if ANOTHERECS_DEBUG
            if (buffer.Length < count)
            {
                throw new ArgumentException($"There is not enough space in {nameof(buffer)} to copy.");
            }
#endif
            var components = head->components;

            var index = -1;
            for (int i = 0, iMax = Math.Min(count, EntityHead.ComponentMax); i < iMax; ++i)
            {
                buffer[++index] = components[i];
            }

            if (count > EntityHead.ComponentMax)
            {
                var next = head->next;
                EntityTail* tail = null;
                while (next != 0)
                {
                    tail = ReadAs<EntityTail>(ref caller, next);
                    components = tail->components;

                    for (int i = 0; i < EntityTail.ComponentMax; ++i)
                    {
                        buffer[++index] = components[i];
                    }

                    next = tail->next;
                }
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetComponent(ref ImplCaller caller, EntityId id, int index)
        {
            if (index < EntityHead.ComponentMax)
            {
                return caller.UnsafeDirectReadPtr(id)->components[index];
            }
            else
            {
                var next = caller.UnsafeDirectReadPtr(id)->next;
                index -= EntityHead.ComponentMax;
                while (next != 0)
                {
                    if (index < EntityTail.ComponentMax)
                    {
                        return ReadAs<EntityTail>(ref caller, next)->components[index];
                    }

                    next = ReadAs<EntityTail>(ref caller, next)->next;
                    index -= EntityTail.ComponentMax;
                }
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort GetGeneration(ref ImplCaller caller, EntityId id)
            => caller.UnsafeDirectReadPtr(id)->generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount(ref ImplCaller caller)
           => caller.GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetUpperBoundId(ref ImplCaller caller)
           => caller.GetAllocated();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCapacity(ref ImplCaller caller)
           => caller.GetCount();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref ImplCaller caller, EntityId id, ushort componentType)
        {
            var head = caller.UnsafeDirectReadPtr(id);
            if (head->count < EntityHead.ComponentMax)
            {
                ref var component = ref head->components[head->count];
                component = componentType;
            }
            else
            {
                var next = head->next;
                EntityTail* tail = null;

                if (next == 0)
                {
                    TryIncResizeDense(ref caller);
                    head->next = caller.UnsafeAllocateForId();
                    tail = ReadAs<EntityTail>(ref caller, next);
                    tail->components[0] = componentType;
                }
                else
                {
                    var index = head->count - EntityHead.ComponentMax + EntityTail.ComponentMax;
                    while (next != 0)
                    {
                        tail = ReadAs<EntityTail>(ref caller, next);
                        next = tail->next;
                        index -= EntityTail.ComponentMax;
                    }

                    if (index < EntityTail.ComponentMax)
                    {
                        tail->components[index] = componentType;
                    }
                    else
                    {
                        TryIncResizeDense(ref caller);
                        tail->next = caller.UnsafeAllocateForId();
                        tail = ReadAs<EntityTail>(ref caller, next);
                        tail->components[0] = componentType;
                    }
                }
            }

            caller.DirectPush(&head->count);
            ++head->count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref ImplCaller caller, EntityId id, ushort componentType)
        {
            var head = caller.UnsafeDirectReadPtr(id);

            var componentPtr = FindComponentPtr(ref caller, id, componentType);
            if (componentPtr != null)
            {
                var lastPtr = FindComponentLastPtr(ref caller, id);

                if (componentPtr != lastPtr)
                {
                    caller.DirectPush(componentPtr);
                    *componentPtr = *lastPtr;
                }

                caller.DirectPush(&head->count);
                --head->count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InterationGarbageCollect
            (ref ImplCaller caller, uint tick, int checkCount)
        {
            var count = GetUpperBoundId(ref caller);
            var currentIndexGC = tick % count;

            while (--checkCount > 0 && currentIndexGC < count)
            {
                var entity = caller.UnsafeDirectReadPtr(currentIndexGC);
                if (entity->generation >= AllocateGeneration && entity->count == 0)
                {
                    DeallocateZero(ref caller, currentIndexGC);
                }

                ++currentIndexGC;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DeallocateZero(ref ImplCaller caller, EntityId id)
        {
            var head = caller.UnsafeDirectReadPtr(id);

            caller.DirectPush(&head->generation);

            head->generation -= AllocateGeneration;
            Remove(ref caller, id);
        }

        private static ushort* FindComponentPtr(ref ImplCaller caller, EntityId id, ushort componentType)
        {
            var head = caller.UnsafeDirectReadPtr(id);
            var components = head->components;

            for (int i = 0, iMax = Math.Min(head->count, EntityHead.ComponentMax); i < iMax; ++i)
            {
                if (components[i] == componentType)
                {
                    return components + i;
                }
            }

            var next = head->next;
            EntityTail* tail = null;
            while (next != 0)
            {
                tail = ReadAs<EntityTail>(ref caller, next);
                components = tail->components;

                for (int i = 0; i < EntityTail.ComponentMax; ++i)
                {
                    if (components[i] == componentType)
                    {
                        return components + i;
                    }
                }

                next = tail->next;
            }
            return null;
        }

        private static ushort* FindComponentLastPtr(ref ImplCaller caller, EntityId id)
        {
            var head = caller.UnsafeDirectReadPtr(id);

            if (head->count <= EntityHead.ComponentMax)
            {
                return head->components + head->count - 1;
            }
            else
            {
                var index = head->count - EntityHead.ComponentMax + EntityTail.ComponentMax;
                var next = head->next;
                EntityTail* tail = null;
                while (next != 0)
                {
                    tail = ReadAs<EntityTail>(ref caller, next);
                    next = tail->next;
                }
                return tail->components + index;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UChunk* ReadAs<UChunk>(ref ImplCaller caller, uint id)
            where UChunk : unmanaged
            => (UChunk*)caller.UnsafeDirectReadPtr(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(ref ImplCaller caller, uint id)
        {
            caller.Remove(id);
            ref var component = ref caller.UnsafeDirectRead(id);
            //MultiStorageActions<EntityHead>.RemoveDense(ref layout, ref depencies, id, ref component);    //TOD DO????????????????
        }
    }
}
