using AnotherECS.Core.Allocators;
using System;
using System.Collections.Generic;

namespace AnotherECS.Core.Collection
{
    public static class NListExtensions
    {
        public unsafe static void Sort<TAllocator, T>(this ref NList<TAllocator, T> nlist)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
        {
            nlist.Dirty();
            nlist.AsSpan().Sort();
        }

        public unsafe static Span<T> AsSpan<TAllocator, T>(this ref NList<TAllocator, T> nlist)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
            => new(nlist.ReadPtr(), (int)nlist.Count);

        public static void AddSort<TAllocator, T, TOrder>(ref this NList<TAllocator, T> nlist, ref TOrder order, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
            where TOrder : struct, IComparer<T>
        {
            int i = nlist._data.BinarySearch(ref order, 0, nlist.Count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref nlist, (uint)~i, element);
        }

        public static void AddSort<TAllocator, T>(ref this NList<TAllocator, T> nlist, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged, IComparable<T>
        {
            int i = nlist._data.BinarySearch(0, nlist.Count, element);
            if (i >= 0)
            {
                throw new ArgumentException($"Element already added: '{element}'");
            }

            Insert(ref nlist, (uint)~i, element);
        }

        public static void Insert<TAllocator, T>(ref this NList<TAllocator, T> nlist, uint index, T element)
            where TAllocator : unmanaged, IAllocator
            where T : unmanaged
        {
            if (index > nlist.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index == nlist.Count)
            {
                nlist.Add(element);
            }
            else
            {
                nlist.Add(default);

                for (uint i = nlist.Count - 1, iMax = index + 1; i >= iMax; --i)
                {
                    nlist.GetRef(i) = nlist.GetRef(i - 1);
                }
                nlist.GetRef(index) = element;
            }
        }
    }
}
