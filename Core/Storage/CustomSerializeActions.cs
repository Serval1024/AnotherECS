using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class LayoutSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref UnmanagedLayout layout)
        {
            ref var storage = ref layout.storage;

            storage.sparse.Pack(ref writer);
            storage.dense.Pack(ref writer);
            storage.version.Pack(ref writer);
            storage.recycle.Pack(ref writer);

            writer.Write(storage.denseIndex);
            writer.Write(storage.recycleIndex);

            ref var history = ref layout.history;

            history.recycleCountBuffer.Pack(ref writer);
            history.recycleBuffer.Pack(ref writer);
            history.countBuffer.Pack(ref writer);
            history.denseBuffer.Pack(ref writer);
            history.sparseBuffer.Pack(ref writer);
            history.versionIndexer.Pack(ref writer);

            writer.Write(history.recycleCountIndex);
            writer.Write(history.recycleIndex);
            writer.Write(history.countIndex);
            writer.Write(history.denseIndex);
            writer.Write(history.sparseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(ref ReaderContextSerializer reader, ref UnmanagedLayout layout)
        {
            ref var storage = ref layout.storage;

            storage.sparse.Unpack(ref reader);
            storage.dense.Unpack(ref reader);
            storage.version.Unpack(ref reader);
            storage.recycle.Unpack(ref reader);

            storage.denseIndex = reader.ReadUInt32();
            storage.recycleIndex = reader.ReadUInt32();

            ref var history = ref layout.history;

            history.recycleCountBuffer.Unpack(ref reader);
            history.recycleBuffer.Unpack(ref reader);
            history.countBuffer.Unpack(ref reader);
            history.denseBuffer.Unpack(ref reader);
            history.sparseBuffer.Unpack(ref reader);
            history.versionIndexer.Unpack(ref reader);

            history.recycleCountIndex = reader.ReadUInt32();
            history.recycleIndex = reader.ReadUInt32();
            history.countIndex = reader.ReadUInt32();
            history.denseIndex = reader.ReadUInt32();
            history.sparseIndex = reader.ReadUInt32();
        }
    }

    internal static unsafe class LayoutSerializer<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommon(ref WriterContextSerializer writer, ref ComponetStorage storage)
        {
            storage.sparse.Pack(ref writer);
            storage.version.Pack(ref writer);
            storage.recycle.Pack(ref writer);

            writer.Write(storage.denseIndex);
            writer.Write(storage.recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommon(ref WriterContextSerializer writer, ref HistoryStorage history)
        {
            history.recycleCountBuffer.Pack(ref writer);
            history.recycleBuffer.Pack(ref writer);
            history.countBuffer.Pack(ref writer);
            history.sparseBuffer.Pack(ref writer);
            history.versionIndexer.Pack(ref writer);

            writer.Write(history.recycleCountIndex);
            writer.Write(history.recycleIndex);
            writer.Write(history.countIndex);
            writer.Write(history.denseIndex);
            writer.Write(history.sparseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackCommon(ref ReaderContextSerializer reader, ref ComponetStorage storage)
        {
            storage.sparse.Unpack(ref reader);
            storage.version.Unpack(ref reader);
            storage.recycle.Unpack(ref reader);

            storage.denseIndex = reader.ReadUInt32();
            storage.recycleIndex = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackCommon(ref ReaderContextSerializer reader, ref HistoryStorage history)
        {
            history.recycleCountBuffer.Unpack(ref reader);
            history.recycleBuffer.Unpack(ref reader);
            history.countBuffer.Unpack(ref reader);
            history.sparseBuffer.Unpack(ref reader);
            history.versionIndexer.Unpack(ref reader);

            history.recycleCountIndex = reader.ReadUInt32();
            history.recycleIndex = reader.ReadUInt32();
            history.countIndex = reader.ReadUInt32();
            history.denseIndex = reader.ReadUInt32();
            history.sparseIndex = reader.ReadUInt32();
        }
    }

    internal static unsafe class NonBlittableSerializeActions<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref UnmanagedLayout<T> layout, HistoryMode historyMode, uint count, bool isStorageBlittable)
        {
            LayoutSerializer<T>.PackCommon(ref writer, ref layout.storage);

            if (isStorageBlittable)
            {
                layout.storage.dense.Pack(ref writer);
            }
            else
            {
                ArrayPtr<T>.CreateWrapper(ref layout.storage.dense).Pack(ref writer);
            }
            if (historyMode != HistoryMode.NONE)
            {
                LayoutSerializer<T>.PackCommon(ref writer, ref layout.history);

                HistroyArrayPtrEachNonBlittableStaticSerializer<T>.Pack(ref writer, layout.history.denseBuffer, historyMode, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(ref ReaderContextSerializer reader, ref UnmanagedLayout<T> layout, HistoryMode historyMode, bool isStorageBlittable)
        {
            LayoutSerializer<T>.UnpackCommon(ref reader, ref layout.storage);

            if (isStorageBlittable)
            {
                layout.storage.dense.Unpack(ref reader);
            }
            else
            {
                ArrayPtr<T> array = default;
                array.Unpack(ref reader);
                layout.storage.dense = ArrayPtr.CreateWrapper(ref array);
            }

            if (historyMode != HistoryMode.NONE)
            {
                LayoutSerializer<T>.UnpackCommon(ref reader, ref layout.history);

                layout.history.denseBuffer = HistroyArrayPtrEachNonBlittableStaticSerializer<T>.Unpack(ref reader, historyMode);
            }
        }
    }

    internal unsafe class HistroyArrayPtrEachNonBlittableStaticSerializer<T>
        where T : unmanaged
    {
        private static readonly ArrayPtrMeta _meta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ArrayPtr arrayPtr, HistoryMode historyMode, uint count)
        {
            _meta.Pack(ref writer, ref arrayPtr);

            if (historyMode == HistoryMode.BYCHANGE)
            {
                var ptr = arrayPtr.GetPtr<TickOffsetData<T>>();
                for (uint i = 0; i < arrayPtr.ElementCount; i++)
                {
                    ptr[i].Pack(ref writer);
                }
            }
            else if (historyMode == HistoryMode.BYTICK)
            {
                var ptr = arrayPtr.GetPtr<TickDataPtr<T>>();
                for (uint i = 0; i < arrayPtr.ElementCount; i++)
                {
                    TickDataPtrNonBlittableSerializer<T>.Pack(ref writer, ref ptr[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr Unpack(ref ReaderContextSerializer reader, HistoryMode historyMode)
        {
            (uint byteLength, uint elementCount) = _meta.Unpack(ref reader);

            if (historyMode == HistoryMode.BYCHANGE)
            {
                var buffer = (TickOffsetData<T>*)UnsafeMemory.Malloc(byteLength);
                for (uint i = 0; i < elementCount; i++)
                {
                    buffer[i].Unpack(ref reader);
                }
                return new ArrayPtr(buffer, byteLength, elementCount);
            }
            else if (historyMode == HistoryMode.BYTICK)
            {
                var buffer = (TickDataPtr<T>*)UnsafeMemory.Malloc(byteLength);
                for (uint i = 0; i < elementCount; i++)
                {
                    buffer[i] = TickDataPtrNonBlittableSerializer<T>.Unpack(ref reader);
                }
                return new ArrayPtr(buffer, byteLength, elementCount);
            }
            return default;     //TODO SER BYVERSION?
        }
    }

    internal class TickDataPtrNonBlittableSerializer<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref TickDataPtr<T> data)
        {
            writer.Write(data.tick);
            var arrayPtr = ArrayPtr<T>.CreateWrapper(ref data.value);
            arrayPtr.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TickDataPtr<T> Unpack(ref ReaderContextSerializer reader)
        {
            TickDataPtr<T> data;

            data.tick = reader.ReadUInt32();
            ArrayPtr<T> array = default;
            array.Unpack(ref reader);
            data.value = ArrayPtr.CreateWrapper(ref array);

            return data;
        }
    }




    internal static unsafe class CustomSerializeActions<T>
        where T : unmanaged, ISerialize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref UnmanagedLayout<T> layout, HistoryMode historyMode, uint count)
        {
            LayoutSerializer<T>.PackCommon(ref writer, ref layout.storage);
            var arrayPtr = ArrayPtr<T>.CreateWrapper(ref layout.storage.dense);
            ArrayPtrEachSerializeStaticSerializer<T>.Pack(ref writer, ref arrayPtr, count);

            if (historyMode != HistoryMode.NONE)
            {
                LayoutSerializer<T>.PackCommon(ref writer, ref layout.history);

                HistroyArrayPtrEachSerializeStaticSerializer<T>.Pack(ref writer, layout.history.denseBuffer, historyMode);    
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(ref ReaderContextSerializer reader, ref UnmanagedLayout<T> layout, HistoryMode historyMode)
        {
            LayoutSerializer<T>.UnpackCommon(ref reader, ref layout.storage);
            var array = ArrayPtrEachSerializeStaticSerializer<T>.Unpack(ref reader);
            layout.storage.dense = ArrayPtr.CreateWrapper(ref array);

            if (historyMode != HistoryMode.NONE)
            {
                LayoutSerializer<T>.UnpackCommon(ref reader, ref layout.history);

                layout.history.denseBuffer = HistroyArrayPtrEachSerializeStaticSerializer<T>.Unpack(ref reader, historyMode);
            }
        }
    }

    internal unsafe class HistroyArrayPtrEachSerializeStaticSerializer<T>
        where T : unmanaged, ISerialize
    {
        private static readonly ArrayPtrMeta _meta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ArrayPtr arrayPtr, HistoryMode historyMode)
        {
            _meta.Pack(ref writer, ref arrayPtr);
            
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
            (uint byteLength, uint elementCount) = _meta.Unpack(ref reader);

            if (historyMode == HistoryMode.BYCHANGE)
            {
                var buffer = (TickOffsetData<T>*)UnsafeMemory.Malloc(byteLength);
                for (uint i = 0; i < elementCount; i++)
                {
                    buffer[i] = TickOffsetDataSerializer<T>.Unpack(ref reader);
                }
                return new ArrayPtr(buffer, byteLength, elementCount);
            }
            else if (historyMode == HistoryMode.BYTICK)
            {
                var buffer = (TickDataPtr<T>*)UnsafeMemory.Malloc(byteLength);
                for (uint i = 0; i < elementCount; i++)
                {
                    buffer[i] = TickDataPtrSerializer<T>.Unpack(ref reader);
                }
                return new ArrayPtr(buffer, byteLength, elementCount);
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
            var arrayPtr = ArrayPtr<T>.CreateWrapper(ref data.value);
            ArrayPtrEachSerializeStaticSerializer<T>.Pack(ref writer, ref arrayPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TickDataPtr<T> Unpack(ref ReaderContextSerializer reader)
        {
            TickDataPtr<T> data;

            data.tick = reader.ReadUInt32();
            var array = ArrayPtrEachSerializeStaticSerializer<T>.Unpack(ref reader);
            data.value = ArrayPtr.CreateWrapper(ref array);

            return data;
        }
    }
}

