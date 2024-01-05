using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [System.Diagnostics.DebuggerTypeProxy(typeof(NContainerArray<,>.NArrayDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NContainerArray<TAllocator, T> : INArray<T>, IDisposable, ISerialize, IEnumerable<T>, IRebindMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        private TAllocator* _allocator;
        private NArray<TAllocator, MemoryHandle> _data;

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

        public NContainerArray(TAllocator* allocator, uint elementCount)
        {
            _allocator = allocator;
            _data = new NArray<TAllocator, MemoryHandle>(allocator, elementCount);
            for (uint i = 0; i < _data.Length; ++i)
            {
                _data.ReadRef(i) = allocator->Allocate((uint)sizeof(T));
            }
        }

        public NContainerArray(TAllocator* allocator, ref MemoryHandle memoryHandle, uint elementCount)
        {
            _allocator = allocator;
            _data = new NArray<TAllocator, MemoryHandle>(allocator, ref memoryHandle, elementCount);
        }

        public void Allocate(TAllocator* allocator, uint elementCount)
        {
            this = new(allocator, elementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TAllocator* GetAllocator()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _allocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return (T*)_data.ReadPtr(index)->pointer;
        }

        #region ReadPtr
        T* INArray<T>.ReadPtr()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryHandle GetMemoryHandle(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _data.GetRef(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirtyHandler<TAllocator> GetDirtyHandler(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return new(_allocator, GetMemoryHandle(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return (T*)_data.ReadPtr(index)->pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return (T*)_data.ReadPtr(index)->pointer;
        }
        #endregion

        #region ReadRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return ref *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return ref *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return ref *ReadPtr(index);
        }
        #endregion

        #region Read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return *ReadPtr(index);
        }
        #endregion

        #region GetPtr
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T* INArray<T>.GetPtr()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ReadPtr(index);
        }
        #endregion

        #region GetRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ref *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ref *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return ref *ReadPtr(index);
        }
        #endregion

        #region Get
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return *ReadPtr(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            Dirty(index);
            return *ReadPtr(index);
        }
        #endregion

        #region Set
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            GetRef(index) = value;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            var lastLength = _data.Length;

            for (uint i = _data.Length - 1; i >= lastLength; --i)
            {
                _allocator->Deallocate(ref _data.GetRef(i));
            }

            _data.Resize(elementCount);

            for (uint i = lastLength; i < _data.Length; ++i)
            {
                _data.ReadRef(i) = _allocator->Allocate((uint)sizeof(T));
            }
        }

        #region Dirty
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DirtyElements()
        {
            DirtyElements(0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DirtyElements(uint start, uint elementCount)
        {
            for (uint i = start; i < elementCount; ++i)
            {
                Dirty(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty()
        {
            _data.Dirty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(uint index)
        {
            _allocator->Dirty(ref _data.GetRef(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(int index)
        {
            _allocator->Dirty(ref _data.ReadRef(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty(ulong index)
        {
            _allocator->Dirty(ref _data.ReadRef(index));
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Clear(0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint start, uint elementCount)
        {
            DirtyElements(start, elementCount);

            for (uint i = start; i < elementCount; ++i)
            {
                _allocator->Reuse(ref _data.GetRef(i), ElementSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_data.IsValid)
            {
                for (uint i = 0; i < _data.Length; ++i)
                {
                    _allocator->Deallocate(ref _data.ReadRef(i));
                }
                _data.Dispose();
            }
        }

        public NArray<TAllocator, T> ToNArray()
        {
            var result = new NArray<TAllocator, T>(_allocator, Length);
            for (uint i = 0; i < result.Length; ++i)
            {
                result.ReadRef(i) = ReadRef(i);
            }

            return result;
        }

        public T[] ToArray()
        {
            var result = new T[Length];
            CopyTo(result, 0, Length);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CopyTo(T[] array, uint startIndex, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
            if (count > Length)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(count)}':{count} must be less or equal than '{nameof(Length)}': {Length}");
            }
#endif
            if (array == null)
            {
                throw new NullReferenceException(nameof(array));
            }
            if (count == 0)
            {
                throw new IndexOutOfRangeException(nameof(count));
            }
            if (array.Length < count + startIndex)
            {
                throw new ArgumentException($"There is not enough space in {nameof(array)} to copy.");
            }

            var iMax = Math.Min(array.Length - startIndex, count);
            for (int i = 0; i < iMax; ++i)
            {
                array[i + startIndex] = GetRef(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            _data.PackBlittable(ref writer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            _data.UnpackBlittable(ref reader);
            _allocator = _data.GetAllocator();

            for(uint i = 0; i < _data.Length; ++i)
            {
                _allocator->Repair(ref _data.GetRef(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
          => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges()
        {
            var memoryHandle = _data.GetMemoryHandle();
            _allocator->EnterCheckChanges(ref memoryHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges()
        {
            var memoryHandle = _data.GetMemoryHandle();
            return _allocator->ExitCheckChanges(ref memoryHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRebindMemoryHandle.RebindMemoryHandle(ref MemoryRebinderContext rebinder)
        {
            MemoryRebinderCaller.Rebind(ref _data, ref rebinder);
            RebindMemoryHandleElement(ref rebinder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RebindMemoryHandleElement(ref MemoryRebinderContext rebinder)
        {
            if (typeof(T) is IRebindMemoryHandle)
            {
                for (uint i = 0; i < _data.Length; ++i)
                {
                    var rmh = (IRebindMemoryHandle)ReadRef(i);
                    rmh.RebindMemoryHandle(ref rebinder);
                    ReadRef(i) = (T)rmh;
                }
            }
        }


        public struct Enumerator : IEnumerator<T>
        {
            private readonly NContainerArray<TAllocator, T> _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NContainerArray<TAllocator, T> data)
            {
                _data = data;
                _length = _data.Length;
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


        private class NArrayDebugView
        {
            private NContainerArray<TAllocator, T> array;
            public NArrayDebugView(NContainerArray<TAllocator, T> array)
            {
                this.array = array;
            }

            public uint Length
                => array.Length;

            public T[] Data
                => array.ToArray();
        }
    }
}