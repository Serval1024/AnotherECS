namespace AnotherECS.Core
{
    public struct GeneralConfig
    {
        public uint entityCapacity;

        public uint componentCapacity;
        public uint recycledCapacity;
        public uint dArrayCapacity;
        public uint filterCapacity;

        public static GeneralConfig Create()
           => new()
           {
               entityCapacity = 16,
               recycledCapacity = 256,

               componentCapacity = 64,
               dArrayCapacity = 32,
               filterCapacity = 32,
           };
    }
}