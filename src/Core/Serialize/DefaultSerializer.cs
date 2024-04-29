using AnotherECS.Converter;
using AnotherECS.Core;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AnotherECS.Serializer
{
    public class DefaultSerializer : ISerializer
    {
        private const int COMPRESS_FLAG_SIZE = 1;

        private readonly LightSerializer _impl;

        public DefaultSerializer()
        {
            var elementSerializers = new ReflectionElementSerializersProvider().Gets();

            var typeSerializers = TypeUtils.GetAllowHasAttributeFromTypesAcrossAll<SerializeAttribute>()
                .Union(elementSerializers.Select(p => p.Type));

            _impl = new LightSerializer(
                new SerializeToUIntConverter(LightSerializer.START_CUSTOM_RANGE_CODES, typeSerializers),
                elementSerializers
                );
        }

        public byte[] Pack(object data, IEnumerable<DependencySerializer> dependencies = null)
        {
            var context = new WriterContextSerializer(_impl, 0, dependencies);
            var isCompress = WriteCompressFlag(ref context, data);
            
            _impl.Pack(ref context, data);
            var result = context.ToArray();
            context.Dispose();

            return isCompress ? CompressUtils.Compress(result, COMPRESS_FLAG_SIZE) : result;
        }

        public object Unpack(byte[] data, IEnumerable<DependencySerializer> dependencies = null)
        {
            ReaderContextSerializer context;

            if (IsCompress(data))
            {
                context = new(_impl, CompressUtils.Decompress(data, COMPRESS_FLAG_SIZE), 0, dependencies);
            }
            else
            {
                context = new(_impl, data, COMPRESS_FLAG_SIZE);
            }
            var result = _impl.Unpack(ref context);
            context.Dispose();

            return result;
        }

        public T Unpack<T>(byte[] data, IEnumerable<DependencySerializer> dependencies = null)
            => (T)Unpack(data, dependencies);



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCompress(object data)
            => data is not IEvent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCompress(byte[] data)
        {
            if (data == null)
            {
                throw new System.NullReferenceException(nameof(data));
            }
            if (data.Length == 0)
            {
                throw new System.ArgumentException(nameof(data.Length));
            }

            return data[0] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool WriteCompressFlag(ref WriterContextSerializer writer, object data)
        {
            var isCompress = IsCompress(data);
            writer.Write(isCompress);
            return isCompress;
        }
    }
}
