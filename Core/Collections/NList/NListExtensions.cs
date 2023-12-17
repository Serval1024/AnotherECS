using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Collection
{
    public static class NListExtensions
    {
        public static void AddSort<TAllocator, T, TOrder>(ref this NList<TAllocator, T> list, ref TOrder order, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
            where TOrder : struct, IComparer<T>
        {
            int i = list._data.BinarySearch(ref order, 0, list.Count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref list, (uint)~i, element);
        }

        public static void AddSort<TAllocator, T>(ref this NList<TAllocator, T> list, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
        {
            int i = list._data.BinarySearch(0, list.Count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref list, (uint)~i, element);
        }

        public static void Insert<TAllocator, T>(ref this NList<TAllocator, T> list, uint index, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
        {
            if (index > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index == list.Count)
            {
                list.Add(element);
            }
            else
            {
                list.Add(default);

                for (uint i = list.Count - 1, iMax = index + 1; i >= iMax; --i)
                {
                    list.GetRef(i) = list.GetRef(i - 1);
                }
                list.GetRef(index) = element;
            }
        }
    }
}
