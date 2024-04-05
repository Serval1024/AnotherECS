using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace AnotherECS.Collections
{
    internal static class FArrayHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadSafe<T, U>(ref U data, uint index)
            where T : struct
            where U : struct
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var value = UnsafeUtility.ReadArrayElement<T>((void*)gcHandle.AddrOfPinnedObject(), (int)index);
            gcHandle.Free();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteSafe<T, U>(ref U data, uint index, T value)
            where T : struct
            where U : struct
        {
            var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            UnsafeUtility.WriteArrayElement((void*)gcHandle.AddrOfPinnedObject(), (int)index, value);
            gcHandle.Free();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadUnsafe<T, U>(ref U data, uint index)
            where T : struct
            where U : struct
            => UnsafeMemory.ReadElementArray<T>(UnsafeUtils.AddressOf(ref data), index);
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteUnsafe<T, U>(ref U data, uint index, T value)
            where T : struct
            where U : struct
            => UnsafeUtility.WriteArrayElement(UnsafeUtility.AddressOf(ref data), (int)index, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref T GetUnsafe<T, U>(ref U data, uint index)
            where T : unmanaged
            where U : struct
            => ref UnsafeMemory.GetElementArray<T>(UnsafeUtils.AddressOf(ref data), index);

        public static unsafe void Pack<T>(ref WriterContextSerializer writer, GCHandle gcHandle, uint count)
            where T : unmanaged
        {
            var ptrT = (T*)gcHandle.AddrOfPinnedObject();

            for (uint i = 0; i < count; ++i)
            {
                writer.WriteStruct(*(ptrT + i));
            }

            gcHandle.Free();
        }

        public static unsafe void Unpack<T>(ref ReaderContextSerializer reader, GCHandle gcHandle, uint count)
            where T : unmanaged
        {
            var ptrT = (T*)gcHandle.AddrOfPinnedObject();

            for (uint i = 0; i < count; ++i)
            {
                *(ptrT + i) = reader.ReadStruct<T>();
            }

            gcHandle.Free();
        }

        public static void ThrowIfOutOfRange(uint index, uint length)
        {
            if (index < 0 || index >= length)
            {
                throw new ArgumentOutOfRangeException($"Index: {index}");
            }
        }

        public static void ThrowIfEmpty(uint length)
        {
            if (length == 0)
            {
                throw new InvalidOperationException($"Collection is empty.");
            }
        }
    }
}