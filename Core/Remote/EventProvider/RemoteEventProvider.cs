using AnotherECS.Core.Remote.Exceptions;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Remote
{
    public class RemoteEventProvider : IRemoteEventProvider, IDisposable
    {
        private readonly ISerializer _serializer;
        private readonly IRemoteProvider _remoteProvider;

        public event ReceiveEventHandler ReceiveEvent;
        public event ReceiveStateHandler ReceiveState;


        public RemoteEventProvider(IRemoteProvider bytesDataProvider)
            : this(new DefaultSerializer(), bytesDataProvider) { }

        public RemoteEventProvider(ISerializer serializer, IRemoteProvider remoteProvider)
        {
            _serializer = serializer;
            _remoteProvider = remoteProvider;

            remoteProvider.ReceiveOtherBytes += OnReceiveOtherBytes;
        }

        public void SendOtherEvent(ITickEvent data)
        {
            var bytes = _serializer.Pack(new Command() { commandType = Command.CommandType.Event, data = data });
            _remoteProvider.SendOther(bytes);
        }

        public void SendState(State data)
        {
            var bytes = _serializer.Pack(new Command() { commandType = Command.CommandType.State, data = data });
            _remoteProvider.SendOther(bytes);
        }

        public void Receive(byte[] bytes)
        {
            try
            {
                ProcessingCommand((Command)_serializer.Unpack(bytes));
            }
            catch (Exception ex)
            {
                _remoteProvider.Error(new ErrorReport(0, new UnpackCorruptedDataException(ex)));
            }
        }

        public void Dispose()
        {
            _remoteProvider.ReceiveOtherBytes -= OnReceiveOtherBytes;
            _remoteProvider.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessingCommand(Command command)
        {
            switch (command.commandType)
            {
                case Command.CommandType.Event:
                    {
                        ReceiveEvent((ITickEvent)command.data);
                        break;
                    }
                case Command.CommandType.State:
                    {
                        ReceiveState((State)command.data);
                        break;
                    }
            }
        }

        private void OnReceiveOtherBytes(byte[] bytes)
        {
            Receive(bytes);
        }
    }
}
