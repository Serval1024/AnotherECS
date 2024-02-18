using System.Collections.Generic;
using System.Linq;

namespace AnotherECS.Core
{
    internal static class LinqExtensions
    {
        public static IEnumerable<TSource> ExceptDuplicates<TSource>(this IEnumerable<TSource> source)
            => source
                .GroupBy(p => p)
                .Select(p => p.Key);
    }
}
