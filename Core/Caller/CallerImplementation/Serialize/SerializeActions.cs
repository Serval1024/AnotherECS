using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
 
namespace AnotherECS.Core.Actions
{
    internal static unsafe class SerializeActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref layout->storage);

            layout->storage.dense.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageSerialize<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref layout->storage);

            NArrayEachSerializeStaticSerializer<TAllocator, TDense>.Pack(ref writer, ref layout->storage.dense, layout->storage.denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorage<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref layout->storage);

            layout->storage.dense.Pack(ref writer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref layout->storage);

            layout->storage.dense.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageSerialize<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref layout->storage);

            NArrayEachSerializeStaticSerializer<TAllocator, TDense>.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorage<TAllocator, TSparse, TDense, TDenseIndex>
          (ref ReaderContextSerializer reader, UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
          where TAllocator : unmanaged, IAllocator
          where TSparse : unmanaged
          where TDense : unmanaged
          where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref layout->storage);

            layout->storage.dense.Unpack(ref reader);
        }
    }

    internal static unsafe class PartialLayoutSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommonBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, ref ComponentStorage<TAllocator, TSparse, TDense, TDenseIndex> storage)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            storage.sparse.Pack(ref writer);
            storage.recycle.Pack(ref writer);
            storage.tickVersion.Pack(ref writer);
            storage.addRemoveVersion.Pack(ref writer);

            writer.Write(storage.denseIndex);
            writer.Write(storage.recycleIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackCommonBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, ref ComponentStorage<TAllocator, TSparse, TDense, TDenseIndex> storage)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            storage.sparse.Unpack(ref reader);
            storage.recycle.Unpack(ref reader);
            storage.tickVersion.Unpack(ref reader);
            storage.addRemoveVersion.Unpack(ref reader);

            storage.denseIndex = reader.ReadUInt32();
            storage.recycleIndex = reader.ReadUInt32();
        }
    }

}

