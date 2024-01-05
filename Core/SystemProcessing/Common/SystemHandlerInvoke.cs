using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public interface ISystemInvokeData<T>
    {
        T System { set; get; }
        State State { set; get; }
    }

    public interface ITaskHandler<TData>
        where TData : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Invoke(ref TData data);
    }

    public struct SystemInvokeData<T> : ISystemInvokeData<T>
    {
        public T System { set; get; }
        public State State { set; get; }
    }

    public struct ReceiverSystemInvokeData<T> : ISystemInvokeData<T>
    {
        public T System { set; get; }
        public State State { set; get; }

        public Dictionary<Type, IEventInvoke> eventContainers;
        public List<ITickEvent> events;
    }

    public struct InitSystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : IInitSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.Init(data.State);
        }
    }

    public struct TickSystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : ITickSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.Tick(data.State);
        }
    }

    public struct DestroySystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : IDestroySystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.Destroy(data.State);
        }
    }

    public struct ConstructSystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : IConstructModule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.Construct(data.State);
        }
    }

    public struct TickStartSystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : ITickStartModule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.TickStarted(data.State);
        }
    }

    public struct TickFinishedSystemHandlerInvoke<T> : ITaskHandler<SystemInvokeData<T>>
        where T : ITickFinishedModule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref SystemInvokeData<T> data)
        {
            data.System.TickFinished(data.State);
        }
    }

    public struct ReceiverSystemHandlerInvoke<T> : ITaskHandler<ReceiverSystemInvokeData<T>>
        where T : IReceiverSystem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ref ReceiverSystemInvokeData<T> data)
        {
            var eventContainers = data.eventContainers;
            var events = data.events;

            for (int i = 0; i < events.Count; ++i)
            {
                if (eventContainers.TryGetValue(events[i].Value.GetType(), out IEventInvoke eventInvoker))
                {
                    eventInvoker.Invoke(data.State, events[i].Value);
                }
            }
            
        }
    }
}