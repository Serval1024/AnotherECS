using AnotherECS.Core.Exceptions;
using System;

namespace AnotherECS.Core
{
    public interface IWorld : IDisposable
    {
        uint Id { get; }
        void Init();
        void Tick(uint tickCount);
        void Destroy();
        void SendEvent(IEvent @event);
        void UpdateFromMainThread();
    }

    public interface IWorldExtend : IWorld
    {
        void SendEvent(ITickEvent @event);
        ITickEvent ToITickEvent(IEvent @event);
        State GetState();
        void SetState(State state);
    }
}