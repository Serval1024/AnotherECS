using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
    internal struct WorldSignals
    {
        private List<SignalCallback> _signalBuffer;
        private Dictionary<Type, List<ISignalReceiver>> _signalReceivers;

        public static WorldSignals Create()
            => new()
            { 
                _signalBuffer = new(),
                _signalReceivers = new()
            };

        public void DispatchSignals(ref WorldData worldData)
        {
            worldData.State.FlushSignalCache(_signalBuffer);
            for (int i = 0; i < _signalBuffer.Count; ++i)
            {
                if (_signalReceivers.TryGetValue(_signalBuffer[i].Signal.GetType(), out var receivers))
                {
                    for (int j = 0; j < receivers.Count; ++j)
                    {
                        switch (_signalBuffer[i].Command)
                        {
                            case SignalCallback.CommandType.Fire:
                                {
                                    receivers[j].OnFire(_signalBuffer[i].Signal);
                                    break;
                                }
                            case SignalCallback.CommandType.Cancel:
                                {
                                    receivers[j].OnCancel(_signalBuffer[i].Signal);
                                    break;
                                }
                            case SignalCallback.CommandType.LeaveBuffer:
                                {
                                    receivers[j].OnLeaveHistoryBuffer(_signalBuffer[i].Signal);
                                    break;
                                }
                        }
                    }
                }
            }
        }

        public void AddSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
            if (_signalReceivers.TryGetValue(typeof(TSignal), out List<ISignalReceiver> signalReceivers))
            {
                signalReceivers.Add(receiver);
            }
            else
            {
                _signalReceivers[typeof(TSignal)] = new() { receiver };
            }
        }

        public void RemoveSignal<TSignal>(ISignalReceiver<TSignal> receiver)
            where TSignal : ISignal
        {
            if (_signalReceivers.TryGetValue(typeof(TSignal), out List<ISignalReceiver> signalReceivers))
            {
                signalReceivers.Remove(receiver);
            }
        }
    }
}