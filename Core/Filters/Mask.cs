using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;

namespace AnotherECS.Core
{
    internal unsafe struct Mask : IHash<Mask, ulong>, IEquatable<Mask>
    {
        public Items includes;
        public Items excludes;

        private ulong _hash;

        public bool IsValide
            => includes.count != 0;

        public ulong Hash
        {
            get
            {
                if (_hash == 0)
                {
                    _hash = GetHash();
                }
                return _hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInclude(ushort item)
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new Exceptions.ComponentAlreadyAddedMaskException(item);
            }
#endif
            includes.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddExclude(ushort item)
        {
#if !ANOTHERECS_RELEASE
            if (Contains(item))
            {
                throw new Exceptions.ComponentAlreadyAddedMaskException(item);
            }
#endif
            excludes.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong GetHash()
            => includes.GetHash() ^ excludes.GetHash();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ulong IHash<Mask, ulong>.GetHash(ref Mask key)
            => key.Hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Mask other)
            => includes.Equals(other.includes) && excludes.Equals(other.excludes);

        public bool Contains(ushort item)
            => includes.Contains(item) || excludes.Contains(item);


        public unsafe struct Items: IEquatable<Items>
        {
            public const int FILTER_COMPONENT_MAX = 8;

            public byte count;
            public fixed ushort values[FILTER_COMPONENT_MAX];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(ushort item)
            {
#if !ANOTHERECS_RELEASE
                if (count == FILTER_COMPONENT_MAX)
                {
                    throw new InvalidOperationException();
                }
#endif
                count = (byte)CapacityValuesAsSpan().TryAddSort(count, item);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(ushort item)
                => BinarySearch(item);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool BinarySearch(ushort item)
            {
                if (count <= 4)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (values[i] == item)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    int left = 0;
                    int right = count - 1;
                    while (left <= right)
                    {
                        int middle = (left + right) / 2;
                        var value = values[middle];
                        if (value == item)
                        {
                            return true;
                        }
                        else if (value > middle)
                        {
                            left = middle + 1;
                        }
                        else
                        {
                            right = middle - 1;
                        }
                    }
                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<ushort> ValuesAsSpan()
                => new(UnsafeUtils.AddressOf(ref values[0]), count);

            public ulong GetHash()
            {
                ulong hash = count;
                for(int i = 0; i < FILTER_COMPONENT_MAX; ++i)
                {
                    hash = unchecked(hash * 314159 + values[i]);       
                }
                return hash;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Items other)
            {
                if (count != other.count)
                {
                    return false;
                }

                for (int i = 0; i < count; ++i)
                {
                    if (values[i] != other.values[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Span<ushort> CapacityValuesAsSpan()
                => new(UnsafeUtils.AddressOf(ref values[0]), FILTER_COMPONENT_MAX);
        }
    }
}
