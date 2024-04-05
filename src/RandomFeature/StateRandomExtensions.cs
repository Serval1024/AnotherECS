using AnotherECS.Core;
using System.Runtime.CompilerServices;

namespace AnotherECS.Random
{
    public static class StateRandomExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref Mathematics.Random GetRandom(this State state)
        {
#if !ANOTHERECS_RELEASE
            if (!state.IsHas<RandomSingle>())
            {
                throw new Core.Exceptions.FeatureNotExists(nameof(RandomFeature));
            }
#endif
            return ref state.Get<RandomSingle>().value;
        }
    }
}