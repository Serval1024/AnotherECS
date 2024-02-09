using AnotherECS.Core.Allocators;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Actions
{
    internal static unsafe class LayoutActions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TAllocator, TDense, TDenseIndex>(ref ULayout<TAllocator, bool, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => layout.sparse.Get(0) ? 1u : 0u;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetCount<TAllocator, TSparse, TDense, TDenseIndex>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => GetSpaceCount(ref layout, startIndex) - layout.recycleIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetSpaceCount<TAllocator, TSparse, TDense, TDenseIndex>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout, uint startIndex)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            => layout.denseIndex - startIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckDenseLimit<TAllocator, TSparse, TDense, TDenseIndex, TNumber>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
            where TNumber : unmanaged
        {
            if (layout.denseIndex == GetMaxValue<TNumber>())
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
        public static void SparseClear<TAllocator, TSparse, TDense, TDenseIndex>(ref ULayout<TAllocator, TSparse, TDense, TDenseIndex> layout)
            where TAllocator : unmanaged, IAllocator
            where TSparse : unmanaged
            where TDense : unmanaged
            where TDenseIndex : unmanaged
        {
            if (layout.sparse.IsValid)
            {
                layout.sparse.Clear();
            }
        }
    }
}

