using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Remote.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AnotherECS.Core.Remote
{
    public class RemoteProcessing : IRemoteProcessing, IDisposable
    {
        private readonly IRemoteProvider _remoteProvider;
        private readonly ISerializer _serializer;
        private readonly IRemoteBehaviorStrategy _remoteBehaviorStrategy;

        private IWorldExtend _world;
        private BehaviorContext _context;

        public RemoteProcessing(IRemoteProvider remoteProvider)
            : this(remoteProvider, new LogAndThrowBehaviorStrategy()) { }

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteBehaviorStrategy remoteBehaviorStrategy)
            : this(remoteProvider, remoteBehaviorStrategy, new DefaultSerializer()) { }

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteBehaviorStrategy remoteBehaviorStrategy, ISerializer serializer)
        {
            _remoteProvider = remoteProvider;
            _serializer = serializer;
            _remoteBehaviorStrategy = remoteBehaviorStrategy;

            _context = new BehaviorContext(this, _remoteProvider);

            remoteProvider.ReceiveBytes += OnReceiveOtherBytes;
            remoteProvider.ConnectPlayer += OnConnectPlayer;
            remoteProvider.DisconnectPlayer += OnDisconnectPlayer;
        }

        public void Construct(IWorldExtend world)
        {
            _world = world;
        }

        public Task<ConnectResult> Connect()
            => _remoteProvider.Connect();

        public Task Disconnect()
            => _remoteProvider.Disconnect();

        public void SendOtherEvent(ITickEvent data)
        {
            SendOther(data);
        }

        public void SendState(Player target, StateSerializationLevel stateSerializationLevel)
        {
            SendState(target, _world.State, stateSerializationLevel);
        }

        public void SendState(Player target, State data, StateSerializationLevel stateSerializationLevel)
        {
            var bytes = _serializer.Pack(data, RemoteProcessingHelper.GetDependencySerializer(stateSerializationLevel));
            _remoteProvider.Send(target, bytes);
        }

        public void Send(Player target, object data)
        {
            var bytes = _serializer.Pack(data);
            _remoteProvider.Send(target, bytes);
        }

        public void SendOther(object data)
        {
            var bytes = _serializer.Pack(data);
            _remoteProvider.SendOther(bytes);
        }

        public void RequestState(Player target, StateSerializationLevel stateSerializationLevel)
        {
            Send(target, new RequestStateEvent() { level = stateSerializationLevel });
        }

        public void ApplyState(State state)
        {
            _world.State = state;
        }

        public void Receive(Player sender, byte[] bytes)
        {
            try
            {
                ProcessingCommand(sender, _serializer.Unpack(bytes));
            }
            catch (Exception ex)
            {
                Error(new ErrorReport(new UnpackCorruptedDataException(sender, ex)));
            }
        }

        public double GetGlobalTime()
            => _remoteProvider.GetGlobalTime();

        public Player GetLocalPlayer()
            => _remoteProvider.GetLocalPlayer();

        public virtual void Dispose()
        {
            _remoteProvider.ReceiveBytes -= OnReceiveOtherBytes;
            _remoteProvider.ConnectPlayer -= OnConnectPlayer;
            _remoteProvider.DisconnectPlayer -= OnDisconnectPlayer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Error(ErrorReport error)
        {
            if (_remoteBehaviorStrategy != null)
            {
                if (error.Is<UnpackCorruptedDataException>())
                {
                    _remoteBehaviorStrategy.OnReceiveCorruptedData(_context, error);
                }
                else if (error.Is<HistoryRevertTickLimitException>())
                {
                    _remoteBehaviorStrategy.OnRevertFailed(_context, error);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessingCommand(Player sender, object data)
        {
            switch (data)
            {
                case ITickEvent:
                    {
                        ReceiveEvent((ITickEvent)data);
                        break;
                    }
                case State:
                    {
                        ReceiveState(sender, (State)data);
                        break;
                    }
                case RequestStateEvent:
                    {
                        RequestState(sender, (RequestStateEvent)data);
                        break;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveEvent(ITickEvent @event)
        {
            _world.SendEvent(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveState(Player sender, State data)
        {
            _remoteBehaviorStrategy.OnReceiveState(_context, sender, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RequestState(Player sender, RequestStateEvent data)
        {
            _remoteBehaviorStrategy.OnRequestState(_context, sender, data.level);
        }

        private void OnConnectPlayer(Player player)
        {
            _remoteBehaviorStrategy.OnPlayerConnected(_context, player);
        }

        private void OnDisconnectPlayer(Player player)
        {
            _remoteBehaviorStrategy.OnPlayerDisconnected(_context, player);
        }

        private void OnReceiveOtherBytes(Player sender, byte[] bytes)
        {
            Receive(sender, bytes);
        }


        private struct RequestStateEvent : ISerialize
        {
            public StateSerializationLevel level;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(level);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                level = reader.ReadEnum<StateSerializationLevel>();
            }
        }
    }
}
