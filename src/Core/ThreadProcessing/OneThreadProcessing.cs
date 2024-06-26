﻿using AnotherECS.Core.Collection;
using AnotherECS.Core.Remote;
using AnotherECS.Core.Threading;
using AnotherECS.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Processing
{
    internal sealed class OneThreadProcessing<TThreadScheduler> : ISystemProcessing
        where TThreadScheduler : struct, IThreadScheduler<Task>
    {
        private State _state;
        private TThreadScheduler _threadScheduler;

        private Task _stateStartup;

        private Task _stateTickStart;
        private Task _stateTickFinished;

        private StateRevertToTaskHandler _stateRevertToTaskHandler;
        private Task _stateRevertTo;

        private Task _revertFinishedTaskHandler;

        private Task[] _attachToStateModuleSystems;
        private Task[] _detachToStateModuleSystems;
        private Task[] _tickStartedSystems;
        private Task[] _tickFinishedSystems;

        private Task[] _createSystems;
        private Task[] _tickSystems;
        private Task[] _destroySystems;

        private Task _receivers;

        public OneThreadProcessing(TThreadScheduler threadScheduler)
        {
            _threadScheduler = threadScheduler;
        }

        public void SetStatistic(ITimerStatistic timerStatistic)
        {
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
            _threadScheduler.Statistic = timerStatistic;
#endif
        }

        public void Prepare(State state, IEnumerable<ISystem> flatSystems)
        {
            var systems = flatSystems.ToArray();
            _state = state;

            _stateStartup = CreateTask(new StateStartupTaskHandler() { State = _state });

            _stateTickStart = CreateTask(new StateTickStartTaskHandler() { State = _state });
            _stateTickFinished = CreateTask(new StateTickFinishedTaskHandler() { State = _state });

            _stateRevertToTaskHandler = new StateRevertToTaskHandler() { State = _state };
            _stateRevertTo = CreateTask(_stateRevertToTaskHandler);

            _revertFinishedTaskHandler = CreateTask(new RevertFinishedTaskHandler() { State = _state });

            _attachToStateModuleSystems = CreateTasks<AttachToStateModuleTaskHandler, IAttachToStateModule>(systems);
            _detachToStateModuleSystems = CreateTasks<DetachToStateModuleTaskHandler, IDetachToStateModule>(systems);

            _tickStartedSystems = CreateTasks<SystemTickStartTaskHandler, ITickStartedModule>(systems);
            _tickFinishedSystems = CreateTasks<SystemTickFinishedTaskHandler, ITickFinishedModule>(systems);

            _createSystems = CreateTasks<SystemCreateTaskHandler, ICreateSystem>(systems);
            _tickSystems = CreateTasks<SystemTickTaskHandler, ITickSystem>(systems);
            _destroySystems = CreateTasks<SystemDestroyTaskHandler, IDestroySystem>(systems);

            _receivers = CreateTask(new ReceiversTaskHandler()
            {
                State = _state,
                receivers = ProcessingUtils.ToReceivers(Filter<IReceiverSystem>(systems)),
                events = _state?.GetEventCache(),
            });
        }

        public void StateStartup()
        {
            _threadScheduler.Run(_stateStartup);
        }

        public void StateTickStart()
        {
            _threadScheduler.Run(_stateTickStart);
        }

        public void StateTickFinished()
        {
            _threadScheduler.Run(_stateTickFinished);
        }

        public void AttachToStateModule()
        {
            _threadScheduler.Run(_attachToStateModuleSystems);
        }

        public void DetachToStateModule()
        {
            _threadScheduler.Run(_detachToStateModuleSystems);
        }

        public void TickStart()
        {
            _threadScheduler.Run(_tickStartedSystems);
        }

        public void TickFinished()
        {
            _threadScheduler.Run(_tickFinishedSystems);
        }

        public void Create()
        {
            _threadScheduler.Run(_createSystems.AsSpan());
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

        public void RevertFinished()
        {
            _threadScheduler.Run(_revertFinishedTaskHandler);
        }

        public void Run(RunTaskHandler runTaskHandler)
        {
            _threadScheduler.Run(CreateTask(runTaskHandler));
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

        public void Clear()
        {
            _threadScheduler.Clear();
        }

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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task[] CreateTasks<THandler, TSystem>(ISystem[] systems)
            where THandler : struct, ITaskHandler, ISystemTaskHandler<TSystem>
            where TSystem : ISystem
            => Filter<TSystem>(systems)
                .Select(p => CreateTask<THandler, TSystem>(p))
                .ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task CreateTask<THandler>(THandler handler)
            where THandler : ITaskHandler
            => new(
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
                typeof(THandler).Name,
#endif
                handler,
                false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task CreateTask<THandler, TSystem>(TSystem system)
            where THandler : struct, ITaskHandler, ISystemTaskHandler<TSystem>
            where TSystem : ISystem
            => new(
#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
                GetTaskId(system.GetType(), typeof(TSystem)),
#endif
                new THandler() { State = _state, System = system },
                IsMainTread(system)
                );

#if !ANOTHERECS_RELEASE || ANOTHERECS_STATISTIC
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetTaskId(Type obj, Type phase)
            => $"{obj.Name}:{phase.Name}";
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMainTread(ISystem system)
                => system is IMainThread;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TType> Filter<TType>(ISystem[] systems)
            => systems
                .Where(p => typeof(TType).IsAssignableFrom(p.GetType()))
                .Cast<TType>();
    }
}

