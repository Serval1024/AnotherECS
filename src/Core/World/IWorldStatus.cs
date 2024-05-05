using AnotherECS.Core.Processing;
using System;

namespace AnotherECS.Core
{
    public interface IWorldStatus : IDisposable
    {
        uint Id { get; }
        LiveState LiveState { get; }
        public uint CurrentTick { get; }
        public uint RequestTick { get; }
    }

    public interface IWorldLiveLoop : IDisposable
    {
        void Init();
        void Startup();
        void Tick(uint tickCount);
        void Destroy();
        void DispatchSignals();
        void UpdateFromMainThread();
    }

    public interface IWorldExtend : IWorldStatus, IWorldLiveLoop, IWorldCommunicate, IWorldExecute
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

    public interface IWorldComposite : IWorldCommunicate, IWorldExecute
    {
        State State { get; set; }
        IWorldExtend InnerWorld { get; set; }
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
}