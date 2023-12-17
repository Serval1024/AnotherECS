using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class LayoutActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TAllocator, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, bool, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => layout.storage.sparse.Get(0) ? 1u : 0u;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => GetSpaceCount(ref layout, startIndex) - layout.storage.recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetSpaceCount<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => layout.storage.denseIndex - startIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResizeDense<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
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
        public static bool TryResizeRecycle<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint capacity)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
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
        public static void CheckDenseLimit<TAllocator, TSparse, TDense, TDenseIndex, TNumber>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TNumber : unmanaged
        {
            if (layout.storage.denseIndex == GetMaxValue<TNumber>())
            {
                throw new Exceptions.ReachedLimitComponentException(GetMaxValue<TNumber>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckLimit<TNumber, TNumberProvider>(uint value)
            where TNumber : unmanaged
        {
            if (value == GetMaxValue<TNumber>())
            {
                throw new Exceptions.ReachedLimitComponentException(GetMaxValue<TNumber>());
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetMaxValue<TNumber>()
            where TNumber : unmanaged
            => Type.GetTypeCode(typeof(TNumber)) switch 
            {
                TypeCode.Boolean => uint.MaxValue,
                TypeCode.Byte => byte.MaxValue,
                TypeCode.UInt16 => ushort.MaxValue,
                TypeCode.UInt32 => uint.MaxValue,
                  _ => throw new ArgumentException(),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SparseClear<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            ref var storage = ref layout.storage;

            if (storage.sparse.IsValide)
            {
                storage.sparse.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StorageClear<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
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
        public static void UpdateVersion<TAllocator, TSparse, TDense, TDenseIndex>(ref UnmanagedLayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint tick, uint count)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            var version = layout.storage.version.GetPtr();
            for (uint i = 0; i < count; ++i)
            {
                version[i] = tick;
            }
        }
    }
}

