using AnotherECS.Serializer;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class FilterHistory : History, IHistory, IRevert, ISerialize
    {
        private Filter _subject;
        private uint _subjectId;

        private int _bufferIndex;
        private ElementData[] _buffer;

        public uint SubjectId
            => _subjectId;

        internal FilterHistory(ref ReaderContextSerializer reader, TickProvider tickProvider)
           : base(ref reader, tickProvider)
        { }

        public FilterHistory(in HistoryConfig config, TickProvider tickProvider)
            : base(config, tickProvider)
        {
            _buffer = new ElementData[config.buffersChangeCapacity];
        }

        public void SetSubject(Filter subject)
        {
            _subject = subject;
            _subjectId = subject.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(int dense, int count, int sparse, int id)
        {
            ref var element = ref _buffer[_bufferIndex++];
            element.tick = Tick;
            element.dense = dense;
            element.count = count;
            element.sparse = sparse;
            element.id = id;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _bufferIndex, ref _buffer, _recordHistoryLength, nameof(_buffer));
        }

        public void RevertTo(uint tick)
        {
            var isNeedContinueSearch = true;

            for (int i = _bufferIndex - 1; i >= 0; --i)
            {
                var frame = _buffer[i];

                if (frame.tick > tick)
                {
                    _subject.SetRaw(frame.dense, frame.count, frame.sparse, frame.id);
                }
                else
                {
                    _bufferIndex = i + 1;
                    isNeedContinueSearch = false;
                    break;
                }
            }

            if (isNeedContinueSearch)
            {
                for (int i = _buffer.Length - 1; i >= _bufferIndex; --i)
                {
                    var frame = _buffer[i];

                    if (frame.tick > tick)
                    {
                        _subject.SetRaw(frame.dense, frame.count, frame.sparse, frame.id);
                    }
                    else
                    {
                        _bufferIndex = (i + 1) % _buffer.Length;
                        break;
                    }
                }
            }
        }

        public override void Pack(ref WriterContextSerializer writer)
        {
            base.Pack(ref writer);

            writer.Write(_subjectId);
            writer.Write(_bufferIndex);
            writer.WriteUnmanagedArray(_buffer);
        }

        public override void Unpack(ref ReaderContextSerializer reader)
        {
            base.Unpack(ref reader);

            _subjectId = reader.ReadUInt32();
            _bufferIndex = reader.ReadInt32();
            _buffer = reader.ReadUnmanagedArray<ElementData>();
        }

        private struct ElementData : IFrameData, ISerialize
        {
            public uint Tick
                => tick;

            public uint tick;
            public int dense;
            public int count;
            public int sparse;
            public int id;

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Write(dense);
                writer.Write(count);
                writer.Write(sparse);
                writer.Write(id);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                dense = reader.ReadInt32();
                count = reader.ReadInt32();
                sparse = reader.ReadInt32();
                id = reader.ReadInt32();
            }
        }
    }
}