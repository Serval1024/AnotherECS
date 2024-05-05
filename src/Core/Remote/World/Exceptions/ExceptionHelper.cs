using System;

namespace AnotherECS.Core.Remote.Exceptions
{
    internal static class ExceptionHelper
    {
        public static void ThrowIfWorldInvalid(IWorldComposite world)
        {
            if (world == null || world.InnerWorld == null)
            {
                throw new InvalidOperationException("The world is not obtained from the network or initialized.");
            }
        }
    }
}