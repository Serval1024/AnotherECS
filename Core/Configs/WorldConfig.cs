namespace AnotherECS.Core
{
    public struct WorldConfig
    {
        public GeneralConfig general;
        public HistoryConfig history;

        public static WorldConfig Create()
            => new()
            {
                general = GeneralConfig.Create(),
                history = HistoryConfig.Create(),
            };
    }
}