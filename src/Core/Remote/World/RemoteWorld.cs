using AnotherECS.Core.Remote.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public class RemoteWorld : IWorldComposite, IWorldCommunicate, IDisposable, ISerializeConstructor
    {
        public uint Id => _world.Id;
        public IRemoteProcessing Remote => _remoteProcessing;
        public IWorldExtend InnerWorld
        { 
            get => _world;
            set 
            {
                if (_world != value)
                {
                    _world = value;
                    InitInternal(_remoteProcessing);
                }
            }
        }

        public double Time => _remoteProcessing.GetGlobalTime();

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
            get => _world?.State;
            set
            {
                ExceptionHelper.ThrowIfWorldInvalid(this);

                SetState(value);
            }
        }

        private bool _isUpdate;
        private IWorldExtend _world;
        private IRemoteProcessing _remoteProcessing;

        private RemoveWorldModuleData _moduleDataThreadDoubleBuffer;

        public RemoteWorld(IWorldExtend world, IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy)
            : this(world, new RemoteProcessing(remoteProvider, remoteSyncStrategy)) { }

        public RemoteWorld(IWorldExtend world, IRemoteProvider remoteProvider)
            : this(world, remoteProvider, new LogAndThrowStrategy()) { }

        public RemoteWorld(IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy)
           : this(null, remoteProvider, remoteSyncStrategy) { }

        public RemoteWorld(IWorldExtend world, IRemoteProcessing remoteProcessing)
        {
            _world = world;
            InitInternal(remoteProcessing);
        }

        public RemoteWorld(ref ReaderContextSerializer reader)
        {
            Unpack(ref reader);
            InitInternal(reader.Dependency.Resolve<IRemoteProcessing>());
        }

        public Task<ConnectResult> Connect()
            => _remoteProcessing.Connect();

        public Task Disconnect()
            => _remoteProcessing.Disconnect();

        public void UpdateFromMainThread()
        {
            if (_isUpdate)
            {
                switch (_world.LiveState)
                {
                    case LiveState.Startup:
                        {
                            UpdateModuleData();
                            var target = (int)(Time / DeltaTime);
                            var delta = target - (int)_world.RequestTick;

                            _world.Tick((uint)delta);                            
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
                
                _world.DispatchSignals();
            }
        }

        public void SendEvent(IEvent @event)
        {
            SendEvent(ToITickEvent(@event));
        }

        public void SendEvent(ITickEvent @event)
        {
            ExceptionHelper.ThrowIfWorldInvalid(this);

            _remoteProcessing.SendOtherEvent(@event);
            _world.SendEvent(@event);
        }

        public void AddSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
            ExceptionHelper.ThrowIfWorldInvalid(this);

            _world.AddSignal(receiver);
        }

        public void RemoveSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
            ExceptionHelper.ThrowIfWorldInvalid(this);

            _world.RemoveSignal(receiver);
        }

        public async void DestroyAndDispose()
        {
            Destroy();
            await Disconnect();
            Dispose();
        }

        public void Dispose()
        {
            _remoteProcessing.Dispose();
            _world?.Dispose();
        }

        public void Run(Processing.RunTaskHandler runTaskHandler)
        {
            ExceptionHelper.ThrowIfWorldInvalid(this);

            _world.Run(runTaskHandler);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Pack(_world);
            writer.Write(_deltaTime);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _world = reader.Unpack<IWorldExtend>();
            _deltaTime = reader.ReadDouble();
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
                    localPlayer = _remoteProcessing.GetLocalPlayer(),
                    deltaTime = DeltaTime,
                    time = Time,
                });
            }

            Thread.MemoryBarrier();
            _isUpdate = value != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateModuleData()
        {
            var data = _world.GetModuleData<RemoveWorldModuleData>(RemoveWorldModuleData.MODULE_DATA_ID);

            _moduleDataThreadDoubleBuffer.localPlayer = _remoteProcessing.GetLocalPlayer();
            _moduleDataThreadDoubleBuffer.time = Time;
            _moduleDataThreadDoubleBuffer.deltaTime = DeltaTime;

            _world.SetModuleData(RemoveWorldModuleData.MODULE_DATA_ID, _moduleDataThreadDoubleBuffer);

            _moduleDataThreadDoubleBuffer = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitInternal(IRemoteProcessing remoteProcessing)
        {
            _remoteProcessing = remoteProcessing ?? throw new ArgumentNullException(nameof(remoteProcessing));

            _moduleDataThreadDoubleBuffer ??= new RemoveWorldModuleData();

            _remoteProcessing.Construct(this);

            if (_world != null)
            {
                if (_world.LiveState == LiveState.Raw)
                {
                    _world.Init();
                }

                if (_world.State != null)
                {
                    SetState(_world.State);
                }
            }
        }
    }
}