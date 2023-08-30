namespace AnotherECS.Core
{
    /*
    internal struct FilterHistoryFactory
    {
        private HistoryConfig _config;
        private readonly Histories _root;

        public FilterHistoryFactory(in HistoryConfig config, Histories root)
        {
            _config = config;
            _root = root;
        }

        public FilterHistory Create(Filter filter)
        {
            var inst = new FilterHistory(_config, _root.TickProvider);
            inst.SetSubject(filter);
            _root.RegisterChild(inst);
            return inst;
        }
    }
    */
}