using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    [Serialize]
    public interface IEvent { }

    [Serialize]
    public interface ITickEvent
    {
        uint Tick { get; }
        IEvent Value { get; }
    }
}