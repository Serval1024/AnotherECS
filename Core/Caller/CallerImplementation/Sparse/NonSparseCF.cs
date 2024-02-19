using AnotherECS.Core.Allocators;
using AnotherECS.Core.Collection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EntityId = System.UInt32;

namespace AnotherECS.Core.Caller
{
    internal unsafe struct NonSparseCF<TAllocator, TDense> :
        ILayoutAllocator<TAllocator, bool, TDense, uint>,
        ISparseResize<TAllocator, bool, TDense, uint>,
        IDenseResize<TAllocator, bool, TDense, uint>,
        ISparseProvider<TAllocator, bool, TDense, uint>,
        IDataIterable<TAllocator, bool, TDense, uint>,
        IBoolConst,
        ISingleDenseFlag,
        IDisposable

        where TAllocator : unmanaged, IAllocator
        where TDense : unmanaged
    {
        private uint _itemId;
        private Dependencies* _dependencies;

        public bool IsSingleDense { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool IsUseSparse { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }
        public bool Is { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => false; }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Config<TMemoryAllocatorProvider>(Dependencies* dependencies, State state, uint callerId)
            where TMemoryAllocatorProvider : IAllocatorProvider<TAllocator, TAllocator>
        { 
            _dependencies = dependencies;
            _itemId = callerId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSparseResize<TSparseBoolConst>()
           where TSparseBoolConst : struct, IBoolConst
           => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LayoutAllocate(ref ULayout<TAllocator, bool, TDense, uint> layout, TAllocator* allocator, ref Dependencies dependencies) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SparseResize<TSparseBoolConst>(ref ULayout<TAllocator, bool, TDense, uint> layout, uint capacity)
            where TSparseBoolConst : struct, IBoolConst { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DenseResize(ref ULayout<TAllocator, bool, TDense, uint> layout, uint capacity) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConvertToDenseIndex(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id)
            => id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsHas(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id)
            => _dependencies->archetype.IsHasItem(_dependencies->entities.ReadArchetypeId(id), _itemId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach<TIterator>(ref ULayout<TAllocator, bool, TDense, uint> layout, ref TIterator iterator, uint startIndex, uint count)
            where TIterator : struct, IDataIterator<TDense>
        {
            if (count != 0)
            {
                var dense = layout.dense;

                dense.Dirty();
                for (uint i = startIndex, iMax = startIndex + count; i < iMax; ++i)
                {
                    iterator.Each(i, ref dense.ReadRef(i));
                    if (--count == 0)
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TDense> GetEnumerable(ULayout<TAllocator, bool, TDense, uint> layout, uint startIndex, uint count)
        {
            if (count != 0)
            {
                var dense = layout.dense;

                dense.Dirty();
                for (uint i = startIndex, iMax = startIndex + count; i < iMax; ++i)
                {
                    yield return dense.ReadRef(i);
                    if (--count == 0)
                    {
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool ReadSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, EntityId id)
            => throw new NotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, EntityId id)
            => throw new NotSupportedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ReadSparse<T>(ref ULayout<TAllocator, bool, TDense, uint> layout, ref Dependencies dependencies)
            where T : unmanaged
            => throw new NotSupportedException();

        public void SetSparse(ref ULayout<TAllocator, bool, TDense, uint> layout, uint id, uint denseIndex)
            => throw new NotImplementedException();

        public void Dispose() { }
    }
}
