using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Processing
{
    internal interface ISystemTaskHandler<T>
        where T : ISystem
    {
        State State { set; get; }
        T System { set; get; }
    }

    internal struct StateTickStartTaskHandler : ITaskHandler
    {
        public State State { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickStarted();
        }
    }

    internal struct StateTickFinishedTaskHandler : ITaskHandler
    {
        public State State { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickFinished();
        }
    }

    internal struct ConstructTaskHandler : ITaskHandler, ISystemTaskHandler<IConstructModule>
    {
        public State State { set; get; }
        public IConstructModule System { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.Construct(State);
        }
    }

    internal struct SystemTickStartTaskHandler : ITaskHandler, ISystemTaskHandler<ITickStartModule>
    {
        public State State { set; get; }
        public ITickStartModule System { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.TickStarted(State);
        }
    }

    internal struct SystemTickFinishedTaskHandler : ITaskHandler, ISystemTaskHandler<ITickFinishedModule>
    {
        public State State { set; get; }
        public ITickFinishedModule System { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.TickFinished(State);
        }
    }

    internal struct SystemInitTaskHandler : ITaskHandler, ISystemTaskHandler<IInitSystem>
    {
        public State State { set; get; }
        public IInitSystem System { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.Init(State);
        }
    }

    internal struct SystemTickTaskHandler : ITaskHandler, ISystemTaskHandler<ITickSystem>
    {
        public State State { set; get; }
        public ITickSystem System { set; get; }

        public void Invoke()
        {
            System.Tick(State);
        }
    }

    internal struct SystemDestroyTaskHandler : ITaskHandler, ISystemTaskHandler<IDestroySystem>
    {
        public State State { set; get; }
        public IDestroySystem System { set; get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.Destroy(State);
        }
    }

    internal struct StateRevertToTaskHandler : ITaskHandler
    {
        public State State { set; get; }
        public uint tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.RevertTo(tick);
        }
    }

    internal struct ReceiversTaskHandler : ITaskHandler
    {
        public State State { set; get; }
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