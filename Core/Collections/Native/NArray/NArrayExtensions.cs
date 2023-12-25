using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace AnotherECS.Core.Collection
{
    public static class NArrayExtensions
    {
        public static void AddSort<TAllocator, T>(ref this NArray<TAllocator, T> array, uint count, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
        {
            int i = array.BinarySearch(0, count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref array, (uint)~i, element);
        }

        public static unsafe void Insert<TAllocator, T>(ref this NArray<TAllocator, T> array, uint index, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
        {
            if (index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var newArray = new NArray<TAllocator, T>(array.GetAllocator(), array.Length + 1);

            for (uint i = 0; i < index; ++i)
            {
                newArray.ReadRef(i) = array.ReadRef(i);
            }

            for (uint i = array.Length - 1, iMax = index + 1; i >= iMax; --i)
            {
                newArray.ReadRef(i) = array.ReadRef(i - 1);
            }
            newArray.ReadRef(index) = element;

            array.Replace(ref newArray);
        }

        public static int BinarySearch<TNArray, T>(ref this TNArray array, T value)
            where TNArray : struct, INArray<T>
            where T : unmanaged, IComparable<T>
            => array.BinarySearch(0, array.Length, value);

        public static int BinarySearch<TNArray, T>(ref this TNArray array, uint index, uint length, T value)
            where TNArray : struct, INArray<T>
            where T : unmanaged, IComparable<T>
        {
            if (array.Length - index < length)
            {
                throw new ArgumentException();
            }

            uint lo = index;
            uint hi = index + length - 1;

            while (lo <= hi)
            {
                uint i = GetMedian(lo, hi);

                int c = array.ReadRef(i).CompareTo(value);
                if (c == 0)
                {
                    return (int)i;
                }
                else if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
            return ~(int)lo;
        }

        public static int BinarySearch<TNArray, T, TOrder>(ref this TNArray array, ref TOrder order, T value)
            where TNArray : struct, INArray<T>
            where T : unmanaged
            where TOrder : struct, IComparer<T>
            => array.BinarySearch(ref order, 0, array.Length, value);

        public static int BinarySearch<TNArray, T, TOrder>(ref this TNArray array, ref TOrder order, uint index, uint length, T value)
            where TNArray : struct, INArray<T>
            where T : unmanaged
            where TOrder : struct, IComparer<T>
        {
            if (array.Length - index < length)
            {
                throw new ArgumentException();
            }

            uint lo = index;
            uint hi = index + length - 1;

            while (lo <= hi)
            {
                uint i = GetMedian(lo, hi);

                int c = order.Compare(array.Get(i), value);
                if (c == 0)
                {
                    return (int)i;
                }
                else if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }
            return ~(int)lo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetMedian(uint low, uint hi)
            => low + ((hi - low) >> 1);
    }
}
