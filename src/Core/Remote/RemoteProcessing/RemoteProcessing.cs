using AnotherECS.Core.Exceptions;
using AnotherECS.Core.Remote.Exceptions;
using AnotherECS.Serializer;
using AnotherECS.SyncTask;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public sealed class RemoteProcessing : IRemoteProcessing, IDisposable
    {
        private readonly IRemoteProvider _remoteProvider;
        private readonly ISerializer _serializer;
        private readonly IRemoteSyncStrategy _remoteSyncStrategy;

        private IWorldInner _world;
        private readonly BehaviorContext _context;

        private readonly RemoteMessageManager _requestTaskManager;

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy)
            : this(remoteProvider, remoteSyncStrategy, new DefaultSerializer()) { }

        public RemoteProcessing(IRemoteProvider remoteProvider, IRemoteSyncStrategy remoteSyncStrategy, ISerializer serializer)
        {
            _remoteProvider = remoteProvider;
            _serializer = serializer;
            _remoteSyncStrategy = remoteSyncStrategy;

            _context = new BehaviorContext(this, _remoteProvider);
            _requestTaskManager = new(OnProcessingMessage);


            remoteProvider.ReceiveBytes += OnReceiveOtherBytes;
            remoteProvider.ConnectPlayer += OnConnectPlayer;
            remoteProvider.DisconnectPlayer += OnDisconnectPlayer;
        }

        private object OnProcessingMessage(object result)
        {
            return result switch
            {
                RequestStateResult requestStaterResult => requestStaterResult,
                RejectRequestStateResult => throw new RejectRequestStateException(),
                _ => throw new InvalidOperationException(),
            };
        }

        public void Construct(IWorldInner world)
        {
            _world = world;
        }

        public void Update()
        {
            _requestTaskManager.Update();
        }

        public STask<ConnectResult> Connect()
            => _requestTaskManager.ToSTask(_remoteProvider.Connect());

        public STask Disconnect()
            => _requestTaskManager.ToSTask(_remoteProvider.Disconnect());

        public void SendOtherEvent(ITickEvent data)
        {
            SendOther(data);
        }

        public void SendState(StateRequest stateRequest)
        {
            var player = _remoteProvider.GetPlayer(stateRequest.PlayerId);
            if (player != default)
            {
                SendState(player, GetWorldState(stateRequest.SerializationLevel), stateRequest.MessageId, stateRequest.SerializationLevel);
            }
        }

        public void SendState(Player target, SerializationLevel serializationLevel)
        {
            SendState(target, GetWorldState(serializationLevel), 0, serializationLevel);
        }

        private WorldData GetWorldState(SerializationLevel serializationLevel)
            => (serializationLevel == SerializationLevel.DataAndConfigAndSystems)
                ? new WorldData(_world.InnerWorld.WorldData.Systems, _world.InnerWorld.WorldData.State)
                : new WorldData(null, _world.InnerWorld.WorldData.State);

        private void SendState(Player target, WorldData data, uint messageId, SerializationLevel serializationLevel)
        {
            _world.Run(new Processing.RunTaskHandler()
            {
                Data = data,
                Handler = p => _serializer.Pack(new StateRespond(messageId, (WorldData)p, serializationLevel), RemoteProcessingHelper.GetDependencySerializer(serializationLevel)),
                Completed = p => OnPackCompleted((byte[])p.Result, target)
            });
        }

        public void SendRejectState(StateRequest stateRequest)
        {
            _remoteProvider.Send(
                _remoteProvider.GetPlayer(stateRequest.PlayerId),
                _serializer.Pack(new StateRespond(stateRequest.MessageId, default, SerializationLevel.None))
                );
        }

        public void Send(Player target, object data)
        {
            _remoteProvider.Send(
                target,
                _serializer.Pack(data)
                );
        }

        public void SendOther(object data)
        {
            _remoteProvider.SendOther(_serializer.Pack(data));
        }

        public STask<RequestStateResult> RequestState(Player target, SerializationLevel serializationLevel)
        {
            var messageToken = _requestTaskManager.BeginMessage<RequestStateResult>();

            Send(target, new StateRequest()
            {
                PlayerId = _remoteProvider.GetLocalPlayer().Id,
                MessageId = messageToken.id,
                SerializationLevel = serializationLevel
            });

            return messageToken.task;
        }

        public void ApplyState(State state)
        {
            var wd = _world.InnerWorld.WorldData;
            wd.State = state;
            _world.InnerWorld.WorldData = wd;
        }

        public void ApplyWorldData(WorldData worldData)
        {
            if (worldData.Type == WorldData.DataType.SystemAndState)
            {
                _world.InnerWorld.WorldData = new Core.WorldData(worldData.Systems, worldData.State);
            }
            else
            {
                var wd = _world.InnerWorld.WorldData;
                wd.State = worldData.State;
                _world.InnerWorld.WorldData = wd;
            }
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

        public uint GetEventTickСorrection(uint tick)
        {
            var correctionTick = _remoteSyncStrategy.OnGetEventTickСorrection(tick);
            return correctionTick < 1 ? 1 : correctionTick;
        }

        public void Dispose()
        {
            _requestTaskManager.Dispose();

            _remoteProvider.ReceiveBytes -= OnReceiveOtherBytes;
            _remoteProvider.ConnectPlayer -= OnConnectPlayer;
            _remoteProvider.DisconnectPlayer -= OnDisconnectPlayer;
        }

        private void OnPackCompleted(byte[] bytes, Player target)
        {
            _remoteProvider.Send(target, bytes);
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
            if (data.Data.State != null && data.SerializationLevel != SerializationLevel.None)
            {
                var result = new RequestStateResult(new(data.Data, data.SerializationLevel));
                _requestTaskManager.EndMessage(data.MessageId, result);
                _remoteSyncStrategy.OnReceiveState(_context, sender, result);
            }
            else
            {
                _requestTaskManager.EndMessage(data.MessageId, new RejectRequestStateResult());
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

            public WorldData Data;
            public SerializationLevel SerializationLevel;

            public StateRespond(uint messageId, WorldData data, SerializationLevel serializationLevel)
            {
                MessageId = messageId;
                Data = data;
                SerializationLevel = serializationLevel;
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(MessageId);
                writer.Write(SerializationLevel);
                Data.Pack(ref writer);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                MessageId = reader.ReadUInt32();
                SerializationLevel = reader.ReadEnum<SerializationLevel>();
                Data.Unpack(ref reader);
            }
        }
    }
}