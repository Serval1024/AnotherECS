namespace AnotherECS.Core
{
    public struct GeneralConfig
    {
        public uint entityCapacity;
        public uint recycledCapacity;
        public uint gcEntityCheckPerTick;

        public uint dArrayCapacity;

        public uint filterCapacity;

        public uint componentCapacity;

        public uint markerCapacity;
        public uint markerBacketSize;


        public HistoryConfig history;

        public static GeneralConfig Create()
            => new()
            {
                entityCapacity = 16,
                recycledCapacity = 256,
                gcEntityCheckPerTick = 8,

                dArrayCapacity = 32,

                filterCapacity = 256,

                componentCapacity = 64,

                markerCapacity = 64,
                markerBacketSize = 16,


                history = HistoryConfig.Create(),
            };
    }
}