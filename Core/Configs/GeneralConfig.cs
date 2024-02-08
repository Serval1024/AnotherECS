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
               entityCapacity = 64,
               recycleCapacity = 32,

               componentCapacity = 32,
               archetypeCapacity = 16,

               chunkLimit = 32,
           };
    }
}