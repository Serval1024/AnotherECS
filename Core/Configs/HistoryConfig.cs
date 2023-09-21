namespace AnotherECS.Core
{
    public struct HistoryConfig
    {
        public uint recordTickLength;

        public uint dArrayBuffersCapacity;
        public uint buffersAddRemoveCapacity;
        public uint buffersChangeCapacity;
        public uint buffersFullCopyCapacity;
        public uint byTickArrayExtraSize;

        public static HistoryConfig Create()
            => new()
            {
                recordTickLength = 20,

                dArrayBuffersCapacity = 32,
                buffersAddRemoveCapacity = 32,
                buffersChangeCapacity = 128,
                buffersFullCopyCapacity = 20, //recordTickLength
                byTickArrayExtraSize = 16,
            };
    }
}