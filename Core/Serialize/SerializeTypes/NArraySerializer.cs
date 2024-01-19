using System;
using System.Runtime.CompilerServices;
using AnotherECS.Core;
using AnotherECS.Core.Collection;

namespace AnotherECS.Serializer
{
    internal unsafe struct NArraySerializer<TAllocator, T>
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged
    {
        private NArrayMeta _meta;
        private CompoundMeta _compound;

        public Type Type => typeof(NArray<,>);

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PackBlittable(ref WriterContextSerializer writer, ref NArray<TAllocator, T> data)
        {
            _meta.Pack(ref writer, ref data);
            if (data.IsValid)
            {
                writer.Write(data.GetAllocator()->GetId());
                data.GetMemoryHandle().Pack(ref writer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpackBlittable(ref ReaderContextSerializer reader, ref NArray<TAllocator, T> data)
        {
            var elementCount = _meta.Unpack(ref reader);
            if (elementCount != uint.MaxValue)
            {
                uint allocatorId = reader.ReadUInt32();
                MemoryHandle memoryHandle = default;
                memoryHandle.Unpack(ref reader);

                data = new NArray<TAllocator, T>(reader.GetDependency<WPtr<TAllocator>>(allocatorId).Value, ref memoryHandle, elementCount);
                return;
            }
            data = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, NArray<TAllocator, T> data)
        {
            Pack(ref writer, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref NArray<TAllocator, T> data)
        {
            _meta.Pack(ref writer, ref data);

            if (data.IsValid)
            {
                writer.Write(data.GetAllocator()->GetId());
                data.GetMemoryHandle().Pack(ref writer);
                
                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    for (uint i = 0; i < data.Length; ++i)
                    {
                        ref var value = ref data.ReadRef(i);
                        ((ISerialize)value).Pack(ref writer);
                    }
                }
                else
                {
                    if (writer.GetSerializer(typeof(T), out var serializer))
                    {
                        for (uint i = 0; i < data.Length; ++i)
                        {
                            serializer.Pack(ref writer, data.Read(i));
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < data.Length; ++i)
                        {
                            _compound.Pack(ref writer, data.Read(i));
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, NArray<TAllocator, T> data)
        {
            Unpack(ref reader, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, ref NArray<TAllocator, T> data)
        {
            var elementCount = _meta.Unpack(ref reader);

            if (elementCount != uint.MaxValue)
            {
                uint allocatorId = reader.ReadUInt32();
                MemoryHandle memoryHandle = default;
                memoryHandle.Unpack(ref reader);
                
                data = new NArray<TAllocator, T>(reader.GetDependency<WPtr<TAllocator>>(allocatorId).Value, ref memoryHandle, elementCount);
                
                if (typeof(ISerialize).IsAssignableFrom(typeof(T)))
                {
                    for (uint i = 0; i < data.Length; ++i)
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
                        for (uint i = 0; i < data.Length; ++i)
                        {
                            data.Set(i, (T)serializer.Unpack(ref reader, null));
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < data.Length; ++i)
                        {
                            data.Set(i, (T)_compound.Unpack(ref reader, typeof(T)));
                        }
                    }
                }
            }
        }
    }
    
    internal unsafe struct NArrayEachSerializeStaticSerializer<TAllocator, T>
        where TAllocator : unmanaged, IAllocator
        where T : unmanaged, ISerialize
    {
        private static readonly NArrayMeta _meta;
        private static readonly CountMeta _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref NArray<TAllocator, T> data)
        {
            Pack(ref writer, ref data, data.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Pack(ref WriterContextSerializer writer, ref NArray<TAllocator, T> data, uint count)
        {
            _meta.Pack(ref writer, ref data);
            if (data.IsValid)
            {
                var ptr = data.GetPtr();
                _count.Pack(ref writer, count);

                writer.Write(data.GetAllocator()->GetId());
                data.GetMemoryHandle().Pack(ref writer);

                for (uint i = 0; i < count; i++)
                {
                    ptr[i].Pack(ref writer);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NArray<TAllocator, T> Unpack(ref ReaderContextSerializer reader)
        {
            var elementCount = _meta.Unpack(ref reader);

            if (elementCount != uint.MaxValue)
            {
                var count = _count.Unpack(ref reader);

                uint allocatorId = reader.ReadUInt32();
                MemoryHandle memoryHandle = default;
                memoryHandle.Unpack(ref reader);

                var nArray = new NArray<TAllocator, T>(reader.GetDependency<WPtr<TAllocator>>(allocatorId).Value, ref memoryHandle, elementCount);
                var buffer = nArray.ReadPtr();

                T element = default;
                for (uint i = 0; i < count; i++)
                {
                    element.Unpack(ref reader);
                    buffer[i] = element;
                }

                return nArray;
            }
            else
            {
                return default;
            }
        }
    }
}