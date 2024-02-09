using AnotherECS.Serializer;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct Events : ISerialize
    {
        public uint NextTickForEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        private SortEventBuffer _buffer;

        public Events(uint recordTickLength)
        {
            _buffer = new SortEventBuffer(128, (int)recordTickLength);
            NextTickForEvent = int.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(ITickEvent @event)
        {
            if (NextTickForEvent > @event.Tick)
            {
                NextTickForEvent = @event.Tick;
            }

            _buffer.Add(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CollectForProcessing(uint tick, List<ITickEvent> result)
        {
            var indexAfterHit = _buffer.Find(tick, result);
            if (NextTickForEvent == tick)
            {
                if (indexAfterHit != -1)
                {
                    NextTickForEvent = _buffer.GetTickByIndex(indexAfterHit);
                }
                else
                {
                    ++NextTickForEvent;
                }
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(NextTickForEvent);
            _buffer.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            NextTickForEvent = reader.ReadUInt32();
            _buffer.Unpack(ref reader);
        }
    }
}

