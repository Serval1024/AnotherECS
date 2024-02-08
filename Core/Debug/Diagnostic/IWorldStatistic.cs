namespace AnotherECS.Core
{
    public interface IWorldStatisticProvider
    {
        WorldStatisticData GetStatistic();
    }

    internal interface IPhaseTimerStatistic
    {
        void StartTimer<TPhase>(string tag);
        void StopTimer<TPhase>(string tag);
    }

    public interface ITimerStatistic
    {
        void StartTimer(string tag);
        void StopTimer(string tag);
    }

    internal interface IWorldStatistic : IWorldStatisticProvider, ITimerStatistic
    {
        void Construct(World world);
        void UpdateSystemGraph(IGroupSystem systems);
    }


    public struct WorldStatisticData
    {
        public string worldName;
        public uint entityCount;
        public uint componentTotal;
        public ulong memoryTotal;
        public ulong historyMemoryTotal;

        public SystemsStatisticData createSystems;
        public SystemsStatisticData tickSystems;
        public SystemsStatisticData destroySystems;

        public SystemsStatisticData createModule;
        public SystemsStatisticData tickStartedModule;
        public SystemsStatisticData tickFinishedModule;

        public TaskStatisticData stateTickStart;
        public TaskStatisticData stateTickFinished;
        public TaskStatisticData stateRevertTo;
    }

    public struct SystemsStatisticData
    {
        public SystemStatisticData[] systems;
    }

    public struct SystemStatisticData
    {
        public int group;
        public int deep;
        public string name;
        public Timer timer;
    }

    public struct TaskStatisticData
    {
        public string name;
        public Timer timer;
    }

    public struct Timer
    {
        public Time[] times;

        public readonly Time GetAverage()
        {
            if (times != null && times.Length != 0)
            {
                Time result = default;
                for (int i = 0; i < times.Length; i++)
                {
                    result.elapsedTicks += times[i].elapsedTicks;
                    result.elapsedMilliseconds += times[i].elapsedMilliseconds;
                }
                result.elapsedTicks /= times.Length;
                result.elapsedMilliseconds /= times.Length;

                return result;
            }

            return default;
        }
    }

    public struct Time
    {
        public long elapsedTicks;
        public long elapsedMilliseconds;

        public string ToStringDisplay()
            => (elapsedMilliseconds < 1)
                ? (elapsedTicks / 10000f).ToString("0.00")
                : elapsedMilliseconds.ToString();
    }

}