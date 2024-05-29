namespace AnotherECS.Core.Remote
{
    public readonly struct StateRespond
    {
        public WorldData Data { get; }
        public SerializationLevel SerializationLevel { get; }

        public StateRespond(WorldData data, SerializationLevel serializationLevel)
        {
            Data = data;
            SerializationLevel = serializationLevel;
        }
    }
}