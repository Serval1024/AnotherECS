using System.Runtime.CompilerServices;
using AnotherECS.Serializer;
 
namespace AnotherECS.Core.Actions
{
    internal static unsafe class SerializeActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageBlittable<TAllocator>
            (ref WriterContextSerializer writer, GenerationULayout<TAllocator>* layout)
            where TAllocator : unmanaged, IAllocator
        {
            layout->generation.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref *layout);

            layout->dense.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorageSerialize<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref *layout);

            NArrayEachSerializeStaticSerializer<TAllocator, TDense>.Pack(ref writer, ref layout->dense, layout->denseIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackStorage<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.PackCommonBlittable(ref writer, ref *layout);

            layout->dense.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageBlittable<TAllocator>
            (ref ReaderContextSerializer reader, GenerationULayout<TAllocator>* layout)
            where TAllocator : unmanaged, IAllocator
        {
            layout->generation.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref *layout);

            layout->dense.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorageSerialize<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged, ISerialize
            where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref *layout);

            NArrayEachSerializeStaticSerializer<TAllocator, TDense>.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackStorage<TAllocator, TSparse, TDense, TDenseIndex>
          (ref ReaderContextSerializer reader, ULayout<TAllocator, TSparse, TDense, TDenseIndex>* layout)
          where TAllocator : unmanaged, IAllocator
          where TSparse : unmanaged
          where TDense : unmanaged
          where TDenseIndex : unmanaged
        {
            PartialLayoutSerializer.UnpackCommonBlittable(ref reader, ref *layout);

            layout->dense.Unpack(ref reader);
        }
    }

    internal static unsafe class PartialLayoutSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PackCommonBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref WriterContextSerializer writer, ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> storage)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            storage.sparse.PackBlittable(ref writer);
            storage.recycle.PackBlittable(ref writer);
            storage.tickVersion.PackBlittable(ref writer);

            writer.Write(storage.denseIndex);
            writer.Write(storage.recycleIndex);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnpackCommonBlittable<TAllocator, TSparse, TDense, TDenseIndex>
            (ref ReaderContextSerializer reader, ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> storage)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            storage.sparse.UnpackBlittable(ref reader);
            storage.recycle.UnpackBlittable(ref reader);
            storage.tickVersion.UnpackBlittable(ref reader);

            storage.denseIndex = reader.ReadUInt32();
            storage.recycleIndex = reader.ReadUInt32();
        }
    }

}

