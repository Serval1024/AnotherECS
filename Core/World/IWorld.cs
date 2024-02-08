namespace AnotherECS.Core
{
    public interface IWorld
    {
        void Init();
        void Tick(uint tickCount);
        void Destroy();
        void Dispose();
        void Send(IEvent @event);
    }
}