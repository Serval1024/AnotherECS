using System.Runtime.CompilerServices;
using System.Collections.Generic;
using AnotherECS.Serializer;

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

        private EventBuffer _buffer;

        public Events(uint recordTickLength)
        {
            _buffer = new EventBuffer((int)recordTickLength);
            NextTickForEvent = 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(ITickEvent @event)
        {
            if (NextTickForEvent > @event.Tick)
            {
                NextTickForEvent = @event.Tick;
            }

            _buffer.Push(@event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Find(uint tick, List<ITickEvent> result)
            => _buffer.Find(tick, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TickStarted(uint tick)
            => NextTickForEvent = tick + 1;

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

