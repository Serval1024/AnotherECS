using System;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer;

namespace AnotherECS.Core.Collection
{
    /*  TODO SER
    internal unsafe struct NContainerArraySerializer<TAllocator, T>
           where TAllocator : unmanaged, IAllocator
           where T : unmanaged
    {
        private NArrayMeta _meta;
        private CompoundMeta _compound;

        public Type Type => typeof(NContainerArray<,>);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, NContainerArray<TAllocator, T> data)
        {
            Pack(ref writer, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pack(ref WriterContextSerializer writer, ref NContainerArray<TAllocator, T> data)
        {
            

            _meta.Pack(ref writer, ref data);

            if (data.IsValide)
            {
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
        public void Unpack(ref ReaderContextSerializer reader, NContainerArray<TAllocator, T> data)
        {
            Unpack(ref reader, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpack(ref ReaderContextSerializer reader, ref NContainerArray<TAllocator, T> data)
        {
            var elementCount = _meta.Unpack(ref reader);

            if (elementCount != uint.MaxValue)
            {
                data = new NContainerArray<TAllocator, T>(reader.GetDepency<NPtr<TAllocator>>().Value, elementCount);

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
    }*/
}