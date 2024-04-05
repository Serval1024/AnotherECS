namespace AnotherECS.Core
{
    public struct StateConfig
    {
        public GeneralConfig general;
        public HistoryConfig history;

        public static StateConfig Create()
            => new()
            {
                general = GeneralConfig.Create(),
                history = HistoryConfig.Create(),
            };
    }
}