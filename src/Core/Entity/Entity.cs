using System;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

[assembly: InternalsVisibleTo("AnotherECS.Unity.Views")]
namespace AnotherECS.Core
{
    public struct Entity : IEquatable<Entity>, IRepairStateId
    {
        public const EntityId Zero = 0;
        public static readonly Entity Null = new();

        internal EntityId id;
        internal ushort generation;
        internal ushort stateId;

        internal State State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if !ANOTHERECS_RELEASE
                if (stateId == 0)
                {
                    throw new Exceptions.InvalidEntityException();
                }
#endif
                return StateGlobalRegister.Get(stateId);
            }
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => id != 0 && stateId != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas()
            => State.IsHas(id, generation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count()
            => State.GetCount(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete()
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Delete(id);
            this = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas<TComponent>()
            where TComponent : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return State.IsHas<TComponent>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(out T component)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return State.TryRead(id, out component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return State.Read(id, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, IComponent component)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Set(id, index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return ref State.Read<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(out T component)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return State.TryGet(id, out component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return ref State.Get<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(T data)
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return ref State.Add<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid<T>()
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.AddVoid<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            State.Remove<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVersion<T>()
            where T : unmanaged, IVersion
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalid();
#endif
            return State.GetVersion<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity p0, Entity p1)
            => p0.id == p1.id && p0.generation == p1.generation && p0.State == p1.State;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Entity p0, Entity p1)
            => !(p0 == p1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Entity other)
            => this == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
            => (obj is Entity entity) && Equals(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => (int)id ^ generation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Entity other)
            => id.CompareTo(other.id);

        public EntityReadOnly ToReadOnly()
            => new(this);

        bool IRepairStateId.IsRepairStateId => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairStateId.RepairStateId(ushort stateId)
        {
            this.stateId = stateId;
        }

#if !ANOTHERECS_RELEASE
        private void ThrowIfInvalid()
        {
            if (!IsHas())
            {
                throw new Exceptions.EntityNotFoundException(id);
            }
        }
#endif
    }
}