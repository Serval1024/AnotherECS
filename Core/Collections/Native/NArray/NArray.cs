using AnotherECS.Core.Allocators;
using AnotherECS.Core.Exceptions;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AnotherECS.Core.Collection
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [System.Diagnostics.DebuggerTypeProxy(typeof(NArray<,>.NArrayDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NArray<TAllocator, T> : INArray<T>, ISerialize, IEnumerable<T>, IRepairMemoryHandle
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        private TAllocator* _allocator;
        private MemoryHandle _data;
        private uint _length;

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length * ElementSize;
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)sizeof(T);
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

        public NArray(TAllocator* allocator, ref MemoryHandle memoryHandle, uint elementCount)
        {
            _data = memoryHandle;
            _allocator = allocator;
            _length = elementCount;

            allocator->Repair(ref _data);
        }

        public NArray(TAllocator* allocator, uint elementCount)
        {
            _allocator = allocator;
            _length = elementCount;
            _data = allocator->Allocate(_length * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(TAllocator* allocator, uint elementCount)
        {
            Dispose();
            this = new(allocator, elementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Allocate(uint elementCount)
        {
            if (IsAllocatorValid())
            {
                Dispose();
                this = new(_allocator, elementCount);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsAllocatorValid()
            => _allocator != null && _allocator->IsValid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TAllocator* GetAllocator()
            => _allocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetAllocator(TAllocator* allocator)
        {
            _allocator = allocator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MemoryHandle GetMemoryHandle()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _data;
        }

        #region ReadPtr
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return (T*)_data.pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ReadPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ReadPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* ReadPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
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
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ReadRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(ReadPtr() + index);
        }
        #endregion

        #region Read
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(ReadPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(ReadPtr() + index);
        }
        #endregion

        #region GetPtr
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            Dirty();
            return ReadPtr();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return GetPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return GetPtr() + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return GetPtr() + index;
        }
        #endregion

        #region GetRef
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(GetPtr() + index);
        }
        #endregion

        #region Get
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(GetPtr() + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(GetPtr() + index);
        }
        #endregion

        #region Set
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, ref T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(GetPtr() + index) = value;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            ResizeInternal(elementCount * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(ref NArray<TAllocator, T> other)
        {
            Dispose();
            _allocator = other._allocator;
            _data = other._data;
            _length = other._length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(T element)
        {
            SetAll(0, Length, element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(uint start, uint elementCount, T element)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            var ptr = GetPtr();
            for(uint i = start, iMax = elementCount + start; i < iMax; ++i)
            {
                ptr[i] = element;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllByte(byte value)
        {
            SetAllByte(0, ByteLength, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAllByte(uint start, uint byteLength, byte value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (ByteLength < byteLength + start)
            {
                throw new OutOfMemoryException();
            }
#endif
            UnsafeMemory.MemSet(((byte*)_data.GetPtr()) + start, value, byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray<TAllocator, T> other)
        {
            CreateFrom(other, other.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray<TAllocator, T> other, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(other);
            if (count > other.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif
            var copySize = count * other.ElementSize;
            if (_data.IsValid && Length != other.Length)
            {
                _allocator->Deallocate(ref _data);
            }
            if (_data.IsValid)
            {
                Dirty();
            }
            else
            {
                _data = _allocator->Allocate(copySize);
            }
            UnsafeMemory.MemCopy(_data.GetPtr(), other._data.GetPtr(), copySize);
            _length = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in NArray<TAllocator, T> other)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            ExceptionHelper.ThrowIfBroken(other);
#endif
            Dirty();
            UnsafeMemory.MemCopy(other.ReadPtr(), ReadPtr(), Math.Min(ByteLength, other.ByteLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dirty()
        {
            _allocator->Dirty(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Clear(0, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint start, uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
            if (_length < elementCount + start)
            {
                throw new ArgumentOutOfRangeException($"{nameof(elementCount)} or {nameof(start)}");
            }
#endif
            Dirty();
            UnsafeMemory.Clear(GetPtr() + start, elementCount * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_data.IsValid)
            {
                _allocator->Deallocate(ref _data);
                _length = 0;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ToWArray()
            => ToWArray(0, _length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WArray<T> ToWArray(uint start, uint count)
        {
#if !ANOTHERECS_RELEASE
            if (start + count > _length)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(start)}' or '{nameof(count)}' out of range.");
            }
#endif
            return new(ReadPtr() + start, count);
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
            ExceptionHelper.ThrowIfBroken(this);
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

            var data = ReadPtr();
            var iMax = Math.Min(array.Length - startIndex, count);
            for (int i = 0; i < iMax; ++i)
            {
                array[i + startIndex] = data[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer)
        {
            default(NArraySerializer<TAllocator, T>)
                .Pack(ref writer, ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader)
        {
            default(NArraySerializer<TAllocator, T>)
                .Unpack(ref reader, ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer)
        {
            default(NArraySerializer<TAllocator, T>)
                .PackBlittable(ref writer, ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            default(NArraySerializer<TAllocator, T>)
                .UnpackBlittable(ref reader, ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
          => new(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterCheckChanges()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);    
#endif
            _allocator->EnterCheckChanges(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExitCheckChanges()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            return _allocator->ExitCheckChanges(ref _data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeInternal(uint byteLength)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfBroken(this);
#endif
            if (byteLength != ByteLength)
            {
                if (_data.IsValid)
                {
                    if (!_allocator->TryResize(ref _data, byteLength))
                    {
                        var newData = _allocator->Allocate(byteLength);
                        UnsafeMemory.MemCopy(newData.GetPtr(), _data.GetPtr(), Math.Min(byteLength, ByteLength));
                        _allocator->Deallocate(ref _data);
                        _data = newData;
                    }
                }
                else
                {
                    _data = _allocator->Allocate(byteLength);
                }
                _length = byteLength / (uint)sizeof(T);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IRepairMemoryHandle.RepairMemoryHandle(ref RepairMemoryContext repairMemoryContext)
        {
            if (IsValid)
            {
                repairMemoryContext.Repair(_allocator->GetId(), ref _data);
                RepairMemoryHandleElement(ref repairMemoryContext);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RepairMemoryHandleElement(ref RepairMemoryContext repairMemoryContext)
        {
            if (typeof(T) is IRepairMemoryHandle)
            {
                for (uint i = 0; i < Length; ++i)
                {
                    var rmh = (IRepairMemoryHandle)ReadRef(i);
                    rmh.RepairMemoryHandle(ref repairMemoryContext);
                    ReadRef(i) = (T)rmh;
                }
            }
        }


        public struct Enumerator : IEnumerator<T>
        {
            private readonly NArray<TAllocator, T> _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NArray<TAllocator, T> data)
            {
                _data = data;
                _length = _data.Length;
                _current = uint.MaxValue;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.Read(_current);
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
            private NArray<TAllocator, T> array;
            public NArrayDebugView(NArray<TAllocator, T> array)
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
