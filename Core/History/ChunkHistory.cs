using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    internal sealed class ChunkHistory<TChunkUnit> : History, IHistory, ISerialize
        where TChunkUnit : unmanaged
    {
        private int _recycledCountIndex = 0;
        private TickData<uint>[] _recycledCountBuffer;

        private int _recycledIndex = 0;
        private RecycledData<uint>[] _recycledBuffer;

        private int _countIndex = 0;
        private TickData<uint>[] _countBuffer;

        private int _elementUshortIndex = 0;
        private ElementOffsetData<TChunkUnit>[] _elementUshortBuffer;


        internal ChunkHistory(ref ReaderContextSerializer reader, HistoryArgs args)
            : base(ref reader, args.tickProvider)
        { }

        public ChunkHistory(in HistoryByChangeArgs args)
            : base(new HistoryArgs(args))
        {
            _recycledCountBuffer = new TickData<uint>[args.buffersAddRemoveCapacity];
            _recycledBuffer = new RecycledData<uint>[args.buffersAddRemoveCapacity];
            _countBuffer = new TickData<uint>[args.buffersAddRemoveCapacity];
            _elementUshortBuffer = new ElementOffsetData<TChunkUnit>[args.buffersChangeCapacity];    
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycledCount(uint recycledCount)
        {
            ref var element = ref _recycledCountBuffer[_recycledCountIndex++];
            element.tick = Tick;
            element.value = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledCountIndex, ref _recycledCountBuffer, _recordHistoryLength, nameof(_recycledCountBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycled(uint recycled, uint recycledCount)
        {
            ref var element = ref _recycledBuffer[_recycledIndex++];
            element.tick = Tick;
            element.recycled = recycled;
            element.recycledIndex = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledIndex, ref _recycledBuffer, _recordHistoryLength, nameof(_recycledBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(uint offset, TChunkUnit data)
        {
            ref var element = ref _elementUshortBuffer[_elementUshortIndex++];
            element.tick = Tick;
            element.offset = offset;
            element.data = data;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _elementUshortIndex, ref _elementUshortBuffer, _recordHistoryLength, nameof(_elementUshortBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushCount(uint count)
        {
            ref var element = ref _countBuffer[_countIndex++];
            element.tick = Tick;
            element.value = count;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _countIndex, ref _countBuffer, _recordHistoryLength, nameof(_countBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, ref ChunkMemory<TChunkUnit> subject)
        {
            RevertToRecycledCountBuffer(tick, ref subject);
            RevertToRecycledBuffer(tick, ref subject);
            RevertToCountBuffer(tick, ref subject);
            RevertToElementBuffer(tick, ref subject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledCountBuffer(uint tick, ref ChunkMemory<TChunkUnit> subject)
        {
            RevertHelper.RevertToRecycledCountBuffer(ref subject, tick, _recycledCountBuffer, ref _recycledCountIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledBuffer(uint tick, ref ChunkMemory<TChunkUnit> subject)
        {
            RevertHelper.RevertToRecycledBuffer(ref subject, tick, _recycledBuffer, ref _recycledCountIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToCountBuffer(uint tick, ref ChunkMemory<TChunkUnit> subject)
        {
            RevertHelper.RevertToCountBuffer(ref subject, tick, _countBuffer, ref _countIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void RevertToElementBuffer(uint tick, ref ChunkMemory<TChunkUnit> subject)
        {
            RevertHelper.RevertToElementBuffer(ref subject, tick, _elementUshortBuffer, ref _elementUshortIndex);
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
            _recycledCountBuffer = reader.ReadUnmanagedArray<TickData<uint>>();

            _recycledIndex = reader.ReadInt32();
            _recycledBuffer = reader.ReadUnmanagedArray<RecycledData<uint>>();

            _elementUshortIndex = reader.ReadInt32();
            _elementUshortBuffer = reader.ReadUnmanagedArray<ElementOffsetData<TChunkUnit>>();
        }
    }
}