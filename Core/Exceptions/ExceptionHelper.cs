#if !ANOTHERECS_RELEASE
using System;
using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    internal static class ExceptionHelper
    {
        public static void ThrowIfDisposed(State state)
        {
            if (state.IsDisposed)
            {
                throw new ObjectDisposedException(state.GetType().Name);
            }
        }

        public static void ThrowIfDontExists(State state, EntityId id)
        {
            ThrowIfDisposed(state);

            if (!state.IsHas(id))
            {
                throw new Exceptions.EntityNotFoundException(id);
            }
        }

        public static void ThrowIfDontExists<T>(State state, EntityId id, ICaller<T> caller)
            where T : unmanaged, IComponent
        {
            ThrowIfDisposed(state);
            ThrowIfNotMultiAccess(state, id, caller);

            if (!state.IsHas<T>(id))
            {
                throw new Exceptions.ComponentNotFoundException(typeof(T));
            }
        }

        public static void ThrowIfExists<T>(State state, EntityId id, ICaller<T> caller)
           where T : unmanaged, IComponent
        {
            ThrowIfDisposed(state);
            ThrowIfNotMultiAccess(state, id, caller);

            if (state.IsHas<T>(id))
            {
                throw new Exceptions.ComponentExistsException(typeof(T));
            }
        }

        public static void ThrowIfNotMultiAccess(State state, EntityId id, ICaller caller)
        {
            ThrowIfDisposed(state);
            ThrowIfDontExists(state, id);

            if (caller.IsSingle)
            {
                throw new Exceptions.ComponentNotMultiException(caller.GetElementType());
            }
        }

        public static void ThrowIfDontExists(State state, uint id, uint index, uint count, ICaller caller)
        {
            ThrowIfDisposed(state);
            ThrowIfNotMultiAccess(state, id, caller);

            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range component count: {count}.");
            }
        }

        public static void ThrowIfDontExists<T>(State state, ICaller<T> caller)
            where T : unmanaged, IShared
        {
            ThrowIfDisposed(state);
            ThrowIfNotSingleAccess(state, caller);

            if (!state.IsHas<T>())
            {
                throw new Exceptions.ComponentNotFoundException(caller.GetElementType());
            }
        }

        public static void ThrowIfExists<T>(State state, ICaller<T> caller)
            where T : unmanaged, IShared
        {
            ThrowIfDisposed(state);
            ThrowIfNotSingleAccess(state, caller);

            if (state.IsHas<T>())
            {
                throw new Exceptions.ComponentExistsException(caller.GetElementType());
            }
        }

        public static void ThrowIfNotSingleAccess<T>(State state, ICaller<T> caller)
            where T : unmanaged, IComponent
        {
            ThrowIfDisposed(state);

            if (caller.IsSingle)
            {
                throw new Exceptions.ComponentNotSharedException(typeof(T));
            }
        }

        public static void ThrowIfNArrayBroken(NArray narray, ulong index, uint segmentSize)
        {
            ThrowIfNArrayBroken(narray);
            if (index * segmentSize >= narray.ByteLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static void ThrowIfNArrayBroken(NArray narray, uint index, uint segmentSize)
        {
            ThrowIfNArrayBroken(narray);
            if (index * segmentSize >= narray.ByteLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static void ThrowIfNArrayBroken(NArray narray, int index, uint segmentSize)
        {
            ThrowIfNArrayBroken(narray);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            ThrowIfNArrayBroken(narray, (uint)index, segmentSize);
        }

        public static void ThrowIfNArrayBroken<T>(NArray<T> narray, ulong index)
        where T : unmanaged
        {
            ThrowIfNArrayBroken(narray);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            ThrowIfNArrayBroken(narray, (uint)index);
        }

        public static void ThrowIfNArrayBroken<T>(NArray<T> narray, int index)
           where T : unmanaged
        {
            ThrowIfNArrayBroken(narray);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            ThrowIfNArrayBroken(narray, (uint)index);
        }

        public static void ThrowIfNArrayBroken<T>(NArray<T> narray, uint index)
            where T : unmanaged
        {
            ThrowIfNArrayBroken(narray);
            if (index >= narray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static void ThrowIfNArrayBroken(NArray narray)
        {
            if (!narray.IsValide)
            {
                throw new InvalidOperationException(nameof(narray.IsValide));
            }
        }

        public static void ThrowIfNArrayBroken<T>(NArray<T> narray)
            where T : unmanaged
        {
            if (!narray.IsValide)
            {
                throw new InvalidOperationException(nameof(narray.IsValide));
            }
        }

        public static void ThrowIfNContainerBroken<T>(NContainer<T> container)
            where T : unmanaged
        {
            if (!container.IsValide)
            {
                throw new InvalidOperationException(nameof(container.IsValide));
            }
        }
    }
}
#endif
