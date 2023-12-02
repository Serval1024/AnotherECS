using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Collection
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [System.Diagnostics.DebuggerTypeProxy(typeof(NArray.NArrayDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NArray : INArray, IDisposable, IEnumerable<byte>, ISerialize
    {
        private void* _data;
        private uint _byteLength;
        private uint _length;

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _byteLength;
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _byteLength / _length;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data != null;
        }

        public NArray(void* data, uint byteLength, uint elementCount)
        {
            _data = data;
            _byteLength = byteLength;
            _length = elementCount;
        }

        public NArray(uint byteLength, uint elementCount)
        {
            _data = UnsafeMemory.Allocate(byteLength);
            _byteLength = byteLength;
            _length = elementCount;
        }

        public void Allocate(uint byteLength, uint elementCount)
            => this = Create(byteLength, elementCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NArray CreateWrapper<T>(ref NArray<T> other)
            where T : unmanaged
        {
            NArray wrapper;
            wrapper._data = other.GetPtr();
            wrapper._byteLength = other.ByteLength;
            wrapper._length = other.Length;
            return wrapper;
        }

        public static NArray Create(uint byteLength, uint elementCount)
            => new(byteLength, elementCount);

        public static NArray Create<T>(uint elementCount)
            where T : unmanaged
        {
            NArray arrayPtr;
            arrayPtr._data = UnsafeMemory.Allocate<T>(elementCount);
            arrayPtr._byteLength = elementCount * (uint)sizeof(T);
            arrayPtr._length = elementCount;
            return arrayPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>()
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return (T*)_data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>(ulong index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ((T*)_data) + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef<T>(ulong index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ref *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(ulong index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(ulong index, T value)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            *(((T*)_data) + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>(uint index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ((T*)_data) + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef<T>(uint index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ref *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(uint index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, T value)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            *(((T*)_data) + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>(int index)
           where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ((T*)_data) + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef<T>(int index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return ref *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(int index)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            return *(((T*)_data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(int index, T value)
            where T : unmanaged
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index, (uint)sizeof(T));
#endif
            *(((T*)_data) + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint byteLength)
        {
            Resize(byteLength, sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize<T>(uint elementCount)
            where T : unmanaged
        {
            Resize(elementCount, (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint elementCount, uint elementSize)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            var byteLength = elementCount * elementSize;
            if (byteLength != ByteLength)
            {
                var ptr = UnsafeMemory.Allocate(byteLength);
                if (_data != null)
                {
                    UnsafeMemory.MemCopy(ptr, _data, Math.Min(byteLength, _byteLength));
                    UnsafeMemory.Deallocate(ref _data);
                }
                _data = ptr;
                _byteLength = byteLength;
                _length = elementCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray other)
        {
            CreateFrom(other, other._length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray other, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(other);
            if (count > other._length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif
            var copySize = count * other.ElementSize;
            if (_data != null && copySize != _byteLength)
            {
                UnsafeMemory.Deallocate(ref _data);
            }
            if (_data == null)
            {
                _data = UnsafeMemory.Allocate(copySize);
            }
            UnsafeMemory.MemCopy(_data, other._data, copySize);
            _byteLength = copySize;
            _length = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in NArray other)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
            ExceptionHelper.ThrowIfNArrayBroken(other);
#endif
            UnsafeMemory.MemCopy(_data, other._data, Math.Min(_byteLength, other._byteLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in NArray other, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
            ExceptionHelper.ThrowIfNArrayBroken(other);
            if (count > _length || count > other._length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (_length != other._length)
            {
                throw new ArgumentException("It is not safe to copy to storage with a different data type.");
            }
#endif
            UnsafeMemory.MemCopy(_data, other._data, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            var segment = _byteLength / this._length;
            UnsafeMemory.Clear(_data, elementCount * segment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            UnsafeMemory.Clear(_data, _byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UnsafeMemory.Deallocate(ref _data);
            _byteLength = 0;
        }

        public byte[] ToArray()
        {
            var result = new byte[_length];
            for (uint i = 0; i < _length; ++i)
            {
                result[i] = Get<byte>(i);
            }
            return result;
        }

        public T[] ToArray<T>()
            where T : unmanaged
        {
            var result = new T[_length];
            for (uint i = 0; i < _length; ++i)
            {
                result[i] = Get<T>(i);
            }
            return result;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            NArraySerializer serializer;
            serializer.Pack(ref writer, ref this);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            NArraySerializer serializer;
            serializer.Unpack(ref reader, ref this);
        }

        public IEnumerator<byte> GetEnumerator()
            => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public struct Enumerator : IEnumerator<byte>
        {
            private readonly NArray _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NArray data)
            {
                _data = data;
                _length = _data.ByteLength;
                _current = uint.MaxValue;
            }

            public byte Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data.GetPtr<byte>()[_current];
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
            private NArray array;
            public NArrayDebugView(NArray array)
            {
                this.array = array;
            }

            public uint Length
                => array.Length;

            public byte[] Data
                => array.ToArray();
        }
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Option.ArrayBoundsChecks, false)]
#endif
    [System.Diagnostics.DebuggerTypeProxy(typeof(NArray<>.NArrayDebugView))]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NArray<T> : INArray, IDisposable, ISerialize, IEnumerable<T>
        where T : unmanaged
    {
        private T* _data;
        private uint _byteLength;
        private uint _length;

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _byteLength;
        }

        public uint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _byteLength / _length;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data != null;
        }

        public NArray(T* data, uint elementCount)
        {
            this._data = data;
            this._byteLength = elementCount * (uint)sizeof(T);
            this._length = elementCount;
        }

        public NArray(uint elementCount)
        {
            _data = UnsafeMemory.Allocate<T>(elementCount);
            _byteLength = elementCount * (uint)sizeof(T);
            this._length = elementCount;
        }

        public void Allocate(uint elementCount)
            => this = Create(elementCount);

        public static NArray<T> Create(uint elementCount)
            => new(elementCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NArray<T> CreateWrapper(ref NArray other)
        {
            NArray<T> wrapper;
            wrapper._data = other.GetPtr<T>();
            wrapper._byteLength = other.ByteLength;
            wrapper._length = other.Length;
            return wrapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            return _data;
        }

        void* INArray.GetPtr()
            => GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return _data + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(ulong index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(ulong index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(_data + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return _data + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(_data + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return _data + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return ref *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            return *(_data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, T value)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this, index);
#endif
            *(_data + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
#endif
            ResizeInternal(elementCount * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(ref NArray<T> other)
        {
            Dispose();
            _data = other._data;
            _byteLength = other._byteLength;
            _length = other._length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray<T> other)
        {
            CreateFrom(other, other.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in NArray<T> other, uint count)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(other);
            if (count > other.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif
            var copySize = count * other.ElementSize;
            UnsafeMemory.Deallocate(ref _data);
            _data = (T*)UnsafeMemory.Allocate(copySize);

            UnsafeMemory.MemCopy(_data, other._data, copySize);
            _byteLength = copySize;
            _length = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in NArray<T> other)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
            ExceptionHelper.ThrowIfNArrayBroken(other);
#endif
            UnsafeMemory.MemCopy(other._data, _data, Math.Min(_byteLength, other._byteLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Clear(Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint elementCount)
        {
#if !ANOTHERECS_RELEASE
            ExceptionHelper.ThrowIfNArrayBroken(this);
            if (_length > elementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            }
#endif
            DisposeElement();
            UnsafeMemory.Clear(_data, _byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            DisposeElement();
            UnsafeMemory.Deallocate(ref _data);
            _byteLength = 0;
        }

        public NArray<T> ToNArray()
        {
            var result = new NArray<T>(Length);
            for (int i = 0; i < result.Length; ++i)
            {
                result.GetRef(i) = GetRef(i);
            }

            return result;
        }

        public T[] ToArray()
        {
            var result = new T[_length];
            for (uint i = 0; i < _length; ++i)
            {
                result[i] = Get(i);
            }
            return result;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            NArraySerializer<T> serializer;
            serializer.Pack(ref writer, ref this);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            NArraySerializer<T> serializer;
            serializer.Unpack(ref reader, ref this);
        }

        public void PackBlittable(ref WriterContextSerializer writer)
        {
            NArraySerializer<T> serializer;
            serializer.PackBlittable(ref writer, ref this);
        }

        public void UnpackBlittable(ref ReaderContextSerializer reader)
        {
            NArraySerializer<T> serializer;
            serializer.UnpackBlittable(ref reader, ref this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<T> GetEnumerator()
          => new Enumerator(ref this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeInternal(uint byteLength)
        {
            if (byteLength != ByteLength)
            {
                var ptr = UnsafeMemory.Allocate(byteLength);
                UnsafeMemory.MemCopy(ptr, _data, Math.Min(byteLength, _byteLength));
                UnsafeMemory.Deallocate(ref _data);
                _data = (T*)ptr;
                _byteLength = byteLength;
                _length = byteLength / (uint)sizeof(T);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisposeElement()
        {
            if (typeof(T) is IDisposable)
            {
                for (int i = 0; i < Length; ++i)
                {
                    ((IDisposable)_data[i]).Dispose();
                }
            }
        }


        public struct Enumerator : IEnumerator<T>
        {
            private readonly NArray<T> _data;
            private uint _current;
            private readonly uint _length;
            public Enumerator(ref NArray<T> data)
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
            private NArray<T> array;
            public NArrayDebugView(NArray<T> array)
            {
                this.array = array;
            }

            public uint Length
                => array.Length;

            public T[] Data
                => array.ToArray();
        }
    }

    public unsafe interface INArray : IDisposable
    {
        public uint ByteLength { get; }
        public uint Length { get; }
        public uint ElementSize { get; }
        public bool IsValide { get; }
        public void* GetPtr();
    }
}
