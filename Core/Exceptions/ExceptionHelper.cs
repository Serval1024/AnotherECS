using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    internal static class ExceptionHelper
    {
#if ANOTHERECS_DEBUG
        public static void ThrowIfDisposed(IDebugException state)
        {
            if (state.IsDisposed)
            {
                throw new System.ObjectDisposedException(state.GetType().Name);
            }
        }

        public static void ThrowIfInvalide(IDebugException state, ISingleStorage storage, bool isComponentExists = true)
        {
            ThrowIfDisposed(state);

            if (isComponentExists)
            {
                if (!storage.IsHas())
                {
                    throw new Exceptions.ComponentNotFoundException(storage.GetElementType());
                }
            }
            else
            {
                if (storage.IsHas())
                {
                    throw new Exceptions.ComponentExistsException(storage.GetElementType());
                }
            }
        }

        public static void ThrowIfInvalide(IDebugException state, EntityId id, IEntityStorage storage, bool isComponentExists = true)
        {
            ThrowIfDisposed(state);
            ThrowIfInvalide(state, id);

            if (isComponentExists)
            {
                if (!storage.IsHas(id))
                {
                    throw new Exceptions.ComponentNotFoundException(storage.GetElementType());
                }
            }
            else
            {
                if (storage.IsHas(id))
                {
                    throw new Exceptions.ComponentExistsException(storage.GetElementType());
                }
            }
        }

        public static void ThrowIfInvalide(IDebugException state, EntityId id)
        {
            ThrowIfDisposed(state);

            if (!state.IsHas(id))
            {
                throw new Exceptions.EntityNotFoundException(id);
            }
        }
#endif
    }
}
