using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public static class FilterUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetHash(ushort[] includes, ushort[] excludes)
        {
            var hash = includes.Length + excludes.Length;
            for (int i = 0, iMax = includes.Length; i < iMax; i++)
            {
                hash = unchecked(hash * 314159 + includes[i]);
            }
            for (int i = 0, iMax = excludes.Length; i < iMax; i++)
            {
                hash = unchecked(hash * 314159 - excludes[i]);
            }

            return hash;
        }
    }
}
