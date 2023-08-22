namespace AnotherECS.Core
{
    public struct GeneralConfig
    {
        public uint componentMaxPerEntity;

        public uint entityCapacity;
        public uint recycledCapacity;
        public uint filterCapacity;

        public uint componentCapacity;

        public uint markerCapacity;
        public uint markerBacketSize;

        public uint dArrayCapacity;

        public uint gcEntityCheckPerTick;

        public HistoryConfig history;

        public static GeneralConfig Create()
            => new()
            {
                entityCapacity = 16,
                recycledCapacity = 256,
                filterCapacity = 256,

                componentCapacity = 64,

                markerCapacity = 64,
                markerBacketSize = 16,

                dArrayCapacity = 32,

                gcEntityCheckPerTick = 8,

                history = HistoryConfig.Create(),
            };
    }
}