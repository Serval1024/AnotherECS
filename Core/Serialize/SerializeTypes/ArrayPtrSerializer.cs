using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core.Collection;
using AnotherECS.Unsafe;

namespace AnotherECS.Serializer
{
    internal unsafe struct ArrayPtrSerializer
    {
        private ArrayPtrMeta _meta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref ArrayPtr data)
        {
            _meta.Pack(ref writer, ref data);
            if (data.GetPtr() != null)
            {
                writer.Write(data.GetPtr(), data.ByteLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, ref ArrayPtr data)
        {   
            var (byteLength, elementCount) = _meta.Unpack(ref reader);
            if (byteLength != uint.MaxValue)
            {
                var buffer = byteLength != 0 ? UnsafeMemory.Malloc(byteLength) : null;
                reader.Read(buffer, byteLength);

                data = new ArrayPtr(buffer, byteLength, elementCount);
                return;
            }
            data = default;
        }
    }


    internal unsafe struct ArrayPtrSerializer<T> : IElementSerializer
        where T : unmanaged
    {
        private ArrayPtrMeta _meta;
        private CompoundMeta _compound;

        public Type Type => typeof(ArrayPtr<>);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer, ref ArrayPtr<T> data)
        {
            _meta.Pack(ref writer, ref data);
            if (data.GetPtr() != null)
            {
                writer.Write(data.GetPtr(), data.ByteLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader, ref ArrayPtr<T> data)
        {
            var (byteLength, elementCount) = _meta.Unpack(ref reader);
            if (byteLength != uint.MaxValue)
            {
                var buffer = byteLength != 0 ? (T*)UnsafeMemory.Malloc(byteLength) : null;
                reader.Read(buffer, byteLength);

                data = new ArrayPtr<T>(buffer, elementCount);
                return;
            }
            data = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref ArrayPtr<T> data)
        {
            _meta.Pack(ref writer, ref data);

            if (data.GetPtr() != null)
            {
                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    for (uint i = 0; i < data.ElementCount; ++i)
                    {
                        var value = data.Get(i);
                        ((ISerialize)value).Pack(ref writer);
                    }
                }
                else
                {
                    if (writer.GetSerializer(typeof(T), out var serializer))
                    {
                        for (uint i = 0; i < data.ElementCount; ++i)
                        {
                            serializer.Pack(ref writer, data.Get(i));
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < data.ElementCount; ++i)
                        {
                            _compound.Pack(ref writer, data.Get(i));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, ref ArrayPtr<T> data)
        {
            (uint byteLength, uint elementCount) = _meta.Unpack(ref reader);

            if (byteLength != uint.MaxValue)
            {
                data.Resize(elementCount);

                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    for (uint i = 0; i < data.ElementCount; ++i)
                    {
                        var serialize = new T() as ISerialize;
                        serialize.Unpack(ref reader);
                        data.Set(i, (T)serialize);
                    }
                }
                else
                {
                    if (reader.GetSerializer(typeof(T), out var serializer))
                    {
                        for (uint i = 0; i < data.ElementCount; ++i)
                        {
                            data.Set(i, (T)serializer.Unpack(ref reader, null));
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < data.ElementCount; ++i)
                        {
                            data.Set(i, (T)_compound.Unpack(ref reader, typeof(T)));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, object value)
        {
            var concreteValue = (ArrayPtr<T>)value;
            Pack(ref writer, ref concreteValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
        {
            ArrayPtr<T> concreteValue = default;
            Unpack(ref reader, ref concreteValue);
            return concreteValue;
        }
    }

    internal unsafe struct ArrayPtrEachSerializeStaticSerializer<T>
       where T : unmanaged, ISerialize
    {
        private static readonly ArrayPtrMeta _meta;
        private static readonly CountMeta _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref ArrayPtr<T> arrayPtr)
        {
            Pack(ref writer, ref arrayPtr, arrayPtr.ElementCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref ArrayPtr<T> arrayPtr, uint count)
        {
            _meta.Pack(ref writer, ref arrayPtr);
            var ptr = arrayPtr.GetPtr();
            if (ptr != null)
            {
                _count.Pack(ref writer, count);

                for (uint i = 0; i < count; i++)
                {
                    ptr[i].Pack(ref writer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArrayPtr<T> Unpack(ref ReaderContextSerializer reader)
        {
            (uint byteLength, uint elementCount) = _meta.Unpack(ref reader);

            if (byteLength != uint.MaxValue)
            {
                var count = _count.Unpack(ref reader);

                var buffer = (T*)UnsafeMemory.Malloc(byteLength);
                T element = default;
                for (uint i = 0; i < count; i++)
                {
                    element.Unpack(ref reader);
                    buffer[i] = element;
                }
                return new ArrayPtr<T>(buffer, elementCount);
            }
            else
            {
                return default;
            }
        }
    }
}