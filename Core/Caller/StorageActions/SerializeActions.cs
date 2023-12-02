using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
 
namespace AnotherECS.Core.Actions
{
    internal static unsafe class LayoutSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackBlittable(ref WriterContextSerializer writer, ref UnmanagedLayout layout)
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref layout.storage);
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref layout.history);
            layout.storage.dense.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackBlittable(ref ReaderContextSerializer reader, ref UnmanagedLayout layout)
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref layout.storage);
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref layout.history);
            layout.storage.dense.Unpack(ref reader);
        }
    }

    internal static unsafe class PartialLayoutSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommonBlittable(ref WriterContextSerializer writer, ref ComponetStorage storage)
        {
            storage.sparse.Pack(ref writer);
            storage.version.Pack(ref writer);
            storage.recycle.Pack(ref writer);

            writer.Write(storage.denseIndex);
            writer.Write(storage.recycleIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommonBlittable(ref WriterContextSerializer writer, ref HistoryStorage history)
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
        public static void UnpackCommonBlittable(ref ReaderContextSerializer reader, ref ComponetStorage storage)
        {
            storage.sparse.Unpack(ref reader);
            storage.version.Unpack(ref reader);
            storage.recycle.Unpack(ref reader);

            storage.denseIndex = reader.ReadUInt32();
            storage.recycleIndex = reader.ReadUInt32();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackCommonBlittable(ref ReaderContextSerializer reader, ref HistoryStorage history)
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

    internal static unsafe class SerializeActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageBlittable<TSparse, TDense, TDenseIndex, TTickData>
            (ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref unknowLayout->storage);

            unknowLayout->storage.dense.Pack(ref writer);           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageSerialize<TSparse, TDense, TDenseIndex, TTickData>
            (ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref unknowLayout->storage);

            NArrayEachSerializeStaticSerializer<TDense>.Pack(ref writer, ref layout->storage.dense, layout->storage.denseIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackHistoryBlittable<TSparse, TDense, TDenseIndex, TTickData>
            (ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref unknowLayout->history);

            unknowLayout->history.denseBuffer.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackHistorySerialize<TSparse, TDense, TDenseIndex, TTickData>
            (ref WriterContextSerializer writer, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ISerialize
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref unknowLayout->history);

            NArrayEachSerializeStaticSerializer<TTickData>.Pack(ref writer, ref layout->history.denseBuffer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageBlittable<TSparse, TDense, TDenseIndex, TTickData>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref unknowLayout->storage);

            unknowLayout->storage.dense.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageSerialize<TSparse, TDense, TDenseIndex, TTickData>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref unknowLayout->storage);

            NArrayEachSerializeStaticSerializer<TDense>.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackHistoryBlittable<TSparse, TDense, TDenseIndex, TTickData>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref unknowLayout->history);

            unknowLayout->history.denseBuffer.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackHistorySerialize<TSparse, TDense, TDenseIndex, TTickData>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData>* layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged, ISerialize
        {
            var unknowLayout = (UnmanagedLayout*)layout;
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref unknowLayout->history);

            NArrayEachSerializeStaticSerializer<TTickData>.Unpack(ref reader);
        }
    }
}

