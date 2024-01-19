using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Threading;

namespace AnotherECS.Core.Processing
{
    internal sealed class OneThreadProcessing<TThreadScheduler> : ISystemProcessing
        where TThreadScheduler : struct, IThreadScheduler<Task>
    {
        private readonly State _state;
        private readonly TThreadScheduler _threadScheduler;

        private Task _stateTickStart;
        private Task _stateTickFinished;

        private StateRevertToTaskHandler _stateRevertToTaskHandler;
        private Task _stateRevertTo;

        private Task[] _constructSystems;
        private Task[] _tickStartSystems;
        private Task[] _tickFinishedSystems;

        private Task[] _initSystems;
        private Task[] _tickSystems;
        private Task[] _destroySystems;

        private Task _receivers;

        public OneThreadProcessing(State state, TThreadScheduler threadScheduler)
        {
            _state = state;
            _threadScheduler = threadScheduler;
        }

        public void Prepare(IGroupSystem systemGroup)
        {
            var systems = systemGroup.GetSystemsAll().ToArray();

            _stateTickStart = new Task(new StateTickStartTaskHandler() { State = _state }, false);
            _stateTickFinished = new Task(new StateTickFinishedTaskHandler() { State = _state }, false);

            _stateRevertToTaskHandler = new StateRevertToTaskHandler() { State = _state };
            _stateRevertTo = new Task(_stateRevertToTaskHandler, false);

            _constructSystems = CreateTasks<ConstructTaskHandler, IConstructModule>(systems);
            _tickStartSystems = CreateTasks<SystemTickStartTaskHandler, ITickStartModule>(systems);
            _tickFinishedSystems = CreateTasks<SystemTickFinishedTaskHandler, ITickFinishedModule>(systems);

            _initSystems = CreateTasks<SystemInitTaskHandler, IInitSystem>(systems);
            _tickSystems = CreateTasks<SystemTickTaskHandler, ITickSystem>(systems);
            _destroySystems = CreateTasks<SystemDestroyTaskHandler, IDestroySystem>(systems);

            _receivers = new Task(new ReceiversTaskHandler()
            {
                State = _state,
                receivers = ProcessingUtils.ToReceivers(Filter<IReceiverSystem>(systems)),
                events = _state.GetEventCache(),
            }, false);
        }

        public void StateTickStart()
        {
            _threadScheduler.Run(_stateTickStart);
        }


        public void StateTickFinished()
        {
            _threadScheduler.Run(_stateTickFinished);
        }

        public void Construct()
        {
            _threadScheduler.Run(_constructSystems);
        }

        public void TickStart()
        {
            _threadScheduler.Run(_tickStartSystems);
        }

        public void TickFinished()
        {
            _threadScheduler.Run(_tickFinishedSystems);
        }

        public void Init()
        {
            _threadScheduler.Run(_initSystems.AsSpan());
        }

        public void Tick()
        {
            _threadScheduler.Run(_tickSystems.AsSpan());
        }

        public void Destroy()
        {
            _threadScheduler.Run(_destroySystems.AsSpan());
        }

        public void Receive()
        {
            _threadScheduler.Run(_receivers);
        }

        public void RevertTo(uint tick)
        {
            _stateRevertToTaskHandler.tick = tick;
            _stateRevertTo.handler = _stateRevertToTaskHandler;
            _threadScheduler.Run(_stateRevertTo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _threadScheduler.IsBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInWork()
            => _threadScheduler.GetInWork();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetThreadMax()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWorkingThreadCount()
            => 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _threadScheduler.Complete();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
            => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetParallelMax()
            => (uint)_threadScheduler.ParallelMax;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CallFromMainThread()
        {
            _threadScheduler.CallFromMainThread();
        }

        public void Dispose()
        {
            _threadScheduler.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickFullLoop()
        {
            StateTickStart();
            TickStart();
            Receive();
            Tick();
            TickFinished();
            StateTickFinished();
        }

        private Task[] CreateTasks<THandler, TSystem>(ISystem[] systems)
            where THandler : struct, ITaskHandler, ISystemTaskHandler<TSystem>
            where TSystem : ISystem
            => Filter<TSystem>(systems)
            .Select(p => new Task(new THandler() { State = _state, System = p }, IsMainTread(p)))
            .ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMainTread(ISystem system)
                => system is IMainThread;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TType> Filter<TType>(ISystem[] systems)
            => systems.OfType<TType>();
    }
}

