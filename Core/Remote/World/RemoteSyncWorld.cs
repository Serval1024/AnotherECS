using System;

namespace AnotherECS.Core.Remote
{
    public class RemoteSyncWorld : IWorld, IDisposable
    {
        public uint Id => _world.Id;

        private readonly IWorldExtend _world;
        private readonly IRemoteEventProvider _removeProvider;

        
        public RemoteSyncWorld(IWorldExtend world, IHubBytesProvider bytesDataProvider)
            : this(world, new RemoteEventProvider(bytesDataProvider.Get(world.Id))) { }

        public RemoteSyncWorld(IWorldExtend world, IRemoteProvider bytesDataProvider)
            : this(world, new RemoteEventProvider(bytesDataProvider)) { }

        public RemoteSyncWorld(IWorldExtend world, IRemoteEventProvider removeProvider)
        {
            _world = world;
            _removeProvider = removeProvider;

            _removeProvider.ReceiveEvent += OnReceiveEvent;
            _removeProvider.ReceiveState += OnReceiveState;
        }

        public void Init()
        {
            _world.Init();
        }

        public void Tick()
        {
            _world.Tick(1u);
        }

        public void Tick(uint tickCount)
        {
            _world.Tick(tickCount);
        }

        public void Destroy()
        {
            _world.Destroy();
        }

        public void UpdateFromMainThread()
        {
            _world.UpdateFromMainThread();
        }

        public void SendEvent(IEvent @event)
        {
            SendEvent(_world.ToITickEvent(@event));
        }

        public void SendEvent(ITickEvent @event)
        {
            _removeProvider.SendOtherEvent(@event);
            _world.SendEvent(@event);
        }

        public void Dispose()
        {
            _removeProvider.ReceiveEvent -= OnReceiveEvent;
            _removeProvider.ReceiveState -= OnReceiveState;

            _removeProvider.Dispose();
            _world.Dispose();
        }

        private void OnReceiveEvent(ITickEvent @event)
        {
            _world.SendEvent(@event);
        }

        private void OnReceiveState(State data)
        {
            _world.SetState(data);
        }
    }
}