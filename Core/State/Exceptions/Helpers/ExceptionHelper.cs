using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using System;
using System.Collections.Generic;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Exceptions
{
    internal static class ExceptionHelper
    {
        public static void ThrowIfWorldDisposed(BDisposable disposable, bool isInit)
        {
            ThrowIfDisposed(disposable);

            if (!isInit)
            {
                throw new InvalidOperationException("World not init yet.");
            }
        }

        public static void ThrowIfDisposed(BDisposable state)
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
                throw new EntityNotFoundException(id);
            }
        }

        public static void ThrowIfDontExists<T>(State state, EntityId id, ICaller<T> caller)
            where T : unmanaged, IComponent
        {
            ThrowIfDisposed(state);
            ThrowIfNotMultiAccess(state, id, caller);

            if (!state.IsHas<T>(id))
            {
                throw new ComponentNotFoundException(typeof(T));
            }
        }

        public static void ThrowIfExists(State state, EntityId id, ICaller caller)
        {
            ThrowIfDisposed(state);
            ThrowIfNotMultiAccess(state, id, caller);

            if (caller.IsHas(id))
            {
                throw new ComponentExistsException(caller.GetElementType());
            }
        }

        public static void ThrowIfNotMultiAccess(State state, EntityId id, ICaller caller)
        {
            ThrowIfDisposed(state);
            ThrowIfDontExists(state, id);

            if (caller.IsSingle)
            {
                throw new ComponentNotMultiException(caller.GetElementType());
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
                throw new ComponentNotFoundException(caller.GetElementType());
            }
        }

        public static void ThrowIfExists<T>(State state, ICaller<T> caller)
            where T : unmanaged, ISingle
        {
            ThrowIfDisposed(state);
            ThrowIfNotSingleAccess(state, caller);

            if (state.IsHas<T>())
            {
                throw new ComponentExistsException(caller.GetElementType());
            }
        }

        public static void ThrowIfNotSingleAccess<T>(State state, ICaller<T> caller)
            where T : unmanaged, IComponent
        {
            ThrowIfDisposed(state);

            if (!caller.IsSingle)
            {
                throw new ComponentNotSingleException(typeof(T));
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
            ThrowIfBroken(narray);
            ThrowIfNArrayBroken(narray, index, narray.ByteLength / narray.ElementSize);
        }

        public static void ThrowIfNArrayBroken<TNArray>(TNArray narray, ulong index, ulong count)
            where TNArray : struct, INArray
        {
            ThrowIfBroken(narray);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)}, value '{index}'");
            }
            if (index >= count)
            {
                throw new ArgumentOutOfRangeException($"{nameof(index)}, value '{index}'");
            }
        }

        public static void ThrowIfBroken<TNative>(TNative native)
            where TNative : struct, INative
        {
            if (!native.IsValid)
            {
                throw new InvalidOperationException(nameof(native.IsValid));
            }
        }

        public static void ThrowIfNContainerBroken<TNative>(TNative container)
            where TNative : struct, INative
        {
            if (!container.IsValid)
            {
                throw new InvalidOperationException(nameof(container.IsValid));
            }
        }

        public static void ThrowIfEmpty<T>(ICaller<T> caller)
            where T : unmanaged, IComponent
        {
            if (caller.GetDenseMemoryAllocated == 0u)
            {
                throw new ComponentHasNoDataException(typeof(T));
            }
        }

        public static void ThrowIfExists<T>(State state, uint id, IConfig[] configs)
           where T : IConfig
        {
            ThrowIfDisposed(state);

            if (configs[id] != null)
            {
                throw new ConfigExistsException(typeof(T));
            }
        }

        public static void ThrowIfDontExists<T>(State state, uint id, IConfig[] configs)
           where T : IConfig
        {
            ThrowIfDisposed(state);

            if (configs[id] == null)
            {
                throw new ConfigNotFoundException(typeof(T));
            }
        }

        public static void ThrowIfDontExists(State state, Type type, IConfig[] configs, Dictionary<Type, uint> configByType)
        {
            ThrowIfDisposed(state);

            if (!configByType.ContainsKey(type) || configs[configByType[type]] == null)
            {
                throw new ConfigNotFoundException(type);
            }
        }

        public static void ThrowIfExists(State state, Type type, IConfig[] configs, Dictionary<Type, uint> configByType)
        {
            ThrowIfDisposed(state);

            if (configByType.ContainsKey(type) && configs[configByType[type]] != null)
            {
                throw new ConfigExistsException(type);
            }
        }
    }
}