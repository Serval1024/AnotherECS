using System.Collections;
using System.Collections.Generic;
using EntityId = System.UInt32;

namespace AnotherECS.Core
{
    public class BEntityIdFilter : BFilter, IFilter, IEnumerable<EntityId>
    {
        public IEnumerator<EntityId> GetEnumerator()
          => new EntityIdEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public sealed class IdFilter<T0> : BEntityIdFilter
        where T0 : IComponent
    { }

    public sealed class IdFilter<T0, T1> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2, T3> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2, T3, T4> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2, T3, T4, T5> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2, T3, T4, T5, T6> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
    { }

    public sealed class IdFilter<T0, T1, T2, T3, T4, T5, T6, T7> : BEntityIdFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
    { }
}
