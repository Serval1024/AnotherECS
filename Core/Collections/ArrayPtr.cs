using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AnotherECS.Serializer;
using AnotherECS.Unsafe;

namespace AnotherECS.Core.Collection
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ArrayPtr : IArrayPtr, IDisposable, ISerialize
    {
        private void* data;
        private uint byteLength;
        private uint elementCount;

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => byteLength;
        }

        public uint ElementCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => elementCount;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => byteLength / elementCount;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data != null;
        }

        public ArrayPtr(void* data, uint byteLength, uint elementCount)
        {
            this.data = data;
            this.byteLength = byteLength;
            this.elementCount = elementCount;
        }

        public ArrayPtr(uint byteLength, uint elementCount)
        {
            data = UnsafeMemory.Allocate(byteLength);
            this.byteLength = byteLength;
            this.elementCount = elementCount;
        }

        public void Allocate(uint byteLength, uint elementCount)
            => this = Create(byteLength, elementCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr CreateWrapper<T>(ref ArrayPtr<T> other)
            where T : unmanaged
        {
            ArrayPtr wrapper;
            wrapper.data = other.GetPtr();
            wrapper.byteLength = other.ByteLength;
            wrapper.elementCount = other.ElementCount;
            return wrapper;
        }

        public static ArrayPtr Create(uint byteLength, uint elementCount)
            => new(byteLength, elementCount);

        public static ArrayPtr Create<T>(uint elementCount)
            where T : unmanaged
        {
            ArrayPtr arrayPtr;
            arrayPtr.data = UnsafeMemory.Allocate<T>(elementCount);
            arrayPtr.byteLength = elementCount * (uint)sizeof(T);
            arrayPtr.elementCount = elementCount;
            return arrayPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* GetPtr()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>()
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            return (T*)data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>(uint index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return ((T*)data) + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef<T>(uint index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return ref *(((T*)data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(uint index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return *(((T*)data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            *(((T*)data) + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr<T>(int index)
           where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return ((T*)data) + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef<T>(int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return ref *(((T*)data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(int index)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            return *(((T*)data) + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(int index, T value)
            where T : unmanaged
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index, (uint)sizeof(T));
#endif
            *(((T*)data) + index) = value;
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
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            var byteLength = elementCount * elementSize;
            if (byteLength != ByteLength)
            {
                var ptr = UnsafeMemory.Allocate(byteLength);
                if (data != null)
                {
                    UnsafeMemory.MemCopy(ptr, data, Math.Min(byteLength, this.byteLength));
                    UnsafeMemory.Deallocate(ref data);
                }
                data = ptr;
                this.byteLength = byteLength;
                this.elementCount = elementCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in ArrayPtr other)
        {
            CreateFrom(other, other.elementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in ArrayPtr other, uint count)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(other);
            if (count > other.elementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif
            var copySize = count * other.ElementSize;
            if (data != null && copySize != byteLength)
            {
                UnsafeMemory.Deallocate(ref data);
            }
            if (data == null)
            {
                data = UnsafeMemory.Allocate(copySize);
            }
            UnsafeMemory.MemCopy(data, other.data, copySize);
            byteLength = copySize;
            elementCount = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in ArrayPtr other)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
            ExceptionHelper.ThrowIfArrayPtrBroken(other);
#endif
            UnsafeMemory.MemCopy(data, other.data, Math.Min(byteLength, other.byteLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in ArrayPtr other, uint count)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
            ExceptionHelper.ThrowIfArrayPtrBroken(other);
            if (count > elementCount || count > other.elementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (elementCount != other.elementCount)
            {
                throw new ArgumentException("It is not safe to copy to storage with a different data type.");
            }
#endif
            UnsafeMemory.MemCopy(data, other.data, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint elementCount)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            var segment = byteLength / this.elementCount;
            UnsafeMemory.Clear(data, elementCount * segment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            UnsafeMemory.Clear(data, byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UnsafeMemory.Deallocate(ref data);
            byteLength = 0;
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            ArrayPtrSerializer serializer;
            serializer.Pack(ref writer, ref this);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            ArrayPtrSerializer serializer;
            serializer.Unpack(ref reader, ref this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ArrayPtr<T> : IArrayPtr, IDisposable, ISerialize
        where T : unmanaged
    {
        private T* data;
        private uint byteLength;
        private uint elementCount;

        public uint ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => byteLength;
        }

        public uint ElementCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => elementCount;
        }

        public uint ElementSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => byteLength / elementCount;
        }

        public bool IsValide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data != null;
        }

        public ArrayPtr(T* data, uint elementCount)
        {
            this.data = data;
            this.byteLength = elementCount * (uint)sizeof(T);
            this.elementCount = elementCount;
        }

        public ArrayPtr(uint elementCount)
        {
            data = UnsafeMemory.Allocate<T>(elementCount);
            byteLength = elementCount * (uint)sizeof(T);
            this.elementCount = elementCount;
        }

        public void Allocate(uint elementCount)
            => this = Create(elementCount);

        public static ArrayPtr<T> Create(uint elementCount)
            => new(elementCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr<T> CreateWrapper(ref ArrayPtr other)
        {
            ArrayPtr<T> wrapper;
            wrapper.data = other.GetPtr<T>();
            wrapper.byteLength = other.ByteLength;
            wrapper.elementCount = other.ElementCount;
            return wrapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            return data;
        }

        void* IArrayPtr.GetPtr()
            => GetPtr();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetPtr(uint index)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index);
#endif
            return data + index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRef(uint index)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index);
#endif
            return ref *(data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(uint index)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index);
#endif
            return *(data + index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, T value)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this, index);
#endif
            *(data + index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(uint elementCount)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            ResizeInternal(elementCount * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in ArrayPtr<T> other)
        {
            CreateFrom(other, other.ElementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CreateFrom(in ArrayPtr<T> other, uint count)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(other);
            if (count > other.ElementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
#endif
            var copySize = count * other.ElementSize;
            UnsafeMemory.Deallocate(ref data);
            data = (T*)UnsafeMemory.Allocate(copySize);

            UnsafeMemory.MemCopy(data, other.data, copySize);
            byteLength = copySize;
            elementCount = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(in ArrayPtr<T> other)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
            ExceptionHelper.ThrowIfArrayPtrBroken(other);
#endif
            UnsafeMemory.MemCopy(other.data, data, Math.Min(byteLength, other.byteLength));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(uint elementCount)
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            UnsafeMemory.Clear(data, elementCount * (uint)sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if ANOTHERECS_DEBUG
            ExceptionHelper.ThrowIfArrayPtrBroken(this);
#endif
            UnsafeMemory.Clear(data, byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UnsafeMemory.Deallocate(ref data);
            byteLength = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeInternal(uint byteLength)
        {
            if (byteLength != ByteLength)
            {
                var ptr = UnsafeMemory.Allocate(byteLength);
                UnsafeMemory.MemCopy(ptr, data, Math.Min(byteLength, this.byteLength));
                UnsafeMemory.Deallocate(ref data);
                data = (T*)ptr;
                this.byteLength = byteLength;
                elementCount = byteLength / (uint)sizeof(T);
            }
        }

        public void Pack(ref WriterContextSerializer writer)
        {
            ArrayPtrSerializer<T> serializer;
            serializer.Pack(ref writer, ref this);
        }

        public void Unpack(ref ReaderContextSerializer reader)
        {
            ArrayPtrSerializer<T> serializer;
            serializer.Unpack(ref reader, ref this);
        }
    }

    public unsafe interface IArrayPtr : IDisposable
    {
        public uint ByteLength { get; }
        public uint ElementCount { get; }
        public uint ElementSize { get; }
        public bool IsValide { get; }
        public void* GetPtr();
    }
}
