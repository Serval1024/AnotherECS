using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public static class NArrayExtensions
    {
        public static void AddSort<T>(ref this NArray<T> array, uint count, T element)
            where T : unmanaged, IComparable<T>
        {
            int i = array.BinarySearch(0, count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref array, (uint)~i, element);
        }

        public static void Insert<T>(ref this NArray<T> array, uint index, T element)
            where T : unmanaged
        {
            if (index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var newArray = new NArray<T>(array.Length + 1);

            for (uint i = 0; i < index; ++i)
            {
                newArray.GetRef(i) = array.GetRef(i);
            }

            for (uint i = array.Length - 1, iMax = index + 1; i >= iMax; --i)
            {
                newArray.GetRef(i) = array.GetRef(i - 1);
            }
            newArray.GetRef(index) = element;

            array.Replace(ref newArray);
        }

        public static int BinarySearch<T>(ref this NArray<T> array, T value)
            where T : unmanaged, IComparable<T>
            => array.BinarySearch(0, array.Length, value);

        public static int BinarySearch<T>(ref this NArray<T> array, uint index, uint length, T value)
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

                int c = array.Get(i).CompareTo(value);
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

        public static int BinarySearch<T, TOrder>(ref this NArray<T> array, ref TOrder order, T value)
            where T : unmanaged
            where TOrder : struct, IComparer<T>
            => array.BinarySearch(ref order, 0, array.Length, value);

        public static int BinarySearch<T, TOrder>(ref this NArray<T> array, ref TOrder order, uint index, uint length, T value)
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
