namespace AnotherECS.Core
{
    public struct InstallContext
    {
        private readonly World _world;
        private readonly State _state;
        private SystemGroup _systemGroup;

        internal InstallContext(World world)
        {
            _world = world;
            _state = world.State;
            _systemGroup = new SystemGroup();
        }

        public World World => _world;

        public SortOrder SystemSortOrder
        {
            get => _systemGroup.SortOrder;
            set => _systemGroup.SortOrder = value;
        }

        public void AddSystem(ISystem system)
        {
            _systemGroup.Add(system);
        }

        public void AddConfig<T>(T config)
            where T : IConfig
        {
            if (!_state.IsHasConfig<T>())
            {
                _state.AddConfig(config);
            }
        }

        public void AddConfig(IConfig config)
        {
            if (!_state.IsHasConfig(config.GetType()))
            {
                _state.AddConfig(config);
            }
        }

        public void AddSingle<T>(T single)
            where T : unmanaged, ISingle
        {
            if (!_state.IsHas<T>())
            {
                _state.Add(single);
            }
        }

        internal bool IsAny()
            => _systemGroup.IsValid && _systemGroup.SystemCount != 0;

        internal SystemGroup GetSystemGroup()
            => _systemGroup;
    }
}
