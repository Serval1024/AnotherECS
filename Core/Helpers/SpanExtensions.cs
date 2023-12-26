using System;
using AnotherECS.Core.Collection;
using Unity.Collections;

namespace AnotherECS.Core
{
    public static class SpanExtensions
    {
        public static int TryAddSort<T>(this Span<T> span, int count, T element)
            where T : struct, IComparable<T>
        {
            int i = span[..count].BinarySearch(element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }
            return TryInsert(span, count, ~i, element);
        }

        public static int TryInsert<T>(this Span<T> span, int count, int index, T element)
        {
            if (index >= 0 && index < span.Length)
            {
                span[index..^1].CopyTo(span[(index + 1)..]);
                span[index] = element;

                return Math.Min(count + 1, span.Length);
            }

            return count;
        }

        public static bool Contains<T>(this Span<T> span, T element)
            where T : IEquatable<T>
            => span.IndexOf(element) != -1;

        public static bool SortContains<T>(this Span<T> span, T element)
            where T : IEquatable<T>, IComparable<T>
            => span.BinarySearch(element) >= 0;

        public static unsafe NArray<TAllocator, T> ToNArray<TAllocator, T>(this Span<T> span, TAllocator* allocator)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
            => ToNArray(span, allocator, (uint)span.Length);

        public static unsafe NArray<TAllocator, T> ToNArray<TAllocator, T>(this Span<T> span, TAllocator* allocator, uint count)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
        {
            if (count > span.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var result = new NArray<TAllocator, T>(allocator, count);
            for(int i = 0; i < count; ++i)
            {
                result.GetRef(i) = span[i];
            }

            return result;
        }

        public static void Sort<T>(this Span<T> array)
            where T : struct, IComparable<T>
        {
            Sort(array, 0, array.Length);
        }

        public static void Sort<T>(this Span<T> array, int start, int count)
            where T : struct, IComparable<T>
        {
            QSort(array, start, count - start - 1);
        }

        private static void Swap<T>(Span<T> array, int i, int j)
        {
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        private static int Partition<T>(Span<T> array, int from, int to, int pivot)
            where T : struct, IComparable<T>
        {
            var arrayPivot = array[pivot];
            Swap(array, pivot, to - 1);
            var newPivot = from;

            for (int i = from; i < to - 1; i++)
            {
                if (array[i].CompareTo(arrayPivot) != 1)
                {
                    Swap(array, newPivot, i);
                    newPivot++;
                }
            }

            Swap(array, newPivot, to - 1);
            return newPivot;
        }

        private static void QSort<T>(Span<T> array, int from, int to)
            where T : struct, IComparable<T>
        {
            if (to == from)
            {
                return;
            }
            else
            {
                int pivot = from + (to - from) / 2;
                pivot = Partition(array, from, to, pivot);

                QSort(array, from, pivot);
                QSort(array, pivot + 1, to);
            }
        }

    }
}