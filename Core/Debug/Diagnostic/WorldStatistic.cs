using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AnotherECS.Core;
using AnotherECS.Core.Collection;
using AnotherECS.Core.Processing;

namespace AnotherECS.Debug.Diagnostic
{
    internal struct WorldStatistic : IWorldStatistic
    {
        private const int TIMER_BUFFER_CAPACITY = 8;

        private World _world;
        private List<SystemData> _systemByDeepGroup;
        private Dictionary<string, Timer> _timers;
        private object _lock;

        void IWorldStatistic.Construct(World world)
        {
            _world = world;
            _systemByDeepGroup = new List<SystemData>();
            _timers = new Dictionary<string, Timer>();
            _lock = new object();
        }

        void IWorldStatistic.UpdateSystemGraph(IGroupSystem systems)
        {
            _systemByDeepGroup.Clear();
            var group = 0;
            UpdateSystemGraph(systems, ref group, 0);
        }

        void ITimerStatistic.StartTimer(string tag)
        {
            if (tag != null)
            {
                lock (_lock)
                {
                    if (_timers.TryGetValue(tag, out var timer))
                    {
                        timer.Restart();
                    }
                    else
                    {
                        timer = Timer.Create(TIMER_BUFFER_CAPACITY);
                        _timers.Add(tag, timer);
                        timer.Start();
                    }
                }
            }
        }

        void ITimerStatistic.StopTimer(string tag)
        {
            if (tag != null)
            {
                lock (_lock)
                {
                    if (_timers.TryGetValue(tag, out var timer))
                    {
                        timer.Stop();
                        _timers[tag] = timer;
                    }
                }
            }
        }

        WorldStatisticData IWorldStatisticProvider.GetStatistic()
            => new()
            {
                worldName = _world.Name,
                entityCount = _world.GetState().EntityCount,
                componentTotal = GetTotalComponents(_world),
                memoryTotal = _world.GetState().GetMemoryTotal(),
                historyMemoryTotal = _world.GetState().GetHistoryMemoryTotal(),

                createSystems = CreateSystemsStatisticData<ICreateSystem>(_systemByDeepGroup, _timers),
                tickSystems = CreateSystemsStatisticData<ITickSystem>(_systemByDeepGroup, _timers),
                destroySystems = CreateSystemsStatisticData<IDestroySystem>(_systemByDeepGroup, _timers),

                createModule = CreateSystemsStatisticData<ICreateModule>(_systemByDeepGroup, _timers),
                tickStartedModule = CreateSystemsStatisticData<ITickStartedModule>(_systemByDeepGroup, _timers),
                tickFinishedModule = CreateSystemsStatisticData<ITickFinishedModule>(_systemByDeepGroup, _timers),

                stateTickStart = CreateTaskStatisticData(nameof(StateTickStartTaskHandler), _timers),
                stateTickFinished = CreateTaskStatisticData(nameof(StateTickFinishedTaskHandler), _timers),
                stateRevertTo = CreateTaskStatisticData(nameof(StateRevertToTaskHandler), _timers),
            };

        private static SystemsStatisticData CreateSystemsStatisticData<TPhase>(List<SystemData> systemByDeepGroup, Dictionary<string, Timer> timers)
            => new()
            {
                systems = systemByDeepGroup
                    .Where(p => typeof(TPhase).IsAssignableFrom(p.system.GetType()))
                    .Select(p =>
                        new SystemStatisticData()
                        {
                            group = p.group,
                            deep = p.deep,
                            name = p.name,
                            timer = GetTimer(timers, $"{p.system.GetType().Name}:{typeof(TPhase).Name}"),
                        })
                    .ToArray(),
            };

        TaskStatisticData CreateTaskStatisticData(string name, Dictionary<string, Timer> timers)
           => new()
           {
                name = name,
                timer = GetTimer(timers, name),
           };

        private static Core.Timer GetTimer(Dictionary<string, Timer> timers, string target)
            => timers.TryGetValue(target, out var timer)
                ? new Core.Timer() { times = timer.buffer.ToArray() }
                : default;
        
        private static uint GetTotalComponents(World world)
        {
            uint totalComponent = 0;
            foreach (var id in world.GetState().CollectAllEntityIds())
            {
                totalComponent += world.GetState().GetCount(id);
            }

            return totalComponent;
        }


        private void UpdateSystemGraph(IEnumerable<ISystem> systems, ref int group, int deep)
        {
            foreach (var system in systems)
            {
                if (system is IEnumerable<ISystem> enumerable)
                {
                    ++group;
                    UpdateSystemGraph(enumerable, ref group, deep + 1);
                    --group;
                }
                else
                {
                    _systemByDeepGroup.Add(new()
                    {
                        group = group,
                        deep = deep,
                        system = system,
                        name = system.GetType().Name
                    });
                }
            }
        }
    }

    internal struct Timer
    {
        public RingBuffer<Time> buffer;
        public Stopwatch stopwatch;

        public static Timer Create(int capacity)
            => new()
            {
                buffer = new RingBuffer<Time>(capacity),
                stopwatch = new Stopwatch(),
            };

        public void Start()
        {
            stopwatch.Start();
        }

        public void Stop()
        {
            stopwatch.Stop();
            buffer.Push(new Time()
            {
                elapsedTicks = stopwatch.ElapsedTicks,
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            });
        }

        public void Restart()
        {
            stopwatch.Restart();
        }
    }

    internal struct SystemData
    {
        public int group;
        public int deep;
        public object system;
        public string name;
    }
}
