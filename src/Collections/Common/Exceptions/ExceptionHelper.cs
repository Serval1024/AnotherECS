using AnotherECS.Core.Collection;
using System.Runtime.CompilerServices;

namespace AnotherECS.Collections.Exceptions
{
    internal static class ExceptionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfBroken<TValid>(TValid native)
            where TValid : struct, IValid
        {
            if (!native.IsValid)
            {
                throw new DCollectionInvalidException(typeof(TValid));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfChange(bool isChange)
        {
            if (isChange)
            {
                throw new CollectionWasModifiedException();
            }
        }
    }
}