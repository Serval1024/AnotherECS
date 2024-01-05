using AnotherECS.Serializer;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal struct EventBuffer : ISerialize
    {
        private uint _counter;
        private SortEventBuffer _buffer;

        public EventBuffer(int bufferTickLimit)
        {
            _counter = 0;
            _buffer = new SortEventBuffer(128, bufferTickLimit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(ITickEvent @event)
            => _buffer.Add(MakeId(@event.Tick), new SortEventBuffer.ElementData(@event.Tick, @event));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Find(uint tick, List<ITickEvent> result)
            => _buffer.Find(tick, result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong MakeId(uint tick)
            => (ulong)tick << 32 | unchecked(++_counter);

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_counter);
            _buffer.Pack(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _counter = reader.ReadUInt32();
            _buffer.Unpack(ref reader);
        }
    }
}