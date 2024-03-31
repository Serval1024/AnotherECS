using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Remote.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Collections.Concurrent;
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

        private uint _idCounter;
        private ConcurrentDictionary<uint, object> _taskDataResult = new();


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

        public void SendState(StateRequest stateRequest)
        {
            var player = _remoteProvider.GetPlayer(stateRequest.playerId);
            if (player != default)
            {
                SendState(player, _world.State, stateRequest.id, stateRequest.level);
            }
        }

        public void SendState(Player target, StateSerializationLevel stateSerializationLevel)
        {
            SendState(target, _world.State, 0, stateSerializationLevel);
        }

        private void SendState(Player target, State state, uint id, StateSerializationLevel stateSerializationLevel)
        {
            var bytes = _serializer.Pack(new StateRespond(id, state), RemoteProcessingHelper.GetDependencySerializer(stateSerializationLevel));
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

        public Task<RequestStateResult> RequestState(Player target, StateSerializationLevel stateSerializationLevel)
        {
            var id = ++_idCounter;
            Send(target, new StateRequest() { id = id, level = stateSerializationLevel });

            return TaskExtensions.Run(RequestStateResultTask, id);
        }

        private async Task<RequestStateResult> RequestStateResultTask(object id)
        {
            int i = 10;
            while (i-- > 0)
            {
                await Task.Delay(1);
                
                if (_taskDataResult.TryGetValue((uint)id, out var result))
                {
                    if (result is RequestStateResult requestStaterResult)
                    {
                        return requestStaterResult;
                    }
                }
            }
            return new RequestStateResult();
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
                case StateRespond:
                    {
                        ReceiveState(sender, (StateRespond)data);
                        break;
                    }
                case StateRequest:
                    {
                        RequestState(sender, (StateRequest)data);
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
        private void ReceiveState(Player sender, StateRespond data)
        {
            var result = new RequestStateResult(data.state);
            _taskDataResult.AddOrUpdate(data.id, result, (k, v) => result);
            _remoteBehaviorStrategy.OnReceiveState(_context, sender, data.state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RequestState(Player sender, StateRequest data)
        {
            _remoteBehaviorStrategy.OnRequestState(_context, sender, data);
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


        private struct StateRespond : ISerialize
        {
            public uint id;
            public State state;

            public StateRespond(uint id, State state)
            {
                this.id = id;
                this.state = state;
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(id);
                writer.Pack(state);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                id = reader.ReadUInt32();
                state = reader.Unpack<State>();
            }
        }
    }

    public struct StateRequest : ISerialize
    {
        public uint id;
        public long playerId;
        public StateSerializationLevel level;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(id);
            writer.Write(level);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            id = reader.ReadUInt32();
            level = reader.ReadEnum<StateSerializationLevel>();
        }
    }
}