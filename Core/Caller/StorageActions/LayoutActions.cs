using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class LayoutActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<bool, TDense, TDenseIndex, TTickData> layout)
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
            => layout.storage.sparse.Get(0) ? (uint)1 : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
            => GetSpaceCount(ref layout, startIndex) - layout.storage.recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetSpaceCount<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint startIndex)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
            => layout.storage.denseIndex - startIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            ref var storage = ref layout.storage;
            if (storage.denseIndex == storage.dense.Length)
            {
                layout.storage.dense.Resize(capacity);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeRecycle<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint capacity)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            ref var storage = ref layout.storage;
            if (storage.recycleIndex == storage.recycle.Length)
            {
                layout.storage.recycle.Resize(capacity);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckDenseLimit<TSparse, TDense, TDenseIndex, TTickData, RNumber>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
            where RNumber : unmanaged
        {
            if (layout.storage.denseIndex == GetMaxValue<RNumber>())
            {
                throw new Exceptions.ReachedLimitComponentException(GetMaxValue<RNumber>());
            }
        }

        public static uint GetMaxValue<QNumber>()
          where QNumber : unmanaged
          => Type.GetTypeCode(typeof(QNumber)) switch
          {
              TypeCode.Boolean => uint.MaxValue,
              TypeCode.Byte => byte.MaxValue,
              TypeCode.UInt16 => ushort.MaxValue,
              TypeCode.UInt32 => uint.MaxValue,
              _ => throw new ArgumentException(),
          };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SparseClear<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
           where TSparse : unmanaged
           where TDense : unmanaged
           where TDenseIndex : unmanaged
           where TTickData : unmanaged
        {
            ref var storage = ref layout.storage;

            if (storage.sparse.IsValide)
            {
                storage.sparse.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StorageClear<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            ref var storage = ref layout.storage;

            if (storage.sparse.IsValide)
            {
                storage.sparse.Clear();
            }
            if (storage.version.IsValide)
            {
                storage.version.Clear();
            }
            if (storage.recycle.IsValide)
            {
                storage.recycle.Clear();
            }
            if (storage.dense.IsValide)
            {
                storage.dense.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateVersion<TSparse, TDense, TDenseIndex, TTickData>(ref UnmanagedLayout<TSparse, TDense, TDenseIndex, TTickData> layout, uint tick, uint count)
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TTickData : unmanaged
        {
            var version = layout.storage.version.GetPtr();
            for (uint i = 0; i < count; ++i)
            {
                version[i] = tick;
            }
        }
    }
}

