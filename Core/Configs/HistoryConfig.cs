namespace AnotherECS.Core
{
    public struct HistoryConfig
    {
        public uint recordTickLength;
        public uint buffersCapacity;

        public static HistoryConfig Create()
            => new()
            {
                recordTickLength = 20,
                buffersCapacity = 1024,
            };
    }
}