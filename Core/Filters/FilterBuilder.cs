namespace AnotherECS.Core
{
    public struct FilterBuilder
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state)
        {
            this = new FilterBuilder(state, default);
        }

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T>(_state, _mask);
        }

        public FilterBuilder Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder(_state, _mask);
        }
        
        public IdFilter BuildAsId()
          => _state.CreateFilter<IdFilter>(ref _mask);
        
        public EntityFilter BuildAsEntity()
            => _state.CreateFilter<EntityFilter>(ref _mask);
    }

    public struct FilterBuilder<T0>
        where T0 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T0, T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T>(_state, _mask);
        }

        public FilterBuilder<T0> Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0>(_state, _mask);
        }

        public IdFilter<T0> BuildAsId()
            => _state.CreateFilter<IdFilter<T0>>(ref _mask);

        public EntityFilter<T0> BuildAsEntity()
            => _state.CreateFilter<EntityFilter<T0>>(ref _mask);
    }

    public struct FilterBuilder<T0, T1>
        where T0 : IComponent
        where T1 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T0, T1, T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T>(_state, _mask);
        }

        public FilterBuilder<T0, T1> Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1>(_state, _mask);
        }

        public IdFilter<T0, T1> BuildAsId()
            => _state.CreateFilter<IdFilter<T0, T1>>(ref _mask);

        public EntityFilter<T0, T1> BuildAsEntity()
            => _state.CreateFilter<EntityFilter<T0, T1>>(ref _mask);
    }

    public struct FilterBuilder<T0, T1, T2>
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T0, T1, T2, T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2, T>(_state, _mask);
        }

        public FilterBuilder<T0, T1, T2> Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2>(_state, _mask);
        }

        public IdFilter<T0, T1, T2> BuildAsId()
            => _state.CreateFilter<IdFilter<T0, T1, T2>>(ref _mask);

        public EntityFilter<T0, T1, T2> BuildAsEntity()
            => _state.CreateFilter<EntityFilter<T0, T1, T2>>(ref _mask);
    }

    public struct FilterBuilder<T0, T1, T2, T3>
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T0, T1, T2, T3, T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2, T3, T>(_state, _mask);
        }

        public FilterBuilder<T0, T1, T2, T3> Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2, T3>(_state, _mask);
        }

        public IdFilter<T0, T1, T2, T3> BuildAsId()
            => _state.CreateFilter<IdFilter<T0, T1, T2, T3>>(ref _mask);

        public EntityFilter<T0, T1, T2, T3> BuildAsEntity()
            => _state.CreateFilter<EntityFilter<T0, T1, T2, T3>>(ref _mask);
    }

    public struct FilterBuilder<T0, T1, T2, T3, T4>
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public FilterBuilder<T0, T1, T2, T3, T4, T> With<T>()
            where T : IComponent
        {
            _mask.AddInclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2, T3, T4, T>(_state, _mask);
        }

        public FilterBuilder<T0, T1, T2, T3, T4> Without<T>()
            where T : IComponent
        {
            _mask.AddExclude(_state.GetIdByType<T>());
            return new FilterBuilder<T0, T1, T2, T3, T4>(_state, _mask);
        }

        public IdFilter<T0, T1, T2, T3, T4> BuildAsId()
            => _state.CreateFilter<IdFilter<T0, T1, T2, T3, T4>>(ref _mask);

        public EntityFilter<T0, T1, T2, T3, T4> BuildAsEntity()
            => _state.CreateFilter<EntityFilter<T0, T1, T2, T3, T4>>(ref _mask);
    }

    public struct FilterBuilder<T0, T1, T2, T3, T4, T5>
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    {
        private readonly State _state;
        private Mask _mask;

        internal FilterBuilder(State state, in Mask mask)
        {
            _state = state;
            _mask = mask;
        }

        public IdFilter<T0, T1, T2, T3, T4, T5> BuildAsId()
           => _state.CreateFilter<IdFilter<T0, T1, T2, T3, T4, T5>>(ref _mask);

        public EntityFilter<T0, T1, T2, T3, T4, T5> BuildAsEntity()
           => _state.CreateFilter<EntityFilter<T0, T1, T2, T3, T4, T5>>(ref _mask);
    }
}
