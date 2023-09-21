using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Actions;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe struct EntitiesCaller : ICaller<EntityHead>
    {
        private UnmanagedLayout<EntityHead>* _layout;
        private GlobalDepencies* _depencies;

        private int _gcEntityCheckPerTick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.Config(UnmanagedLayout* layout, GlobalDepencies* depencies, ushort id, State state)
        {
            Config((UnmanagedLayout<EntityHead>*)layout, depencies);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Config(UnmanagedLayout<EntityHead>* layout, GlobalDepencies* depencies)
        {
            _layout = layout;
            _depencies = depencies;
            _gcEntityCheckPerTick = (int)depencies->config.gcEntityCheckPerTick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ICaller.AllocateLayout()
        {
            EntitiesActions.AllocateLayout(ref *_layout, ref *_depencies);
            EntitiesActions.SetCurrentIndexGC(ref *_layout, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetElementType()
            => typeof(EntityHead);

        public EntityHead Create()
            => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(EntityId id)
            => id >= 1 && id < EntitiesActions.GetUpperBoundId(ref *_layout) && IsHasRaw(id);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHasRaw(EntityId id)
            => GetGeneration(id) >= EntitiesActions.AllocateGeneration;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponentCount(EntityId id)
            => EntitiesActions.GetComponentCount(ref *_layout, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponents(EntityId id, ushort[] buffer)
            => EntitiesActions.GetComponents(ref *_layout, id, buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetComponent(EntityId id, int index)
            => EntitiesActions.GetComponent(ref *_layout, id, index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetGeneration(EntityId id)
            => EntitiesActions.GetGeneration(ref *_layout, id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCount()
            => EntitiesActions.GetCount(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetCapacity()
            => EntitiesActions.GetCapacity(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResizeDense()
            => EntitiesActions.TryResizeDense(ref *_layout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(EntityId id, ushort componentType)
            => EntitiesActions.Add(ref *_layout, ref *_depencies, id, componentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(EntityId id, ushort componentType)
            => EntitiesActions.Remove(ref *_layout, ref *_depencies, id, componentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityId Allocate()
            => EntitiesActions.Allocate(ref *_layout, ref *_depencies);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(EntityId id)
            => EntitiesActions.Deallocate(ref *_layout, ref *_depencies, id);

        public void TickFinished()
        {
            if (_gcEntityCheckPerTick != 0)
            {
                InterationGarbageCollect(_gcEntityCheckPerTick);
            }
        }

        private void InterationGarbageCollect(int checkCount)
        {
            EntitiesActions.InterationGarbageCollect(ref *_layout, ref *_depencies, checkCount);
        }
    }
}
