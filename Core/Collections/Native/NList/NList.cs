using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    public unsafe struct NList<TAllocator, T> : INArray<T>, ISerialize, IEnumerable<T>, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        internal NArray<TAllocator, T> _data;
        private uint _count;

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public uint Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length;
        }

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.ByteLength;
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.Length;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.ElementSize;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsValid;
        }

        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data.IsDirty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NList(NArray<TAllocator, T> array)
        {
            _data = array.ToNArray();
            _count = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NList(TAllocator* allocator, uint capacity)
        {
            _data = new NArray<TAllocator, T>(allocator, capacity);
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NList<TAllocator, T> CreateWrapper(NArray<TAllocator, T> other)
        {
            NList<TAllocator, T> wrapper = default;
            wrapper._data = other;
            wrapper._count = other.Length;
            return wrapper;
        }

        public void ExtendToCapacity()
        {
            _count = Capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (Count == _data.Length)
            {
                _data.Resize(Count << 1);
            }
            _data.GetRef(_count++) = value;
        }

        #region ReadPtr
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _data.ReadPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ReadPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ReadPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ReadPtr() + index;
        }
        #endregion

        #region ReadRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef()
            => ref *ReadPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(ReadPtr() + index);
        }
        #endregion

        #region Read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(ReadPtr() + index);
        }
        #endregion

        #region GetPtr
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _data.GetPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return GetPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return GetPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return GetPtr() + index;
        }
        #endregion

        #region GetRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return ref *(GetPtr() + index);
        }
        #endregion

        #region Get
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            return *(GetPtr() + index);
        }
        #endregion

        #region Set
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, Count);
#endif
            *(GetPtr() + index) = value;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            if (Count != 0)
            {
                --_count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(uint index)
        {
            RemoveAtInternal(index, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveAtInternal(uint index, uint capacity)
        {
            if (index == capacity - 1)
            {
                RemoveLast();
            }
            else
            {
                for (uint i = index; i < capacity - 1; ++i)
                {
                    _data.GetRef(i) = _data.GetRef(i + 1);
                }
                --_count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _data.Clear();
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            writer.Write(_count);
            _data.Pack(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _count = reader.ReadUInt32();
            _data.Unpack(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer)
        {
            writer.Write(_count);
            _data.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            _count = reader.ReadUInt32();
            _data.UnpackBlittable(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
         => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public NArray<TAllocator, T> ToNArray()            
        {
            var result = new NArray<TAllocator, T>(_data.GetAllocator(), Count);
            for (int i = 0; i < result.Length; ++i)
            {
                result.GetRef(i) = _data.GetRef(i);
            }

            return result;
        }

        public void Resize(uint elementCount)
        {
            _data.Resize(elementCount);
            if (elementCount < Count)
            {
                _count = elementCount;
            }
        }

        public void Dirty()
        {
            _data.Dirty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly NList<TAllocator, T> _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NList<TAllocator, T> data)
            {
                _data = data;
                _length = _data.Count;
                _current = uint.MaxValue;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.Get(_current);
            }

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
                => unchecked(++_current < _length);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
    }
}
