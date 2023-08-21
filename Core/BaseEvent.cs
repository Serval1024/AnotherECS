namespace AnotherECS.Core
{
    public abstract class BaseEvent : IEvent { }

    public interface IEvent { }

    public interface ITickEvent
    {
        uint Tick { get; }
        BaseEvent Value { get; }
    }
}