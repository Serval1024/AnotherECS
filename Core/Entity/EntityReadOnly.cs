using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public struct EntityReadOnly : IEquatable<EntityReadOnly>, IRepairStateId
    {
        internal Entity entity;
        
        public EntityReadOnly(in Entity entity)
        {
            this.entity = entity;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entity.IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas()
            => entity.IsHas();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count()
            => entity.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<TComponent>()
            where TComponent : unmanaged, IComponent
            => entity.IsHas<TComponent>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(out T component)
            where T : unmanaged, IComponent
            => entity.TryRead(out component);
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(uint index)
            => entity.Read(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, IComponent
            => ref entity.Read<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion<T>()
            where T : unmanaged, IVersion
            => entity.GetVersion<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(EntityReadOnly p0, EntityReadOnly p1)
            => p0.entity == p1.entity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(EntityReadOnly p0, EntityReadOnly p1)
            => !(p0 == p1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(EntityReadOnly other)
            => this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => (obj is EntityReadOnly entity) && Equals(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => entity.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(EntityReadOnly other)
            => entity.CompareTo(other.entity);

        bool IRepairStateId.IsRepairStateId => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairStateId.RepairStateId(ushort stateId)
        {
            ComponentCompileUtils.RepairStateId(ref entity, stateId);
        }
    }
}