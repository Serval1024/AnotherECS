namespace AnotherECS.Core
{
    public struct GeneralConfig
    {
        public uint entityCapacity;
        public uint recycledCapacity;

        public uint componentCapacity;
        public uint dArrayCapacity;

        public uint gcEntityCheckPerTick;

        public static GeneralConfig Create()
           => new()
           {
               entityCapacity = 16,
               recycledCapacity = 256,

               componentCapacity = 64,
               dArrayCapacity = 32,

               gcEntityCheckPerTick = 8,
           };
    }
}