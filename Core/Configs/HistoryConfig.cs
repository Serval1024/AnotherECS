namespace AnotherECS.Core
{
    public struct HistoryConfig
    {
        
        public uint dArrayBuffersCapacity;
        public uint buffersAddRemoveCapacity;
        public uint buffersChangeCapacity;
        public uint buffersFullCopyCapacity;
        public uint recordTickLength;
        public uint byTickArrayExtraSize;

        public static HistoryConfig Create()
            => new()
            {
                dArrayBuffersCapacity = 32,
                buffersAddRemoveCapacity = 32,
                buffersChangeCapacity = 128,
                buffersFullCopyCapacity = 20, //recordTickLength
                recordTickLength = 20,
                byTickArrayExtraSize = 16,
            };
    }
}