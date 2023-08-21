using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
#if ANOTHERECS_DEBUG
using AnotherECS.Debug;
#endif

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class EntitiesHistory : History, IHistory, IRevert, ISerialize
    {
        private Entities _subject;

        private int _recycledCountIndex = 0;
        private TickIntData[] _recycledCountBuffer;

        private int _recycledIndex = 0;
        private RecycledData[] _recycledBuffer;

        private int _elementUshortIndex = 0;
        private ElementUshortData[] _elementUshortBuffer;


        internal EntitiesHistory(ref ReaderContextSerializer reader, TickProvider tickProvider)
            : base(ref reader, tickProvider)
        { }

        public EntitiesHistory(in HistoryConfig config, TickProvider tickProvider)
            : base(config, tickProvider)
        {
            _recycledCountBuffer = new TickIntData[config.buffersAddRemoveCapacity];
            _recycledBuffer = new RecycledData[config.buffersAddRemoveCapacity];
            _elementUshortBuffer = new ElementUshortData[config.buffersChangeCapacity];    
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSubject(Entities subject)
            => _subject = subject;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycledCount(int recycledCount)
        {
            ref var element = ref _recycledCountBuffer[_recycledCountIndex++];
            element.tick = Tick;
            element.value = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledCountIndex, ref _recycledCountBuffer, _recordHistoryLength, nameof(_recycledCountBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycled(int recycled, int recycledCount)
        {
            ref var element = ref _recycledBuffer[_recycledIndex++];
            element.tick = Tick;
            element.recycled = recycled;
            element.recycledIndex = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledIndex, ref _recycledBuffer, _recordHistoryLength, nameof(_recycledBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushArrayElement(int offset, ushort data)
        {
            ref var element = ref _elementUshortBuffer[_elementUshortIndex++];
            element.tick = Tick;
            element.offset = offset;
            element.data = data;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _elementUshortIndex, ref _elementUshortBuffer, _recordHistoryLength, nameof(_elementUshortBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick)
        {
            RevertToRecycledCountBuffer(tick);
            RevertToRecycledBuffer(tick);
            RevertToElementBuffer(tick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledCountBuffer(uint tick)
        {
            var isNeedContinueSearch = true;

            for (int i = _recycledCountIndex - 1; i >= 0; --i)
            {
                var frame = _recycledCountBuffer[i];

                if (frame.tick > tick)
                {
                    _subject.SetRecycledCountRaw(frame.value);
                }
                else
                {
                    _recycledCountIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = _recycledCountBuffer.Length - 1; i >= _recycledCountIndex; --i)
                {
                    var frame = _recycledCountBuffer[i];

                    if (frame.tick > tick)
                    {
                        _subject.SetRecycledCountRaw(frame.value);
                    }
                    else
                    {
                        _recycledCountIndex = (i + 1) % _recycledCountBuffer.Length;
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledBuffer(uint tick)
        {
            var isNeedContinueSearch = true;

            var recycled = _subject.GetRecycledRaw();

            for (int i = _recycledIndex - 1; i >= 0; --i)
            {
                var frame = _recycledBuffer[i];

                if (frame.tick > tick)
                {
                    recycled[frame.recycledIndex] = frame.recycled;
                }
                else
                {
                    _recycledIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = _recycledBuffer.Length - 1; i >= _recycledIndex; --i)
                {
                    var frame = _recycledBuffer[i];

                    if (frame.tick > tick)
                    {
                        recycled[frame.recycledIndex] = frame.recycled;
                    }
                    else
                    {
                        _recycledIndex = (i + 1) % _recycledBuffer.Length;
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToElementBuffer(uint tick)
        {
            
            var isNeedContinueSearch = true;

            var elements = _subject.GetArrayRaw();

            for (int i = _elementUshortIndex - 1; i >= 0; --i)
            {
                var frame = _elementUshortBuffer[i];

                if (frame.tick > tick)
                {
                    elements[frame.offset] = frame.data;
                }
                else
                {
                    _elementUshortIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = _elementUshortBuffer.Length - 1; i >= _elementUshortIndex; --i)
                {
                    var frame = _elementUshortBuffer[i];

                    if (frame.tick > tick)
                    {
                        elements[frame.offset] = frame.data;
                    }
                    else
                    {
                        _elementUshortIndex = (i + 1) % _elementUshortBuffer.Length;
                        break;
                    }
                }
            }
        }

        public override void Pack(ref WriterContextSerializer writer)
        {
            base.Pack(ref writer);

            writer.Write(_recycledCountIndex);
            writer.WriteUnmanagedArray(_recycledCountBuffer);

            writer.Write(_recycledIndex);
            writer.WriteUnmanagedArray(_recycledBuffer);

            writer.Write(_elementUshortIndex);
            writer.WriteUnmanagedArray(_elementUshortBuffer);
        }

        public override void Unpack(ref ReaderContextSerializer reader)
        {
            base.Unpack(ref reader);

            _recycledCountIndex = reader.ReadInt32();
            _recycledCountBuffer = reader.ReadUnmanagedArray<TickIntData>();

            _recycledIndex = reader.ReadInt32();
            _recycledBuffer = reader.ReadUnmanagedArray<RecycledData>();

            _elementUshortIndex = reader.ReadInt32();
            _elementUshortBuffer = reader.ReadUnmanagedArray<ElementUshortData>();
        }

        private struct TickIntData : IFrameData, ISerialize
        {
            public uint Tick
                => tick;

            public uint tick;
            public int value;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Write(value);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                value = reader.ReadInt32();
            }
        }

        private struct RecycledData : IFrameData, ISerialize
        {
            public uint Tick
                => tick;

            public uint tick;
            public int recycled;
            public int recycledIndex;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Write(recycled);
                writer.Write(recycledIndex);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                recycled = reader.ReadInt32();
                recycledIndex = reader.ReadInt32();
            }
        }

        private struct ElementUshortData : IFrameData, ISerialize
        {
            public uint Tick
                => tick;

            public uint tick;
            public int offset;
            public ushort data;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Write(offset);
                writer.Write(data);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                offset = reader.ReadInt32();
                data = reader.ReadUInt16();
            }
        }
    }
}