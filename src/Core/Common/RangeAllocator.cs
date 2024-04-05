using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using AnotherECS.Serializer;
using System;
using System.Runtime.CompilerServices;

namespace AnotherECS.Core
{
    public unsafe struct RangeAllocator<TAllocator> : IRepairMemoryHandle, ISerialize, IDisposable
        where TAllocator : unmanaged, IAllocator
    {
        private uint _current;
        private NList<TAllocator, Range> _recycleRanges;
        private NDictionary<TAllocator, uint, uint, U4U4HashProvider> _sizes;


        public RangeAllocator(TAllocator* allocator, uint capacity)
        {
            _current = 0;
            _recycleRanges = new NList<TAllocator, Range>(allocator, capacity);
            _sizes = new NDictionary<TAllocator, uint, uint, U4U4HashProvider>(allocator, capacity);
        }

        public uint Allocate(uint size)
        {
            if (size == 0)
            {
                return 0;
            }

            if (_recycleRanges.Count == 0)
            {
                var result = _current;
                _current += size;
                _sizes.Add(result, size);
                return result;
            }
            else
            {
                var index = BinarySearchEnough(_recycleRanges, size);
                if (index == -1)
                {
                    var result = _current;
                    _current += size;
                    _sizes.Add(result, size);
                    return result;
                }
                else
                {
                    var location = _recycleRanges.Get(index);
                    _recycleRanges.RemoveAt((uint)index);

                    if (location.size > size)
                    {
                        var tailIndex = location.index + size;
                        var railSize = location.size - size;
                        _recycleRanges.AddSort(new Range() { index = tailIndex, size = railSize });
                        _sizes.Add(tailIndex, railSize);

                        location.size = size;
                    }

                    _sizes.Add(location.index, location.size);

                    return location.index;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deallocate(uint index)
        {
            _recycleRanges.AddSort(new Range() { index = index, size = _sizes[index] });
            _sizes.Remove(index);
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_current);
            _recycleRanges.PackBlittable(ref writer);
            _sizes.PackBlittable(ref writer);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            _current = reader.ReadUInt32();
            _recycleRanges.UnpackBlittable(ref reader);
            _sizes.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            RepairMemoryCaller.Repair(ref _recycleRanges, ref repairMemoryContext);
            RepairMemoryCaller.Repair(ref _sizes, ref repairMemoryContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BinarySearchEnough(NList<TAllocator, Range> values, uint size)
        {
            if (values.Count <= 4)
            {
                for (int i = 0; i < values.Count; ++i)
                {
                    if (values.ReadRef(i).size >= size)
                    {
                        return i;
                    }
                }
                return -1;
            }
            else
            {
                int lo = 0;
                int hi = lo + (int)values.Count - 1;

                while (lo <= hi)
                {
                    int i = GetMedian(lo, hi);

                    uint c = values.ReadRef(i).size;
                    if (c < size)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        return i;
                    }
                }
                return ~lo;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static int GetMedian(int low, int hi)
                    => low + ((hi - low) >> 1);
            }
        }

        public void Dispose()
        {
            _recycleRanges.Dispose();
            _sizes.Dispose();
        }

        private struct Range : IComparable<Range>
        {
            public uint index;
            public uint size;

            public int CompareTo(Range other)
                => (int)size - (int)other.size;
        }
    }
}