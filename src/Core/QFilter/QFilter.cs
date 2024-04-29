using AnotherECS.Core.Caller;
using AnotherECS.Core.Collection;
using System.Collections;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    internal interface IQFilter
    { 
        void Construct(State state, ICaller caller); 
    };

    internal interface IQFilter<TComponent> : IQFilter
        where TComponent : unmanaged, IComponent { };

    public struct QFilter<TComponent> : IQFilter<TComponent>, IFilter, IEnumerable<TComponent>
        where TComponent : unmanaged, IComponent
    {
        private State _state;
        private ICaller<TComponent> _caller;

        internal QFilter(State state, ICaller<TComponent> caller)
        {
            _state = state;
            _caller = caller;
        }

        void IQFilter.Construct(State state, ICaller caller)
        {
            _state = state;
            _caller = (ICaller<TComponent>)caller;
        }

        public void Each<TIterator>(TIterator iterator = default)
            where TIterator : struct, IIterator<TComponent>
        {
            Each(ref iterator);
        }

        public void Each<TIterator>(ref TIterator iterator)
            where TIterator : struct, IIterator<TComponent>
        {
            var wrapper = new QFilterIteratorWrapper<TComponent, TIterator>() { data = iterator };
            _caller.Each(ref wrapper);
        }

        public IEnumerator<TComponent> GetEnumerator()
            => _caller.GetEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
