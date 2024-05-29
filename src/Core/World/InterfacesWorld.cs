using AnotherECS.Core.Processing;
using System;

namespace AnotherECS.Core
{
    public interface IWorldStatus
    {
        uint Id { get; }
        public uint CurrentTick { get; }
        public uint RequestTick { get; }
    }

    public interface IWorldLiveLoop : IWorldThreadLiveLoop
    {
        void Init();
        void Startup();
        void Tick(uint tickCount);
        void Destroy();
        void DispatchSignals();
    }

    public interface IWorldThreadLiveLoop : IDisposable
    {
        void UpdateFromMainThread();
        void Wait();
    }

    public interface IWorldModule
    {
        State State { get; set; }

        TModuleData GetModuleData<TModuleData>(uint id)
            where TModuleData : IModuleData;

        void SetModuleData<TModuleData>(uint id, TModuleData data)
            where TModuleData : IModuleData;
    }

    public interface IWorldExecute
    {
        void Run(RunTaskHandler runTaskHandler);
    }

    public interface IWorldCommunicate
    {
        void SendEvent(IEvent @event);
        void SendEvent(ITickEvent @event);
        void AddSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal;
        void RemoveSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal;
    }

    public interface IWorldInner : IWorldThreadLiveLoop, IWorldCommunicate, IWorldExecute
    {
        public IWorldExtend InnerWorld { get; }
    }

    public interface IWorldData
    {
        WorldData WorldData { get; set; }
    }

    public interface IWorldExtend : IWorldData, IWorldModule, IWorldStatus, IWorldLiveLoop, IWorldCommunicate, IWorldExecute { }

}