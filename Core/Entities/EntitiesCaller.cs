using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Caller;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    using ImplCaller = Caller<
                uint, EntityData, uint, TOData<EntityData>, EntityData,
                UintNumber,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                RecycleStorageFeature<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                NonSparseFeature<EntityData, TOData<EntityData>, EntityData>,
                EntityDenseFeature<uint, EntityData, TOData<EntityData>>,
                Nothing,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
#if ANOTHERECS_HISTORY_DISABLE
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>,
#else
                ByChangeHistoryFeature<uint, EntityData, uint>,
#endif
                BBSerialize<uint, EntityData, uint, TOData<EntityData>>,
                Nothing<uint, EntityData, uint, TOData<EntityData>, EntityData>
                >;


#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct EntitiesCaller : ICaller<EntityData>, ITickFinishedCaller, IRevertCaller
    {
        private ImplCaller _impl;
        private GlobalDepencies* _depencies;

        public ushort ElementId => 0;
        public bool IsValide => _impl.IsValide;
        public bool IsSingle => false;
        public bool IsRevert => _impl.IsRevert;
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
            CallerWrapper.Config<ImplCaller, EntityData>(ref _impl, layout, depencies, 0, null);
            _depencies = depencies;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            CallerWrapper.AllocateLayout<ImplCaller, EntityData>(ref _impl);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityData ICaller<EntityData>.Create()
           => _impl.Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => _impl.GetElementType();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref uint ReadArchetypeId(EntityId id)
            => ref _impl.UnsafeDirectReadPtr(id)->archetypeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id < _impl.GetAllocated() && IsHasRaw(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => ReadGeneration(id) >= EntitiesActions.AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadGeneration(EntityId id)
            => EntitiesActions.GetGeneration(ref _impl, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => EntitiesActions.GetCount(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => EntitiesActions.GetCapacity(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeedResizeDense()
            => EntitiesActions.IsNeedResizeDense(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
            => EntitiesActions.TryResizeDense(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate()
            => EntitiesActions.Allocate(ref _impl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
            => EntitiesActions.Deallocate(ref _impl, id);

        public void TickFinished()
        {
            if (_depencies->archetype.GetFilterZeroCount() != 0)
            {
                const int idsLength = 32;
                var ids = stackalloc EntityId[idsLength];
                var count = _depencies->archetype.FilterZero(ids, idsLength);
                for (int i = 0; i < count; ++i)
                {
                    EntitiesActions.DeallocateZero(ref _impl, ids[i]);
                }
            }

            _impl.TickFinished();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, State state)
        {
            _impl.RevertTo(tick, state);
        }

        public void Add(uint id, ref EntityData component)
        {
            throw new NotSupportedException();
        }

        public ref EntityData Add(uint id)
        {
            throw new NotSupportedException();
        }

        public ref readonly EntityData Read(uint id)
            => ref _impl.Read(id);

        public ref EntityData Get(uint id)
            => ref _impl.Get(id);

        public void Set(uint id, ref EntityData component)
        {
            throw new NotSupportedException();
        }

        public void SetOrAdd(uint id, ref EntityData component)
        {
            throw new NotSupportedException();
        }

        public void RemoveRaw(uint id)
        {
            Remove(id);
        }

        public void Remove(uint id)
        {
            Deallocate(id);
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
            public static EntitiesCaller Install(State state)
                => state.AddLayout<EntitiesCaller, int, EntityData, uint, TOData<EntityData>>();
        }
    }


    internal static unsafe class EntitiesActions
    {
        internal const ushort AllocateGeneration = 32768;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static EntityId Allocate(ref ImplCaller caller)
        {
            var id = caller.UnsafeAllocate();
            ref var head = ref caller.Get(id);

            head.generation += AllocateGeneration + 1;
            if (head.generation == ushort.MaxValue)
            {
                head.generation = AllocateGeneration;
            }

            return id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deallocate(ref ImplCaller caller, EntityId id)
        {
            ref var head = ref caller.Get(id);
            head.generation -= AllocateGeneration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNeedResizeDense(ref ImplCaller caller)
            => caller.IsNeedResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense(ref ImplCaller caller)
            => caller.TryResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryIncResizeDense(ref ImplCaller caller)
            => caller.TryIncResizeDense();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint GetArchetypeId(ref ImplCaller caller, EntityId id)
            => ref caller.UnsafeDirectReadPtr(id)->archetypeId;

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
           => caller.GetCapacity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeallocateZero(ref ImplCaller caller, EntityId id)
        {
            ref var head = ref caller.Get(id);
            head.generation -= AllocateGeneration;
            caller.Remove(id);
        }
    }
}
