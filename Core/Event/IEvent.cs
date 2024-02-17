namespace AnotherECS.Core
{
    public interface IEvent { }

    public interface ITickEvent
    {
        uint Tick { get; }
        IEvent Value { get; }
    }
}