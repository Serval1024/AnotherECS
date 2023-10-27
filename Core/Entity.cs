using System.Runtime.CompilerServices;
using System;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public struct Entity : IEquatable<Entity>
    {
        public static readonly Entity Null = new();

        internal EntityId id;
        internal ushort generation;
        internal State state;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas()
            => state.IsHas(id, generation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count()
            => state.Count(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete()
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Delete(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent Read(int index)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            return state.Read(id, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, IComponent component)
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Set(id, index, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Read<T>()
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            return ref state.Read<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            return ref state.Get<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(T data)
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Set(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            return ref state.Add<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(ref T data)
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Add(id, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVoid<T>()
          where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.AddVoid<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove<T>()
            where T : unmanaged, IComponent
        {
#if !ANOTHERECS_RELEASE
            ThrowIfInvalide();
#endif
            state.Remove<T>(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity p0, Entity p1)
            => p0.id == p1.id && p0.generation == p1.generation && p0.state == p1.state;

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
            => (int)id ^ generation ^ state.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Entity other)
            => id.CompareTo(other.id);

#if !ANOTHERECS_RELEASE
        private void ThrowIfInvalide()
        {
            if (!IsHas())
            {
                throw new Exceptions.EntityNotFoundException(id);
            }
        }
#endif
    }
}