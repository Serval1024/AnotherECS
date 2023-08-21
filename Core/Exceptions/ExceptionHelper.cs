using EntityId = System.Int32;

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

        public static void ThrowIfInvalide(IDebugException state, ISinglePool pool, bool isComponentExists = true)
        {
            ThrowIfDisposed(state);

            if (isComponentExists)
            {
                if (!pool.IsHas())
                {
                    throw new Exceptions.ComponentNotFoundedException(pool.GetElementType());
                }
            }
            else
            {
                if (pool.IsHas())
                {
                    throw new Exceptions.ComponentExistsException(pool.GetElementType());
                }
            }
        }

        public static void ThrowIfInvalide(IDebugException state, EntityId id, IEntityPool pool, bool isComponentExists = true)
        {
            ThrowIfDisposed(state);
            ThrowIfInvalide(state, id);

            if (isComponentExists)
            {
                if (!pool.IsHas(id))
                {
                    throw new Exceptions.ComponentNotFoundedException(pool.GetElementType());
                }
            }
            else
            {
                if (pool.IsHas(id))
                {
                    throw new Exceptions.ComponentExistsException(pool.GetElementType());
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
