using AnotherECS.Serializer;
using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct Signals : ISerialize
    {
        private readonly uint _recordTickLength;

        private int _bufferIndex;
        private SignalData[] _buffer;

        private int _tickBufferIndex;
        private SignalEvent[] _tickBuffer;

        private int _bufferIndexForDiffNow;
        private int _startDiffBufferIndex;

        private int _cancelBufferTempCount;
        private SignalData[] _cancelBufferTemp;

        private bool _isSequenceBroken;

        public Signals(uint recordTickLength)
        {
            _recordTickLength = recordTickLength;

            _bufferIndex = 0;
            _buffer = new SignalData[8];

            _tickBufferIndex = 0;
            _tickBuffer = new SignalEvent[8];

            _bufferIndexForDiffNow = 0;
            _startDiffBufferIndex = 0;

            _cancelBufferTempCount = 0;
            _cancelBufferTemp = new SignalData[8];
            

            _isSequenceBroken = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(uint tick, uint type, ISignal signal)
        {
            if (++_bufferIndex == _buffer.Length)
            {
                if (_buffer[^1].Tick - _buffer[0].Tick > _recordTickLength)
                {
                    _bufferIndex = 0;
                }
                else
                {
                    Array.Resize(ref _buffer, _buffer.Length << 1);
                }
            }

            var signalData = new SignalData(tick, type, signal);

            if (_isSequenceBroken || _buffer[_bufferIndex].HashCode != signalData.HashCode)
            {
                _isSequenceBroken = true;

                if (_buffer[_bufferIndex].Tick != 0)
                {
                    AddTickBuffer(SignalEvent.CommandType.LeaveBuffer, _buffer[_bufferIndex].Signal);
                }

                _buffer[_bufferIndex] = signalData;
                AddTickBuffer(SignalEvent.CommandType.Fire, signal);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddTickBuffer(SignalEvent.CommandType type, ISignal signal)
        {
            _tickBuffer[_tickBufferIndex] = new SignalEvent(type, signal);

            if (++_tickBufferIndex == _tickBuffer.Length)
            {
                Array.Resize(ref _tickBuffer, _tickBuffer.Length << 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<SignalEvent> GetCurrentTickBuffer()
            => _tickBuffer.AsSpan(0, _tickBufferIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCurrentTickBuffer()
        {
            _tickBufferIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiffBuffer()
            => _startDiffBufferIndex >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<SignalData> GetDiffBuffer()
        {
            ComputeDiffBuffer();
            return _cancelBufferTemp.AsSpan(0, _startDiffBufferIndex + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            _isSequenceBroken = false;
            _cancelBufferTempCount = 0;

            for (int i = _bufferIndex; i >= 0; --i)
            {
                ref var frame = ref _buffer[i];

                if (frame.Tick > tick)
                {
                    AddToCancelBuffer(frame, ref _cancelBufferTempCount);
                }
                else
                {
                    _bufferIndex = i;
                    _bufferIndexForDiffNow = (_bufferIndex + 1) % _buffer.Length;
                    return;
                }
            }

            for (int i = _buffer.Length - 1; i > _bufferIndex; --i)
            {
                ref var frame = ref _buffer[i];

                if (frame.Tick > tick)
                {
                    AddToCancelBuffer(frame, ref _cancelBufferTempCount);
                }
                else
                {
                    _bufferIndex = i;
                    _bufferIndexForDiffNow = (_bufferIndex + 1) % _buffer.Length;
                    return;
                }
            }

            _bufferIndexForDiffNow = (_bufferIndex + 1) % _buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_bufferIndex);
            writer.WriteArray(_buffer, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _bufferIndex = reader.ReadInt32();
            _buffer = reader.ReadArray<SignalData>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToCancelBuffer(SignalData data, ref int cancelBufferIndex)
        {
            if (cancelBufferIndex == _cancelBufferTemp.Length)
            {
                Array.Resize(ref _cancelBufferTemp, _cancelBufferTemp.Length << 1);
            }
            _cancelBufferTemp[cancelBufferIndex++] = data;
        }

        private void ComputeDiffBuffer()
        {
            if (_cancelBufferTempCount <= 0)
            {
                _startDiffBufferIndex = -1;
                return;
            }

            int cancelBufferIndex = _cancelBufferTempCount - 1;

            var isFirstSegment = _bufferIndexForDiffNow - 1 <= _bufferIndex;

            if (cancelBufferIndex >= 0)
            {
                for (int i = _bufferIndexForDiffNow; i < _buffer.Length; ++i)
                {
                    ref var frame = ref _buffer[i];

                    if (cancelBufferIndex >= 0)
                    {
                        if (isFirstSegment && i > _bufferIndex)
                        {
                            _startDiffBufferIndex = cancelBufferIndex;
                            break;
                        }

                        if (_cancelBufferTemp[cancelBufferIndex].HashCode == frame.HashCode)
                        {
                            --cancelBufferIndex;
                        }
                        else
                        {
                            _startDiffBufferIndex = cancelBufferIndex;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (cancelBufferIndex >= 0)
            {
                if (isFirstSegment && _bufferIndex == _buffer.Length - 1)
                {
                    _startDiffBufferIndex = cancelBufferIndex;
                }
                else
                {
                    for (int i = 0; i < _bufferIndexForDiffNow; ++i)
                    {
                        ref var frame = ref _buffer[i];

                        if (cancelBufferIndex >= 0)
                        {
                            if (!isFirstSegment && i > _bufferIndex)
                            {
                                _startDiffBufferIndex = cancelBufferIndex;
                                break;
                            }

                            if (_cancelBufferTemp[cancelBufferIndex].HashCode == frame.HashCode)
                            {
                                --cancelBufferIndex;
                            }
                            else
                            {
                                _startDiffBufferIndex = cancelBufferIndex;
                                break;
                            }
                        }
                    }
                }
            }

            _startDiffBufferIndex = cancelBufferIndex;
        }


        internal struct SignalData : ISerialize
        {
            public uint Tick { get; private set; }
            public ISignal Signal { get; private set; }
            public ulong HashCode { get; private set; }

            public SignalData(uint tick, uint type, ISignal signal)
            {
                Tick = tick;
                Signal = signal;
                HashCode =
                    ((ulong)((ushort)type) << 48) |
                    ((ulong)(ushort)tick << 32) |
                    ((ulong)(uint)Signal.GetHashCode());
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(Tick);
                writer.Pack(Signal);
                writer.Pack(HashCode);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                Tick = reader.ReadUInt32();
                Signal = reader.Unpack<ISignal>();
                HashCode = reader.ReadUInt64();
            }
        }

        internal struct SignalEvent
        {
            public CommandType Command;
            public ISignal Signal;

            public SignalEvent(CommandType command, ISignal signal)
            {
                Command = command;
                Signal = signal;
            }

            public enum CommandType
            {
                None = 0,
                Fire,
                LeaveBuffer,
            }
        }
    }
}

