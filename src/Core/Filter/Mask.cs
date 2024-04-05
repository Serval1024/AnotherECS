using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    internal unsafe struct Mask : IHashProvider<Mask, uint>, IEquatable<Mask>
    {
        public Items includes;
        public Items excludes;

        private uint _hash;

        public bool IsValid
            => includes.count != 0;

        public uint Hash
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
        public void AddInclude(uint item)
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
        public void AddExclude(uint item)
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
        private uint GetHash()
            => (includes.GetHash() ^ excludes.GetHash()) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint IHashProvider<Mask, uint>.GetHash(ref Mask key)
            => key.Hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Mask other)
            => includes.Equals(other.includes) && excludes.Equals(other.excludes);

        public bool Contains(uint item)
            => includes.Contains(item) || excludes.Contains(item);


        public unsafe struct Items: IEquatable<Items>
        {
            public const int FILTER_COMPONENT_MAX = 8;

            public byte count;
            public fixed uint values[FILTER_COMPONENT_MAX];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(uint item)
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
            public bool Contains(uint item)
                => BinarySearch(item);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool BinarySearch(uint item)
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
                    int lo = 0;
                    int hi = lo + count - 1;

                    while (lo <= hi)
                    {
                        int i = GetMedian(lo, hi);

                        uint c = values[i];
                        if (c == item)
                        {
                            return true;
                        }
                        else if (c < item)
                        {
                            lo = i + 1;
                        }
                        else
                        {
                            hi = i - 1;
                        }
                    }
                    return false;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static int GetMedian(int low, int hi)
                => low + ((hi - low) >> 1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<uint> ValuesAsSpan()
                => new(UnsafeUtils.AddressOf(ref values[0]), count);

            public uint GetHash()
            {
                uint hash = count;
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
            private Span<uint> CapacityValuesAsSpan()
                => new(UnsafeUtils.AddressOf(ref values[0]), FILTER_COMPONENT_MAX);
        }
    }
}
