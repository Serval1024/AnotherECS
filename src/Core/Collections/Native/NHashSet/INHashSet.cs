using System.Collections.Generic;

namespace AnotherECS.Core.Collection
{
    public interface INHashSet<TValue> : INative, IEnumerable<TValue>
        where TValue : unmanaged { }
}