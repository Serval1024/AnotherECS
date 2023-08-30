using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;
using AnotherECS.Collections;
using AnotherECS.Unsafe;
#if ANOTHERECS_DEBUG
using AnotherECS.Debug;
#endif

namespace AnotherECS.Core
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    internal unsafe sealed class DArrayHistory : History, IHistory, IDisposable, ISerialize
    {
        private uint _buffersCapacity;

        private int _recycledCountIndex = 0;
        private TickData<ushort>[] _recycledCountBuffer;

        private int _recycledIndex = 0;
        private RecycledData<ushort>[] _recycledBuffer;

        private int _countIndex = 0;
        private TickData<ushort>[] _countBuffer;

        private ElementBufferData[] _elementBuffer;


        internal DArrayHistory(ref ReaderContextSerializer reader, in HistoryArgs args)
            : base(ref reader, args.tickProvider)
        { }

        public DArrayHistory(in HistoryByChangeArgs args, uint dArrayBuffersCapacity)
            : base(new HistoryArgs(args))
        {
            _recycledCountBuffer = new TickData<ushort>[args.buffersAddRemoveCapacity];
            _recycledBuffer = new RecycledData<ushort>[args.buffersAddRemoveCapacity];
            _countBuffer = new TickData<ushort>[args.buffersAddRemoveCapacity];
            _elementBuffer = new ElementBufferData[1];
            _buffersCapacity = dArrayBuffersCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SubjectResized(int size)
        {
            var oldLength = _elementBuffer.Length;
            Array.Resize(ref _elementBuffer, size);
            for(int i = oldLength; i < _elementBuffer.Length; ++i)
            {
                _elementBuffer[i] = new ElementBufferData() { elements = new ElementData[_buffersCapacity] };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycledCount(ushort recycledCount)
        {
            ref var element = ref _recycledCountBuffer[_recycledCountIndex++];
            element.tick = Tick;
            element.value = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledCountIndex, ref _recycledCountBuffer, _recordHistoryLength, nameof(_recycledCountBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushRecycled(ushort recycled, ushort recycledCount)
        {
            ref var element = ref _recycledBuffer[_recycledIndex++];
            element.tick = Tick;
            element.recycled = recycled;
            element.recycledIndex = recycledCount;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _recycledIndex, ref _recycledBuffer, _recordHistoryLength, nameof(_recycledBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushCount(ushort count)
        {
            ref var element = ref _countBuffer[_countIndex++];
            element.tick = Tick;
            element.value = count;

            HistoryUtils.CheckAndResizeLoopBuffer(ref _countIndex, ref _countBuffer, _recordHistoryLength, nameof(_countBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushMemory(ushort id, void* source, int count, int elementSize)
        {
            ref var buffer = ref _elementBuffer[id];
            ref var element = ref buffer.elements[buffer.index++];
            if (element.tick != 0)
            {
                element.OnRecycle();
            }

            element.tick = Tick;
            element.count = count;
            element.elementSize = elementSize;
            element.data = (count != 0)
                ? UnsafeMemory.Copy(source, count * elementSize) 
                : IntPtr.Zero.ToPointer();

            HistoryUtils.CheckAndResizeLoopBuffer(ref buffer.index, ref buffer.elements, _recordHistoryLength, nameof(_elementBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RevertTo(uint tick, DArrayStorage subject)
        {
            RevertToRecycledCountBuffer(tick, subject);
            RevertToRecycledBuffer(tick, subject);
            RevertToCountBuffer(tick, subject);
            RevertToElementBuffer(tick, subject);

            subject.RevertFinished();
        }

        public void Dispose()
        {
            for (int i = 1; i < _elementBuffer.Length; ++i)
            {
                _elementBuffer[i].OnRecycle();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledCountBuffer(uint tick, DArrayStorage subject)
        {
            RevertHelper.RevertToRecycledCountBufferClass(subject, tick, _recycledCountBuffer, ref _recycledCountIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToRecycledBuffer(uint tick, DArrayStorage subject)
        {
            RevertHelper.RevertToRecycledBufferClass(subject, tick, _recycledBuffer, ref _recycledIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToCountBuffer(uint tick, DArrayStorage subject)
        {
            RevertHelper.RevertToCountBufferClass(subject, tick, _countBuffer, ref _countIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RevertToElementBuffer(uint tick, DArrayStorage subject)
        {
            RevertToElementBufferClass(subject, tick, _elementBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RevertToElementBufferClass(DArrayStorage subject, uint tick, ElementBufferData[] elementBuffer)
        {
            var elements = subject.GetDenseRaw();

            for (int k = 1; k < elementBuffer.Length; ++k)
            {
                var isNeedContinueSearch = true;
                ref var buffer = ref elementBuffer[k];

                for (int i = buffer.index - 1; i >= 0; --i)
                {
                    ref var frame = ref buffer.elements[i];

                    if (frame.tick <= tick)
                    {
                        elements[k].Replace(frame.data, frame.count, frame.elementSize);

                        frame.tick = 0;
                        buffer.index = k;
                        isNeedContinueSearch = false;
                        break;
                    }
                }

                if (isNeedContinueSearch)
                {
                    for (int i = buffer.elements.Length - 1; i >= buffer.index; --i)
                    {
                        ref var frame = ref buffer.elements[i];

                        if (frame.tick <= tick)
                        {
                            elements[k].Replace(frame.data, frame.count, frame.elementSize);

                            frame.tick = 0;
                            buffer.index = k;
                            break;
                        }
                    }
                }
            }
        }

        public override void Pack(ref WriterContextSerializer writer)
        {
            base.Pack(ref writer);

            writer.Write(_buffersCapacity);

            writer.Write(_recycledCountIndex);
            writer.WriteUnmanagedArray(_recycledCountBuffer);

            writer.Write(_recycledIndex);
            writer.WriteUnmanagedArray(_recycledBuffer);

            writer.Write(_countIndex);
            writer.WriteUnmanagedArray(_countBuffer);

            writer.Pack(_elementBuffer);
        }

        public override void Unpack(ref ReaderContextSerializer reader)
        {
            base.Unpack(ref reader);

            _buffersCapacity = reader.ReadUInt32();

            _recycledCountIndex = reader.ReadInt32();
            _recycledCountBuffer = reader.ReadUnmanagedArray<TickData<ushort>>();

            _recycledIndex = reader.ReadInt32();
            _recycledBuffer = reader.ReadUnmanagedArray<RecycledData<ushort>>();

            _countIndex = reader.ReadInt32();
            _countBuffer = reader.ReadUnmanagedArray<TickData<ushort>>();

            _elementBuffer = reader.Unpack<ElementBufferData[]>();
        }

       

        private struct ElementBufferData : ISerialize
        {
            public int index;
            public ElementData[] elements;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRecycle()
            {
                for(int i = 0; i< elements.Length; ++i)
                {
                    if (elements[i].tick != 0)
                    {
                        elements[i].OnRecycle();
                    }
                }
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(index);
                writer.Pack(elements);
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                index = reader.ReadInt32();
                elements = reader.Unpack<ElementData[]>();
            }
        }

        private struct ElementData : IFrameData, ISerialize
        {
            public uint Tick
                => tick;

            public uint tick;
            public int count;
            public int elementSize;
            public void* data;

            public int ByteLength
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => count * elementSize;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRecycle()
            {
                if ((IntPtr)data != IntPtr.Zero)
                {
                    tick = 0;
                    data = null;
                    UnsafeMemory.Free(data);
                }
            }

            public void Pack(ref WriterContextSerializer writer)
            {
                writer.Write(tick);
                writer.Write(count);
                writer.Write(elementSize);
                writer.WriteStruct(new ArrayPtr(data, (uint)ByteLength));
            }

            public void Unpack(ref ReaderContextSerializer reader)
            {
                tick = reader.ReadUInt32();
                count = reader.ReadInt32();
                elementSize = reader.ReadInt32();
                data = reader.ReadStruct<ArrayPtr>().data;
            }
        }
    }
}