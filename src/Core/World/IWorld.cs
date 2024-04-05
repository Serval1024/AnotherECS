using System;

namespace AnotherECS.Core
{
    public interface IWorld : IDisposable
    {
        uint Id { get; }
        LiveState LiveState { get; }
        public uint CurrentTick { get; }
        public uint RequestTick { get; }

        void Init();
        void Startup();
        void Tick(uint tickCount);
        void Destroy();
        void UpdateFromMainThread();
    }

    public interface IWorldExtend : IWorld, IWorldCommunicate
    {
        State State { get; set; }

        public TModuleData GetModuleData<TModuleData>(uint id)
            where TModuleData : IModuleData;

        public void SetModuleData<TModuleData>(uint id, TModuleData data)
            where TModuleData : IModuleData;
    }

    public interface IWorldCommunicate
    {
        void SendEvent(IEvent @event);
        void SendEvent(ITickEvent @event);
    }
}