using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public class RemoteWorld : IWorldCommunicate, IDisposable
    {
        public uint Id => _world.Id;
        public IWorldExtend LocalWorld => _world;
        public IRemoteProcessing Remote => _removeProvider;

        public double Time => _removeProvider.GetGlobalTime();

        private double _deltaTime = 1.0 / 20.0;
        public double DeltaTime
        {
            get => _deltaTime;
            set
            {
                if (_deltaTime <= 0)
                {
                    throw new ArgumentException($"{nameof(DeltaTime)} must be more than 0.");
                }
                _deltaTime = value;
            }
        }


        public State State
        {
            get => _world.State;
            set
            {
                SetState(value);
            }
        }

        private readonly IWorldExtend _world;
        private readonly IRemoteProcessing _removeProvider;

        private RemoveWorldModuleData _threadDoubleBuffer;

        public RemoteWorld(IWorldExtend world, IRemoteProvider remoteProvider)
            : this(world, new RemoteProcessing(remoteProvider)) { }

        public RemoteWorld(IWorldExtend world, IRemoteProcessing removeProvider)
        {            
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _removeProvider = removeProvider ?? throw new ArgumentNullException(nameof(removeProvider));

            _threadDoubleBuffer = new RemoveWorldModuleData();

            _removeProvider.Construct(_world);

            if (world.LiveState == LiveState.Raw)
            {
                _world.Init();
            }

            if (_world.State != null)
            {
                SetState(_world.State);
            }
        }

        public Task<ConnectResult> Connect()
            => _removeProvider.Connect();

        public Task Disconnect()
            => _removeProvider.Disconnect();

        public void UpdateFromMainThread()
        {
            if (State != null)
            {
                switch (_world.LiveState)
                {
                    case LiveState.Startup:
                        {
                            UpdateModuleData();
                            var target = (uint)(Time / DeltaTime);
                            var delta = target - _world.RequestTick;

                            UnityEngine.Debug.Log(Time + " : " + delta);
                            if (delta > 0)
                            {
                                _world.Tick(delta);
                            }
                            _world.UpdateFromMainThread();
                            break;
                        }
                    case LiveState.Inited:
                        {
                            UpdateModuleData();
                            _world.Startup();
                            break;
                        }
                }
            }
        }

        public void SendEvent(IEvent @event)
        {
            SendEvent(ToITickEvent(@event));
        }

        public void SendEvent(ITickEvent @event)
        {
            _removeProvider.SendOtherEvent(@event);
            _world.SendEvent(@event);
        }

        public async void DestroyAndDispose()
        {
            Destroy();
            await Disconnect();
            Dispose();
        }

        public void Dispose()
        {
            _removeProvider.Dispose();
            _world.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Destroy()
        {
            if (_world.LiveState == LiveState.Startup)
            {
                _world.Destroy();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private EventContainer ToITickEvent(IEvent @event)
            => new(_world.State.Tick + 1, @event);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetState(State value)
        {
            _world.State = value;
            if (value != null)
            {
                _world.SetModuleData(RemoveWorldModuleData.MODULE_DATA_ID, new RemoveWorldModuleData()
                {
                    localPlayer = _removeProvider.GetLocalPlayer(),
                    deltaTime = DeltaTime,
                    time = Time,
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateModuleData()
        {
            var data = _world.GetModuleData<RemoveWorldModuleData>(RemoveWorldModuleData.MODULE_DATA_ID);

            _threadDoubleBuffer.localPlayer = _removeProvider.GetLocalPlayer();
            _threadDoubleBuffer.time = Time;
            _threadDoubleBuffer.deltaTime = DeltaTime;

            _world.SetModuleData(RemoveWorldModuleData.MODULE_DATA_ID, _threadDoubleBuffer);

            _threadDoubleBuffer = data;
        }
    }
}