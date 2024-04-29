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
        private readonly IRemoteSyncStrategy _remoteSyncStrategy;

        private IWorldComposite _world;
        private BehaviorContext _context;

        private uint _idCounter;
        private readonly ConcurrentDictionary<uint, object> _taskDataResult = new();

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy)
            : this(remoteProvider, remoteSyncStrategy, new DefaultSerializer()) { }

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy, ISerializer serializer)
        {
            _remoteProvider = remoteProvider;
            _serializer = serializer;
            _remoteSyncStrategy = remoteSyncStrategy;

            _context = new BehaviorContext(this, _remoteProvider);

            remoteProvider.ReceiveBytes += OnReceiveOtherBytes;
            remoteProvider.ConnectPlayer += OnConnectPlayer;
            remoteProvider.DisconnectPlayer += OnDisconnectPlayer;
        }

        public void Construct(IWorldComposite world)
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
            var player = _remoteProvider.GetPlayer(stateRequest.PlayerId);
            if (player != default)
            {
                SendState(player, _world.State, stateRequest.MessageId, stateRequest.SerializationLevel);
            }
        }

        public void SendState(Player target, SerializationLevel serializationLevel)
        {
            if (serializationLevel == SerializationLevel.World)
            {
                SendState(target, _world.State, 0, serializationLevel);
            }
            else
            {
                SendState(target, _world, 0, serializationLevel);
            }
        }

        private void SendState(Player target, object data, uint messageId, SerializationLevel serializationLevel)
        {
            _world.Run(new Processing.RunTaskHandler()
            {
                Data = data,
                Handler = p => _serializer.Pack(new StateRespond(messageId, p, serializationLevel), RemoteProcessingHelper.GetDependencySerializer(serializationLevel)),
                Completed = p => OnPackCompleted(p, target)
            });
        }

        public void SendRejectState(StateRequest stateRequest)
        {
            var bytes = _serializer.Pack(new StateRespond(stateRequest.MessageId, null, SerializationLevel.None));

            _remoteProvider.Send(_remoteProvider.GetPlayer(stateRequest.PlayerId), bytes);
        }


        private void OnPackCompleted(Processing.RunTaskHandler runTaskHandler, Player target)
        {
            _remoteProvider.Send(target, (byte[])runTaskHandler.Result);
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

        public Task<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel)
        {
            var id = unchecked(++_idCounter);
            Send(target, new StateRequest()
            {
                PlayerId = _remoteProvider.GetLocalPlayer().Id,
                MessageId = id,
                SerializationLevel = serializationLevel
            });

            return TaskExtensions.Run(RequestStateResultTask, id);
        }

        private async Task<RequestStateResult> RequestStateResultTask(object id)
        {
            while (true)
            {
                await Task.Delay(15);

                if (_taskDataResult.TryGetValue((uint)id, out var result))
                {
                    return result switch
                    {
                        RequestStateResult requestStaterResult => requestStaterResult,
                        RejectRequestStateResult => throw new RejectRequestStateException(),
                        _ => throw new InvalidOperationException(),
                    };
                }
            }
            throw new InvalidOperationException();
        }

        public void ApplyState(State state)
        {
            _world.State = state;
        }

        public void ApplyWorld(IWorldExtend world)
        {
            _world.InnerWorld = world;
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

        public IWorldExtend GetWorld()
            => _world.InnerWorld;

        public virtual void Dispose()
        {
            _remoteProvider.ReceiveBytes -= OnReceiveOtherBytes;
            _remoteProvider.ConnectPlayer -= OnConnectPlayer;
            _remoteProvider.DisconnectPlayer -= OnDisconnectPlayer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Error(ErrorReport error)
        {
            if (_remoteSyncStrategy != null)
            {
                if (error.Is<UnpackCorruptedDataException>())
                {
                    _remoteSyncStrategy.OnReceiveCorruptedData(_context, error);
                }
                else if (error.Is<HistoryRevertTickLimitException>())
                {
                    _remoteSyncStrategy.OnRevertFailed(_context, error);
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
            if (data.Data != null && data.SerializationLevel != SerializationLevel.None)
            {
                var worldData = new WorldData(data.Data);
                var result = new RequestStateResult(worldData);
                _taskDataResult.AddOrUpdate(data.MessageId, result, (k, v) => result);

                _remoteSyncStrategy.OnReceiveState(
                    _context,
                    sender,
                    new Remote.StateRespond(worldData, data.SerializationLevel));
            }
            else
            {
                _taskDataResult.AddOrUpdate(
                    data.MessageId, 
                    new RejectRequestStateResult(),
                    (k, v) => new RejectRequestStateResult());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RequestState(Player sender, StateRequest stateRequest)
        {
            _context.BeginCheckRejectRequestState();

            _remoteSyncStrategy.OnRequestState(_context, sender, stateRequest);

            if (_context.EndCheckRejectRequestState())
            {
                _context.SendReject(stateRequest);
            }
        }

        private void OnConnectPlayer(Player player)
        {
            _remoteSyncStrategy.OnPlayerConnected(_context, player);
        }

        private void OnDisconnectPlayer(Player player)
        {
            _remoteSyncStrategy.OnPlayerDisconnected(_context, player);
        }

        private void OnReceiveOtherBytes(Player sender, byte[] bytes)
        {
            Receive(sender, bytes);
        }

     
        private struct StateRespond : ISerialize
        {
            public uint MessageId;
            public object Data;
            public SerializationLevel SerializationLevel;

            public StateRespond(uint messageId, object data, SerializationLevel serializationLevel)
            {
                MessageId = messageId;
                Data = data;
                SerializationLevel = serializationLevel;
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(MessageId);
                writer.Write(SerializationLevel);
                writer.Pack(Data);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                MessageId = reader.ReadUInt32();
                SerializationLevel = reader.ReadEnum<SerializationLevel>();
                Data = reader.Unpack<State>();
            }
        }
    }

    public struct StateRequest : ISerialize
    {
        public long PlayerId;
        public SerializationLevel SerializationLevel;

        internal uint MessageId;

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(MessageId);
            writer.Write(PlayerId);
            writer.Write(SerializationLevel);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            MessageId = reader.ReadUInt32();
            PlayerId = reader.ReadInt64();
            SerializationLevel = reader.ReadEnum<SerializationLevel>();
        }
    }
}