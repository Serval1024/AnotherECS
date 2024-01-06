using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using static AnotherECS.Core.Threading.ThreadRestrictionsBuilder;

namespace AnotherECS.Core.Threading
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class MultiThreadProcessing : ISystemProcessing
    {
        private readonly State _state;
        private readonly IThreadScheduler _threadScheduler;
        private readonly int _parallelMax;

        private Phase<SystemInvokeData<IConstructModule>, IConstructModule> _constructModule;
        private Phase<SystemInvokeData<ITickStartModule>, ITickStartModule> _tickStartModule;
        private Phase<SystemInvokeData<ITickFinishedModule>, ITickFinishedModule> _tickFinishedModule;

        private Phase<SystemInvokeData<IInitSystem>, IInitSystem> _init;
        private Phase<SystemInvokeData<ITickSystem>, ITickSystem> _tick;
        private Phase<SystemInvokeData<IDestroySystem>, IDestroySystem> _destroy;

        private Phase<ReceiverSystemInvokeData<IReceiverSystem>, IReceiverSystem> _receiver;

        public MultiThreadProcessing(State state, IThreadScheduler threadScheduler)
            : this(state, int.MaxValue, threadScheduler) { }

        public MultiThreadProcessing(State state, int parallelMax, IThreadScheduler threadScheduler)
        {
            _state = state;
            _threadScheduler = threadScheduler;
            _parallelMax = parallelMax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Prepare(IGroupSystem systemGroup)
        {
            var phaseArgs = new PhaseArgs(_state, systemGroup);

            _constructModule = CreatePhase<IConstructModule>(ref phaseArgs);
            _tickStartModule = CreatePhase<ITickStartModule>(ref phaseArgs);
            _tickFinishedModule = CreatePhase<ITickFinishedModule>(ref phaseArgs);

            _init = CreatePhase<IInitSystem>(ref phaseArgs);
            _tick = CreatePhase<ITickSystem>(ref phaseArgs);
            _destroy = CreatePhase<IDestroySystem>(ref phaseArgs);

            _receiver = CreatePhaseReceiver<IReceiverSystem>(ref phaseArgs);

            _threadScheduler.ParallelMax = IsSingleParallel() ? 1 : Math.Min(_parallelMax, GetParallelMax());

            phaseArgs.Dispose();
        }

        public void StateTickStart()
        {
            Run<StateTickStartedHandlerInvoke, StateInvokeData>(new StateInvokeData() { State = _state });
        }

        public void StateTickFinished()
        {
            Run<StateTickFinishedHandlerInvoke, StateInvokeData>(new StateInvokeData() { State = _state });
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
            Run<ReceiverSystemHandlerInvoke<IReceiverSystem>, ReceiverSystemInvokeData<IReceiverSystem>, IReceiverSystem>(ref _receiver);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            Run<RevertHandlerInvoke, RevertInvokeData>(new RevertInvokeData() { State = _state, tick = tick });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBusy()
            => _threadScheduler.IsBusy();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait()
        {
            _threadScheduler.Wait();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDeterministicSequence()
            => IsSingleParallel();

        public void CallFromMainThread()
        {
            _threadScheduler.CallFromMainThread();
        }

        public void Dispose()
        {
            _threadScheduler.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Run<TMethod, TData, TSystem>(ref Phase<TData, TSystem> phase)
            where TMethod : struct, ITaskHandler<TData>
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            for (uint i = 0; i < phase.heads.Length; ++i)
            {
                var head = phase.heads[i];
                _threadScheduler.Run<TMethod, TData>(phase.systems.AsSpan(head.index, head.count), head.relativeIndexMainThread);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Run<TMethod, TData>(TData data)
            where TMethod : struct, ITaskHandler<TData>
            where TData : struct
        {
            _threadScheduler.Run<TMethod, TData>(new ThreadArg<TData>() { arg = data });
        }

        private int GetParallelMax()
        {
            var parallelMax = new IPhase[] { _constructModule, _tickStartModule, _tickFinishedModule, _init, _tick, _destroy, _receiver }
                .Max(p => p.ParallelMax);

            return Math.Min(parallelMax, ThreadUtils.GetThreadCount());
        }

        private Phase<SystemInvokeData<TSystem>, TSystem> CreatePhase<TSystem>(ref PhaseArgs phaseArgs)
            where TSystem : ISystem
            => CreatePhase<SystemInvokeData<TSystem>, TSystem>(ref phaseArgs);

        private Phase<ReceiverSystemInvokeData<TSystem>, TSystem> CreatePhaseReceiver<TSystem>(ref PhaseArgs phaseArgs)
            where TSystem : ISystem, IReceiverSystem
        {
            var phase = CreatePhase<ReceiverSystemInvokeData<TSystem>, TSystem>(ref phaseArgs);
            for (int i = 0; i < phase.systems.Length; ++i)
            {
                phase.systems[i].arg.events = _state.GetEventCache();
                phase.systems[i].arg.eventContainers = ReflectionUtils.GetEventMap(phase.systems[i].arg.System);
            }
            return phase;
        }

        private Phase<TData, TSystem> CreatePhase<TData, TSystem>(ref PhaseArgs phaseArgs)
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            var result = new Context<TData, TSystem>(_state, ref phaseArgs);
            CollectsPhase(phaseArgs.systemGroup.OfType<TSystem>(), ref result);
            var phase = result.ToPhase();
            return phase;
        }

        private void CollectsPhase<TData, TSystem>(IEnumerable<TSystem> systems, ref Context<TData, TSystem> context)
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            context.Push(systems);

            foreach (var system in systems)
            {
                var st = GetSystemType<TSystem>(system);
                switch (st)
                {
                    case SystemType.Sync:
                        {
                            context.FlushASync();
                            context.AddSync(system);
                            break;
                        }
                    case SystemType.ASync:
                        {
                            context.PushASyncCandidate((IAsyncThread)system);
                            break;
                        }
                    case SystemType.Collection:
                        {
                            CollectsPhase((IEnumerable<TSystem>)system, ref context);
                            break;
                        }
                }
            }

            context.FlushASync();
        }

        private SystemType GetSystemType<TSystem>(ISystem system)
        {
            if (system is IEnumerable<TSystem>)
            {
                return SystemType.Collection;
            }

            return !IsSingleParallel() && (system is IAsyncThread)
                ? SystemType.ASync
                : SystemType.Sync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSingleParallel()
            => _parallelMax == 1;


        private struct Context<TData, TSystem>
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            private readonly State _state;
            private readonly PhaseArgs _phaseArgs;

            private readonly List<Head> _heads;
            private readonly Dictionary<Type, PlaceSystemDetail> _systems;
            private readonly List<IAsyncThread> _asyncBuffer;
            private int _parallelMax;

            public readonly int ParallelMax => _parallelMax;

            public Context(State state, ref PhaseArgs phaseArgs)
            {
                _state = state;
                _phaseArgs = phaseArgs;

                _heads = new List<Head>();
                _systems = new Dictionary<Type, PlaceSystemDetail>();
                _asyncBuffer = new List<IAsyncThread>();
                _parallelMax = 1;
            }

            public void Push(IEnumerable<TSystem> systems)
            {
                foreach (var system in systems)
                {
                    if (system is not IEnumerable<TSystem>)
                    {
                        Push(system);
                    }
                }
            }

            public void Push(TSystem system)
            {
                var place = new PlaceSystemDetail()
                {
                    index = _systems.Count,
                    detail = _phaseArgs.systemDetails[system.GetType()],
                };

                _systems.Add(system.GetType(), place);
            }

            public void AddSync(TSystem system)
            {
                _heads.Add(new Head() { index = _systems[system.GetType()].index, count = 1 });
            }

            public void PushASyncCandidate(IAsyncThread system)
            {
                _asyncBuffer.Add(system);
            }

            public unsafe void FlushASync()
            {
                if (_asyncBuffer.Count != 0)
                {
                    using var restrictions = new ThreadRestrictions(&_state.GetGlobalDependencies()->bAllocator);

                    var system = _systems[_asyncBuffer[0].GetType()];
                    _heads.Add(new Head() { index = system.index, count = 1 });
                    restrictions.Add(system.detail.restrictions);

                    for (int i = 1; i < _asyncBuffer.Count; ++i)
                    {
                        system = _systems[_asyncBuffer[i].GetType()];

                        if (restrictions.IsCollision(system.detail.restrictions))
                        {
                            restrictions.Clear();
                            restrictions.Add(system.detail.restrictions);
                            _heads.Add(new Head() { index = system.index, count = 1 });
                        }
                        else
                        {
                            var index = _heads.Count - 1;
                            var head = _heads[index];
                            ++head.count;
                            _heads[index] = head;

                            if (_parallelMax < head.count)
                            {
                                _parallelMax = head.count;
                            }

                            restrictions.Add(system.detail.restrictions);
                        }
                    }
                    
                    _asyncBuffer.Clear();
                }
            }

            public Phase<TData, TSystem> ToPhase()
            {
                var systems = new ThreadArg<TSystem>[_systems.Count];
                foreach (var system in _systems)
                {
                    systems[system.Value.index].arg = (TSystem)system.Value.detail.system;
                    systems[system.Value.index].isMainThread = system.Value.detail.isMainThread;
                }
                return new Phase<TData, TSystem>(_state, _heads.ToArray(), systems, _parallelMax);
            }


            private struct PlaceSystemDetail
            {
                public int index;
                public SystemDetail detail;

                public PlaceSystemDetail(int index, SystemDetail detail)
                {
                    this.index = index;
                    this.detail = detail;
                }
            }
        }

        private struct Phase<TData, TSystem> : IPhase
            where TData : struct, ISystemInvokeData<TSystem>
            where TSystem : ISystem
        {
            public Head[] heads;
            public ThreadArg<TData>[] systems;
            public int parallelMax;

            public readonly int ParallelMax => parallelMax;

            public Phase(State state, Head[] heads, ThreadArg<TSystem>[] systems, int parallelMax)
            {
                this.heads = heads;

                this.systems = new ThreadArg<TData>[systems.Length];
                for (int i = 0; i < systems.Length; ++i)
                {
                    this.systems[i] = new ThreadArg<TData>()
                    {
                        arg = new TData() { State = state, System = systems[i].arg },
                        isMainThread = systems[i].isMainThread
                    };
                }
                this.parallelMax = parallelMax;

                Init();
            }

            private void Init()
            {
                for (int i = 0; i < heads.Length; ++i)
                {
                    var index = heads[i].index;
                    var span = systems.AsSpan(index, heads[i].count);
                    span.Sort();

                    for (int j = 0; j < span.Length; ++j)
                    {
                        if (span[j].isMainThread)
                        {
                            heads[i].relativeIndexMainThread = j;
                            break;
                        }
                        else if (j == span.Length - 1)
                        {
                            heads[i].relativeIndexMainThread = span.Length;
                        }
                    }
                }
            }
        }

        private interface IPhase
        {
            int ParallelMax { get; }
        }

        private struct Head
        {
            public int index;
            public int count;
            public int relativeIndexMainThread;
        }

        private struct SystemDetail : IDisposable
        {
            public ISystem system;
            public bool isMainThread;
            public ThreadRestrictions restrictions;

            public SystemDetail(ISystem system, bool isMainThread, ThreadRestrictions restrictions)
            {
                this.system = system;
                this.isMainThread = isMainThread;
                this.restrictions = restrictions;
            }

            public void Dispose()
            {
                restrictions.Dispose();
            }
        }

        private struct PhaseArgs : IDisposable
        {
            public IEnumerable<ISystem> systemGroup;
            public Dictionary<Type, SystemDetail> systemDetails;

            public PhaseArgs(State state, IGroupSystem systemGroup)
            {
                this.systemGroup = systemGroup;
                
                systemDetails = systemGroup
                    .GetSystemsAll()
                    .ToDictionary(k => k.GetType(), v => new SystemDetail()
                    {
                        system = v,
                        isMainThread = IsMainTread(v),
                        restrictions = GetRestrictions(v, state),
                    });
            }

            private static ThreadRestrictions GetRestrictions(ISystem system, State state)
            {
                if (system is IAsyncThread asyncThread)
                {
                    var builder = new ThreadRestrictionsBuilder(state);
                    asyncThread.Restrictions(ref builder);
                    return builder.Build();
                }
                return default;
            }

            private static bool IsMainTread(ISystem system)
                => system is IMainThread;

            public void Dispose()
            {
                foreach(var system in systemDetails)
                {
                    system.Value.Dispose();
                }
            }
        }

        private enum SystemType
        {
            Collection,
            Sync,
            ASync,
        }
    }
}