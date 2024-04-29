using AnotherECS.Core.Remote;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Processing
{
    internal interface IStateTaskHandler : ITaskHandler
    {
        State State { get; set; }
    }

    internal interface ISystemTaskHandler : IStateTaskHandler
    {
        ISystem AsSystem { get; }
    }

    internal interface ISystemTaskHandler<T> : ISystemTaskHandler
        where T : ISystem
    {
        T System { get; set; }
        ISystem ISystemTaskHandler.AsSystem { get => System; }
    }


    internal struct StateTickStartTaskHandler : IStateTaskHandler
    {
        public State State { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickStarted();
        }
    }

    internal struct StateTickFinishedTaskHandler : IStateTaskHandler
    {
        public State State { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.TickFinished();
        }
    }

    internal struct AttachToStateModuleTaskHandler : ISystemTaskHandler<IAttachToStateModule>
    {
        public State State { get; set; }
        public IAttachToStateModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnAttachToStateModule(State);
        }
    }

    internal struct DetachToStateModuleTaskHandler : ISystemTaskHandler<IDetachToStateModule>
    {
        public State State { get; set; }
        public IDetachToStateModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnDetachToStateModule(State);
        }
    }

    internal struct SystemTickStartTaskHandler : ISystemTaskHandler<ITickStartedModule>
    {
        public State State { get; set; }
        public ITickStartedModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnTickStarted(State);
        }
    }

    internal struct SystemTickFinishedTaskHandler : ISystemTaskHandler<ITickFinishedModule>
    {
        public State State { get; set; }
        public ITickFinishedModule System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnTickFinished(State);
        }
    }

    internal struct SystemCreateTaskHandler : ISystemTaskHandler<ICreateSystem>
    {
        public State State { get; set; }
        public ICreateSystem System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnCreate(State);
        }
    }

    internal struct SystemTickTaskHandler : ISystemTaskHandler<ITickSystem>
    {
        public State State { get; set; }
        public ITickSystem System { get; set; }

        public void Invoke()
        {
            System.OnTick(State);
        }
    }

    internal struct SystemDestroyTaskHandler : ISystemTaskHandler<IDestroySystem>
    {
        public State State { get; set; }
        public IDestroySystem System { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            System.OnDestroy(State);
        }
    }

    internal struct StateRevertToTaskHandler : IStateTaskHandler
    {
        public State State { get; set; }
        public uint tick;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            State.RevertTo(tick);
        }
    }

    internal struct ReceiversTaskHandler : IStateTaskHandler
    {
        public State State { get; set; }
        public Receivers receivers;
        public List<ITickEvent> events;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            receivers.Receive(State, events);
        }
    }

    public class RunTaskHandler : ITaskHandler
    {
        public object Data;
        public object Result;
        public Func<object, object> Handler;
        public Action<RunTaskHandler> Completed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke()
        {
            Result = Handler.Invoke(Data);
            Completed?.Invoke(this);
        }
    }
}