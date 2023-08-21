namespace AnotherECS.Core
{
    public interface ISystem { }
    public interface ISystem<TState> : ISystem
        where TState : State
    { }

    public interface IReceiverSystem : ISystem
    { }

    public interface IReceiverSystem<UEvent> : IReceiverSystem
        where UEvent : BaseEvent
    {
        void Receive(State state, UEvent @event);
    }

    public interface IReceiverSystem<TState, UEvent> : IReceiverSystem<UEvent>
        where TState : State
        where UEvent : BaseEvent
    {
        void Receive(TState state, UEvent @event);
        void IReceiverSystem<UEvent>.Receive(State state, UEvent @event)
            => Receive((TState)state, @event);
    }

    public interface IInitSystem : ISystem
    {
        void Init(State state);
    }

    public interface IInitSystem<TState> : ISystem<TState>, IInitSystem
        where TState : State
    {
        void Init(TState state);
        void IInitSystem.Init(State state)
            => Init((TState)state);
    }

    public interface ITickSystem : ISystem
    {
        void Tick(State state);
    }

    public interface ITickSystem<TState> : ISystem<TState>, ITickSystem
        where TState : State
    {
        void Tick(TState state);
        void ITickSystem.Tick(State state)
            => Tick((TState)state);
    }

    public interface IDestroySystem : ISystem
    {
        void Destroy(State state);
    }
    public interface IDestroySystem<TState> : ISystem<TState>, IDestroySystem
        where TState : State
    {
        void Destroy(TState state);
        void IDestroySystem.Destroy(State state)
            => Destroy((TState)state);
    }
}