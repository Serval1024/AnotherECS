using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Threading
{
    public class MainThreadProcessing : ISystemProcessing
    {
        private readonly State _state;

        private SystemInvokeData<IConstructModule>[] _constructModule;
        private SystemInvokeData<ITickStartModule>[] _tickStartModule;
        private SystemInvokeData<ITickFinishedModule>[] _tickFinishedModule;

        private SystemInvokeData<IInitSystem>[] _init;
        private SystemInvokeData<ITickSystem>[] _tick;
        private SystemInvokeData<IDestroySystem>[] _destroy;

        private Dictionary<Type, List<IEventInvoke>> _receivers;

        public MainThreadProcessing(State state)
        {
            _state = state;
        }

        public void Prepare(IGroupSystem systemGroup)
        {
            var phaseArgs = new PhaseArgs() { systems = systemGroup.GetSystemsAll().ToArray() };

            _constructModule = CreatePhase<IConstructModule>(ref phaseArgs);
            _tickStartModule = CreatePhase<ITickStartModule>(ref phaseArgs);
            _tickFinishedModule = CreatePhase<ITickFinishedModule>(ref phaseArgs);

            _init = CreatePhase<IInitSystem>(ref phaseArgs);
            _tick = CreatePhase<ITickSystem>(ref phaseArgs);
            _destroy = CreatePhase<IDestroySystem>(ref phaseArgs);

            _receivers = CollectReceiverSystems(ref phaseArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StateTickStart()
        {
            _state.TickStarted();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StateTickFinished()
        {
            _state.TickFinished();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Construct()
        {
            Run<ConstructSystemHandlerInvoke<IConstructModule>, SystemInvokeData<IConstructModule>, IConstructModule>(ref _constructModule);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStart()
        {
            Run<TickStartSystemHandlerInvoke<ITickStartModule>, SystemInvokeData<ITickStartModule>, ITickStartModule>(ref _tickStartModule);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFinished()
        {
            Run<TickFinishedSystemHandlerInvoke<ITickFinishedModule>, SystemInvokeData<ITickFinishedModule>, ITickFinishedModule>(ref _tickFinishedModule);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init()
        {
            Run<InitSystemHandlerInvoke<IInitSystem>, SystemInvokeData<IInitSystem>, IInitSystem>(ref _init);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick()
        {
            Run<TickSystemHandlerInvoke<ITickSystem>, SystemInvokeData<ITickSystem>, ITickSystem>(ref _tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy()
        {
            Run<DestroySystemHandlerInvoke<IDestroySystem>, SystemInvokeData<IDestroySystem>, IDestroySystem>(ref _destroy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Receive()
        {
            Receive(_state.GetEventCache());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
          => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetParallelMax()
            => 1u;

        public void RevertTo(uint tick)
        {
            _state.RevertTo(tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread() { }

        public void TickFullLoop()
        {
            StateTickStart();
            TickStart();
            Receive();
            Tick();
            TickFinished();
            StateTickFinished();
        }

        public void Dispose() { }


        private void Run<TMethod, TData, TSystem>(ref TData[] phase)
            where TMethod : struct, ITaskHandler<TData>
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            for(int i = 0; i < phase.Length; ++i)
            {
                default(TMethod).Invoke(ref phase[i]);
            }
        }

        private Dictionary<Type, List<IEventInvoke>> CollectReceiverSystems(ref PhaseArgs phaseArgs)
        {
            var receivers = new Dictionary<Type, List<IEventInvoke>>();

            foreach (var system in phaseArgs.systems.OfType<IReceiverSystem>())
            {
                foreach (var element in ReflectionUtils.GetEventMap(system))
                {
                    if (receivers.TryGetValue(element.Key, out List<IEventInvoke> list))
                    {
                        list.Add(element.Value);
                    }
                    else
                    {
                        receivers.Add(element.Key, new List<IEventInvoke>() { element.Value });
                    }
                }
            }

            return receivers;
        }

        private void Receive(List<ITickEvent> events)
        {
            for (int i = 0; i < events.Count; ++i)
            {
                Receive(events[i].Value);
            }
        }

        private void Receive(BaseEvent @event)
        {
            var value = @event.GetType();
            while (value != null)
            {
                if (_receivers.TryGetValue(value, out var systems))
                {
                    for (int j = 0; j < systems.Count; ++j)
                    {
                        systems[j].Invoke(_state, @event);
                    }
                }
                value = value.BaseType;
            }
        } 


        private SystemInvokeData<TSystem>[] CreatePhase<TSystem>(ref PhaseArgs phaseArgs)
            where TSystem : ISystem
            => phaseArgs.systems.OfType<TSystem>()
            .Select(p => new SystemInvokeData<TSystem>() { State = _state, System = p })
            .ToArray();

        private struct PhaseArgs
        {
            public ISystem[] systems;
        }
    }
}
