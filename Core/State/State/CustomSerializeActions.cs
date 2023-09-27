using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class CustomSerializeActions<T>
        where T : unmanaged, ISerialize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref UnmanagedLayout<T> layout, HistoryMode historyMode)
        {
            layout.storage.PackCommon(ref writer);
            ArrayPtrEachTStaticSerializer<T>.Pack(ref writer, ArrayPtr<T>.CreateWrapper(layout.storage.dense));

            if (historyMode != HistoryMode.NONE)
            {
                layout.history.PackCommon(ref writer);

                HistroyArrayPtrEachTStaticSerializer<T>.Pack(ref writer, layout.history.denseBuffer, historyMode);    
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(ref ReaderContextSerializer reader, ref UnmanagedLayout<T> layout, HistoryMode historyMode)
        {
            layout.storage.UnpackCommon(ref reader);
            layout.storage.dense = ArrayPtr.CreateWrapper(ArrayPtrEachTStaticSerializer<T>.Unpack(ref reader));

            if (historyMode != HistoryMode.NONE)
            {
                layout.history.UnpackCommon(ref reader);

                layout.history.denseBuffer = HistroyArrayPtrEachTStaticSerializer<T>.Unpack(ref reader, historyMode);
            }
        }
    }

    internal unsafe class ArrayPtrEachTStaticSerializer<T>
        where T : unmanaged, ISerialize
    {
        private static readonly CountMeta _сount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ArrayPtr<T> arrayPtr)
        {
            _сount.Pack(ref writer, arrayPtr.ByteLength);
            _сount.Pack(ref writer, arrayPtr.ElementCount);
            var ptr = arrayPtr.GetPtr();
            for (uint i = 0; i < arrayPtr.ElementCount; i++)
            {
                ptr[i].Pack(ref writer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr<T> Unpack(ref ReaderContextSerializer reader)
        {
            uint length = _сount.Unpack(ref reader);
            uint count = _сount.Unpack(ref reader);
            var buffer = (T*)UnsafeMemory.Malloc(length);
            T element = default;
            for (uint i = 0; i < count; i++)
            {
                element.Unpack(ref reader);
                buffer[i] = element;
            }
            return new ArrayPtr<T>(buffer, count);
        }
    }

    internal unsafe class HistroyArrayPtrEachTStaticSerializer<T>
        where T : unmanaged, ISerialize
    {
        private static readonly CountMeta _сount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ArrayPtr arrayPtr, HistoryMode historyMode)
        {
            _сount.Pack(ref writer, arrayPtr.ByteLength);
            _сount.Pack(ref writer, arrayPtr.ElementCount);

            if (historyMode == HistoryMode.BYCHANGE)
            {
                var ptr = arrayPtr.GetPtr<TickOffsetData<T>>();
                for (uint i = 0; i < arrayPtr.ElementCount; i++)
                {
                    TickOffsetDataSerializer<T>.Pack(ref writer, ref ptr[i]);
                }
            }
            else if(historyMode == HistoryMode.BYTICK)
            {
                var ptr = arrayPtr.GetPtr<TickDataPtr<T>>();
                for (uint i = 0; i < arrayPtr.ElementCount; i++)
                {
                    TickDataPtrSerializer<T>.Pack(ref writer, ref ptr[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr Unpack(ref ReaderContextSerializer reader, HistoryMode historyMode)
        {
            uint length = _сount.Unpack(ref reader);
            uint count = _сount.Unpack(ref reader);

            if (historyMode == HistoryMode.BYCHANGE)
            {
                var buffer = (TickOffsetData<T>*)UnsafeMemory.Malloc(length);
                for (uint i = 0; i < count; i++)
                {
                    buffer[i] = TickOffsetDataSerializer<T>.Unpack(ref reader);
                }
                return new ArrayPtr(buffer, length, count);
            }
            else if (historyMode == HistoryMode.BYTICK)
            {
                var buffer = (TickDataPtr<T>*)UnsafeMemory.Malloc(length);
                for (uint i = 0; i < count; i++)
                {
                    buffer[i] = TickDataPtrSerializer<T>.Unpack(ref reader);
                }
                return new ArrayPtr(buffer, length, count);
            }
            return default;
        }
    }

    internal class TickOffsetDataSerializer<T>
           where T : struct, ISerialize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref TickOffsetData<T> data)
        {
            writer.Write(data.tick);
            writer.Write(data.offset);
            data.value.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TickOffsetData<T> Unpack(ref ReaderContextSerializer reader)
        {
            TickOffsetData<T> data;

            data.tick = reader.ReadUInt32();
            data.offset = reader.ReadUInt32();
            data.value = default;
            data.value.Unpack(ref reader);
            
            return data;
        }
    }

    internal class TickDataPtrSerializer<T>
          where T : unmanaged, ISerialize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref TickDataPtr<T> data)
        {
            writer.Write(data.tick);
            ArrayPtrEachTStaticSerializer<T>.Pack(ref writer, ArrayPtr<T>.CreateWrapper(data.value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TickDataPtr<T> Unpack(ref ReaderContextSerializer reader)
        {
            TickDataPtr<T> data;

            data.tick = reader.ReadUInt32();
            data.value = ArrayPtr.CreateWrapper(ArrayPtrEachTStaticSerializer<T>.Unpack(ref reader));

            return data;
        }
    }
}

