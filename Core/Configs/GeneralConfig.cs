namespace AnotherECS.Core
{
    public struct GeneralConfig
    {
        public uint entityCapacity;

        public uint componentCapacity;
        public uint recycleCapacity;
        public uint archetypeCapacity;

        public uint chunkLimit;

        public static GeneralConfig Create()
           => new()
           {
               entityCapacity = 16,
               recycleCapacity = 256,

               componentCapacity = 64,
               archetypeCapacity = 32,

               chunkLimit = 32,
           };
    }
}