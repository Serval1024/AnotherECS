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
            _state = world.GetState();
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
            _state.AddConfig(config);
        }

        public void AddConfig(IConfig config)
        {
            _state.AddConfig(config);
        }

        public void AddSingle<T>(T single)
            where T : unmanaged, ISingle
        {
            _state.Add(single);
        }

        internal SystemGroup GetSystemGroup()
            => _systemGroup;
    }
}
