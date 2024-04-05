using AnotherECS.Core.Allocators;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core.Collection
{
    public static class NArrayExtensions
    {
        public unsafe static void Sort<TNArray, T>(this ref TNArray narray)
            where TNArray : struct, INArray<T>
            where T : unmanaged, IComparable<T>
        {
            narray.Dirty();
            narray.AsSpan<TNArray, T>().Sort();
        }

        public unsafe static Span<T> AsSpan<TNArray, T>(this ref TNArray narray)
            where TNArray : struct, INArray<T>
            where T : unmanaged
            => AsSpan<TNArray, T>(ref narray, (int)narray.Length);

        public unsafe static Span<T> AsSpan<TNArray, T>(this ref TNArray narray, int count)
           where TNArray : struct, INArray<T>
           where T : unmanaged
           => AsSpan<TNArray, T>(ref narray, 0, count);

        public unsafe static Span<T> AsSpan<TNArray, T>(this ref TNArray narray, int start, int count)
            where TNArray : struct, INArray<T>
            where T : unmanaged
        {
            if (start + count > narray.Length)
            {
                throw new ArgumentException();
            }
            return new(narray.ReadPtr() + start, count);
        }

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

            int lo = (int)index;
            int hi = lo + (int)length - 1;

            while (lo <= hi)
            {
                int i = GetMedian(lo, hi);

                int c = array.ReadRef(i).CompareTo(value);
                if (c == 0)
                {
                    return i;
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
            return ~lo;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int GetMedian(int low, int hi)
                => low + ((hi - low) >> 1);
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

            int lo = (int)index;
            int hi = lo + (int)length - 1;

            while (lo <= hi)
            {
                int i = GetMedian(lo, hi);

                int c = order.Compare(array.ReadRef(i), value);
                if (c == 0)
                {
                    return i;
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
            return ~lo;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int GetMedian(int low, int hi)
                => low + ((hi - low) >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TAllocator, T>(this ref NArray<TAllocator, T> narray)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IDisposable
        {
            DeepDispose<NArray<TAllocator, T>, T>(ref narray, 0, narray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TNArray, T>(this ref TNArray narray)
           where TNArray : struct, INArray<T>
           where T : unmanaged, IDisposable
        {
            DeepDispose<TNArray, T>(ref narray, 0, narray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeepDispose<TNArray, T>(this ref TNArray narray, uint start, uint elementCount)
            where TNArray : struct, INArray<T>
            where T : unmanaged, IDisposable
        {
            for (uint i = start; i < elementCount; ++i)
            {
                narray.ReadRef(i).Dispose();
            }
            narray.Dispose();
        }
    }
}
