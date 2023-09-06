using AnotherECS.Serializer;
using System;
using System.Collections.Generic;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal struct EventSortBuffer : ISerialize
    {
        private int _count;
        private ulong[] _keys;
        private ElementData[] _values;
        private int _tickLimit;

        public EventSortBuffer(int capacity, int tickLimit)
        {
            if (capacity == 0)
            {
                capacity = 1;
            }
            _count = 0;
            _keys = new ulong[capacity];
            _values = new ElementData[capacity];
            _tickLimit = tickLimit;
        }

        public void Add(ulong key, ElementData value)
        {
            int i = Array.BinarySearch(_keys, 0, _count, key);
            if (i >= 0)
                throw new ArgumentException();
            Insert(~i, key, value);
        }

        private void Insert(int index, ulong key, ElementData value)
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

        public void Find(uint tick, List<ITickEvent> result)
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
                    result.Add(_values[i].content);
                }
                else
                {
                    break;
                }
            }
        }

        private void EnsureCapacity()
        {
            var newCapacity = _keys.Length << 1;

            var newKeys = new ulong[newCapacity];
            var newValues = new ElementData[newCapacity];

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
            _tickLimit = reader.ReadInt32();
            _keys = reader.Unpack<ulong[]>();
            _values = reader.Unpack<ElementData[]>();
        }



        public struct ElementData : ISerialize
        {
            public long tick;
            public ITickEvent content;

            public ElementData(long tick, ITickEvent content)
            {
                this.tick = tick;
                this.content = content;
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write((uint)tick);
                writer.Pack(content);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                content = reader.Unpack<ITickEvent>();
            }
        }
    }
}

