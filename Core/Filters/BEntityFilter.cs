using System.Collections;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    public class BEntityFilter : BFilter, IFilter, IEnumerable<Entity>
    {
        public IEnumerator<Entity> GetEnumerator()
            => new EntityEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public sealed class EntityFilter<T0> : BEntityFilter
        where T0 : IComponent
    { }

    public sealed class EntityFilter<T0, T1> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2, T3> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2, T3, T4> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2, T3, T4, T5> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2, T3, T4, T5, T6> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
    { }

    public sealed class EntityFilter<T0, T1, T2, T3, T4, T5, T6, T7> : BEntityFilter
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
    { }
}
