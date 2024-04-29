#if ANOTHERECS_RELEASE
using AnotherECS.Unsafe;
#endif

using AnotherECS.Serializer;

namespace AnotherECS.Core
{
    [Serialize]
    public interface ISystem { }
    public interface ISystem<TState> : ISystem
        where TState : State { }

    public interface IReceiverSystem : ISystem { }

    public interface IReceiverSystem<UEvent> : IReceiverSystem<State, UEvent>
        where UEvent : IEvent { }

    public interface IReceiverSystem<TState, UEvent> : IReceiverSystem
        where TState : State
        where UEvent : IEvent
    {
        void Receive(TState state, UEvent @event);
    }

    public interface ICreateSystem : ISystem
    {
        void OnCreate(State state);
    }

    public interface ICreateSystem<TState> : ISystem<TState>, ICreateSystem
        where TState : State
    {
        void OnCreate(TState state);
        void ICreateSystem.OnCreate(State state)
#if !ANOTHERECS_RELEASE
            => OnCreate((TState)state);
#else
            => OnCreate(UnsafeUtils.As<State, TState>(ref state));
#endif
    }

    public interface ITickSystem : ISystem
    {
        void OnTick(State state);
    }

    public interface ITickSystem<TState> : ISystem<TState>, ITickSystem
        where TState : State
    {
        void OnTick(TState state);
        void ITickSystem.OnTick(State state)
#if !ANOTHERECS_RELEASE
            => OnTick((TState)state);
#else
            => OnTick(UnsafeUtils.As<State, TState>(ref state));
#endif
    }

    public interface IDestroySystem : ISystem
    {
        void OnDestroy(State state);
    }

    public interface IDestroySystem<TState> : ISystem<TState>, IDestroySystem
        where TState : State
    {
        void OnDestroy(TState state);
        void IDestroySystem.OnDestroy(State state)
#if !ANOTHERECS_RELEASE
            => OnDestroy((TState)state);
#else
            => OnDestroy(UnsafeUtils.As<State, TState>(ref state));
#endif
    }

    public interface IInstallSystem : ISystem
    {
        void Install(ref InstallContext context);
    }

    public interface ISyncThread : ISystem { }
    public interface IMainThread : ISyncThread { }
}