using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct SortEventBuffer : ISerialize
    {
        private int _count;
        private ulong[] _keys;
        private EventData[] _values;
        private ulong _tickLimit;
        private uint _counter;

        public SortEventBuffer(int capacity, int tickLimit)
        {
            if (capacity == 0)
            {
                capacity = 1;
            }
            _count = 0;
            _keys = new ulong[capacity];
            _values = new EventData[capacity];
            _tickLimit = ((ulong)tickLimit) << 32;
            _counter = 0;
        }

        public void Add(ITickEvent value)
        {
            var key = MakeId(value.Tick);
            int i = Array.BinarySearch(_keys, 0, _count, key);
            if (i >= 0)
                throw new ArgumentException();
            Insert(~i, key, new EventData(key, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, ulong key, EventData value)
        {
            if (_count == _keys.Length)
            {
                EnsureCapacity();
            }

            if (_values[^1].tick - _values[0].tick > _tickLimit)
            {
                Array.Copy(_keys, 1, _keys, 0, _count - index);     //lost first
                Array.Copy(_values, 1, _values, 0, _count - index); //lost first
            }
            else
            {
                if (index < _count)
                {
                    Array.Copy(_keys, index, _keys, index + 1, _count - index);
                    Array.Copy(_values, index, _values, index + 1, _count - index);
                }
                ++_count;
            }
            
            _keys[index] = key;
            _values[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetTickByIndex(int index)
            => ToTick(_values[index].tick);

        public int Find(uint tick, List<ITickEvent> result)
        {
            int findIndex = _count - 1;
            for (; findIndex >= 0; --findIndex)
            {
                var valueTick = _values[findIndex].tick;
                if (valueTick < tick)
                {
                    break;
                }
            }

            for (int i = findIndex + 1; i < _count; ++i)
            {
                var valueTick = _values[i].tick;
                if (valueTick == tick)
                {
                    result.Add(_values[i].@event);
                }
                else
                {
                    return (result.Count == 0) ? -1 : i;
                }
            }
            return -1;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong MakeId(uint tick)
          => (ulong)tick << 32 | unchecked(++_counter);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ToTick(ulong key)
          => (uint)(key >> 32);

        private void EnsureCapacity()
        {
            var newCapacity = _keys.Length << 1;

            var newKeys = new ulong[newCapacity];
            var newValues = new EventData[newCapacity];

            Array.Copy(_keys, 0, newKeys, 0, _count);
            Array.Copy(_values, 0, newValues, 0, _count);

            _keys = newKeys;
            _values = newValues;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_count);
            writer.Write(_tickLimit);
            writer.Pack(_keys);
            writer.Pack(_values);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _count = reader.ReadInt32();
            _tickLimit = reader.ReadUInt64();
            _keys = reader.Unpack<ulong[]>();
            _values = reader.Unpack<EventData[]>();
        }



        private struct EventData : ISerialize
        {
            public ulong tick;
            public ITickEvent @event;

            public EventData(ulong tick, ITickEvent content)
            {
                this.tick = tick;
                this.@event = content;
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Pack(@event);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt64();
                @event = reader.Unpack<ITickEvent>();
            }
        }
    }
}

