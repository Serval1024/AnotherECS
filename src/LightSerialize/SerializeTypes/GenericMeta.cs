using System;

namespace AnotherECS.Serializer
{
    public struct GenericMeta
    {
        private readonly TypeMeta _typeMeta;
        private readonly CountMeta _countMeta;

        public void Pack(ref WriterContextSerializer writer, Type type)
        {
            if (type.IsGenericType)
            {
                var types = type.GetGenericArguments();

                _typeMeta.Pack(ref writer, writer.TypeToId(type.GetGenericTypeDefinition()));
                _countMeta.Pack(ref writer, (uint)types.Length);

                for (int i = 0; i < types.Length; ++i)
                {
                    Pack(ref writer, types[i]);
                }
            }
            else
            {
                if (type.IsArray)
                {
                    _typeMeta.Pack(ref writer, LightSerializer.CODE_ARRAY);
                    Pack(ref writer, type.GetElementType());
                }
                else
                {
                    _typeMeta.Pack(ref writer, writer.TypeToId(type));
                }
            }
        }

        public Type Unpack(ref ReaderContextSerializer reader, uint typeId)
        {
            if (typeId == LightSerializer.CODE_ARRAY)
            {
                var arrayElementType = Unpack(ref reader);
                return arrayElementType.MakeArrayType();
            }
            else
            {
                var type = reader.IdToType(typeId);

                if (type.IsGenericType)
                {
                    var count = _countMeta.Unpack(ref reader);
                    var types = new Type[count];
                    for (int i = 0; i < count; ++i)
                    {
                        types[i] = Unpack(ref reader);
                    }
                    return type.MakeGenericType(types);
                }
                else
                {
                    return reader.IdToType(typeId);
                }
            }
        }

        public Type Unpack(ref ReaderContextSerializer reader)
            => Unpack(ref reader, _typeMeta.Unpack(ref reader));
    }
}
