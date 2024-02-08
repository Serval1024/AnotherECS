using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Processing
{
    internal interface ISystemTaskHandler
    {
        State State { get; set; }
        ISystem AsSystem { get; }
    }

    internal interface ISystemTaskHandler<T> : ISystemTaskHandler
        where T : ISystem
    {
        T System { get; set; }
        ISystem ISystemTaskHandler.AsSystem { get => System; }
    }

    internal struct StateTickStartTaskHandler : ITaskHandler
    {
        public State State { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickStarted();
        }
    }

    internal struct StateTickFinishedTaskHandler : ITaskHandler
    {
        public State State { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickFinished();
        }
    }

    internal struct ConstructTaskHandler : ITaskHandler, ISystemTaskHandler<ICreateModule>
    {
        public State State { get; set; }
        public ICreateModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnCreateModule(State);
        }
    }

    internal struct SystemTickStartTaskHandler : ITaskHandler, ISystemTaskHandler<ITickStartedModule>
    {
        public State State { get; set; }
        public ITickStartedModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnTickStarted(State);
        }
    }

    internal struct SystemTickFinishedTaskHandler : ITaskHandler, ISystemTaskHandler<ITickFinishedModule>
    {
        public State State { get; set; }
        public ITickFinishedModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnTickFinished(State);
        }
    }

    internal struct SystemCreateTaskHandler : ITaskHandler, ISystemTaskHandler<ICreateSystem>
    {
        public State State { get; set; }
        public ICreateSystem System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnCreate(State);
        }
    }

    internal struct SystemTickTaskHandler : ITaskHandler, ISystemTaskHandler<ITickSystem>
    {
        public State State { get; set; }
        public ITickSystem System { get; set; }

        public void Invoke()
        {
            System.OnTick(State);
        }
    }

    internal struct SystemDestroyTaskHandler : ITaskHandler, ISystemTaskHandler<IDestroySystem>
    {
        public State State { get; set; }
        public IDestroySystem System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnDestroy(State);
        }
    }

    internal struct StateRevertToTaskHandler : ITaskHandler
    {
        public State State { get; set; }
        public uint tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.RevertTo(tick);
        }
    }

    internal struct ReceiversTaskHandler : ITaskHandler
    {
        public State State { get; set; }
        public Receivers receivers;
        public List<ITickEvent> events;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.FlushEvents();
            receivers.Receive(State, events);
        }
    }

}