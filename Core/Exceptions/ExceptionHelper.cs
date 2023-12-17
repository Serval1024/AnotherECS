using System;
using System.Runtime.CompilerServices;
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
            where T : unmanaged, ISingle
        {
            ThrowIfDisposed(state);
            ThrowIfNotSingleAccess(state, caller);

            if (!state.IsHas<T>())
            {
                throw new Exceptions.ComponentNotFoundException(caller.GetElementType());
            }
        }

        public static void ThrowIfExists<T>(State state, ICaller<T> caller)
            where T : unmanaged, ISingle
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
                throw new Exceptions.ComponentNotSingleException(typeof(T));
            }
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, int index)
           where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray, (ulong)index);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, uint index, uint count)
            where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray, (ulong)index, (ulong)count);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, int index, uint count)
            where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray, (ulong)index, (ulong)count);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, uint index)
            where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray, (ulong)index);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, ulong index)
            where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray);
            ThrowIfNArrayBroken(narray, index, narray.ByteLength / narray.ElementSize);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, ulong index, ulong count)
            where TNArray : struct, INArray
        {
            ThrowIfNArrayBroken(narray);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray)
            where TNArray : struct, INArray
        {
            if (!narray.IsValide)
            {
                throw new InvalidOperationException(nameof(narray.IsValide));
            }
        }

        public static void ThrowIfNContainerBroken<TNative>(TNative container)
            where TNative : struct, INative
        {
            if (!container.IsValide)
            {
                throw new InvalidOperationException(nameof(container.IsValide));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfChange(bool isChange)
        {
            if (isChange)
            {
                throw new Exceptions.CollectionWasModifiedException();
            }
        }
    }
}