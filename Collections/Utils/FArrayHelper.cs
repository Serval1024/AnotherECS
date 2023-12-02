using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using AnotherECS.Serializer;

namespace AnotherECS.Collections
{
    internal static class FArrayHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadSafe<T, U>(ref U data, int index)
            where U : struct
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var value = UnsafeUtility.ReadArrayElement<T>((void*)gcHandle.AddrOfPinnedObject(), index);
            gcHandle.Free();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteSafe<T, U>(ref U data, int index, T value)
            where U : struct
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            UnsafeUtility.WriteArrayElement((void*)gcHandle.AddrOfPinnedObject(), index, value);
            gcHandle.Free();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadUnsafe<T, U>(ref U data, int index)
            where U : struct
            => UnsafeUtility.ReadArrayElement<T>(UnsafeUtility.AddressOf(ref data), index);
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteUnsafe<T, U>(ref U data, int index, T value)
            where U : struct
            => UnsafeUtility.WriteArrayElement(UnsafeUtility.AddressOf(ref data), index, value);

        public static unsafe void Pack<T>(ref WriterContextSerializer writer, GCHandle gcHandle, int count)
            where T : unmanaged
        {
            var ptrT = (T*)gcHandle.AddrOfPinnedObject();

            for (int i = 0; i < count; ++i)
            {
                writer.WriteStruct(*(ptrT + i));
            }

            gcHandle.Free();
        }

        public static unsafe void Unpack<T>(ref ReaderContextSerializer reader, GCHandle gcHandle, int count)
            where T : unmanaged
        {
            var ptrT = (T*)gcHandle.AddrOfPinnedObject();

            for (int i = 0; i < 2; ++i)
            {
                *(ptrT + i) = reader.ReadStruct<T>();
            }

            gcHandle.Free();
        }

        public static void ThrowIfOutOfRange(int index, int length)
        {
            if (index < 0 || index >= length)
            {
                throw new ArgumentOutOfRangeException($"Index: {index}");
            }
        }

        public static void ThrowIfEmpty(int length)
        {
            if (length == 0)
            {
                throw new InvalidOperationException($"Collection is empty.");
            }
        }
    }
}