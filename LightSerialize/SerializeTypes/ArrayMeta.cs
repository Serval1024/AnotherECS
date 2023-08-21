using System;

namespace AnotherECS.Serializer
{
    public struct ArrayMeta
    {
        private readonly CountMeta _сount;
        private readonly GenericMeta _type;
        private readonly CompoundMeta _compound;

        public void Pack(ref WriterContextSerializer writer, Array array)
        {
            Pack(ref writer, array, array.Length);
        }

        public void Pack(ref WriterContextSerializer writer, Array array, int count)
        {
            var elementType = array.GetType().GetElementType();

            _type.Pack(ref writer, elementType);

            if (elementType.IsValueType)
            {
                PackValue(ref writer, array, elementType, count);
            }
            else
            {
                PackRefence(ref writer, array, count);
            }
        }

        public Array Unpack(ref ReaderContextSerializer reader)
        {
            var elementType = _type.Unpack(ref reader);

            return elementType.IsValueType
                ? UnpackValue(ref reader, elementType)
                : UnpackReference(ref reader, elementType);
        }

        public void PackValue(ref WriterContextSerializer writer, Array array, Type elementType, int count)
        {
            _сount.Pack(ref writer, (uint)array.Length);
            _сount.Pack(ref writer, (uint)count);

            if (typeof(ISerialize).IsAssignableFrom(elementType))
            {
                for (int i = 0; i < count; ++i)
                {
                    var value = array.GetValue(i);
                    ((ISerialize)value).Pack(ref writer);
                }
            }
            else
            {
                if (writer.GetSerializer(elementType, out var serializer))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        serializer.Pack(ref writer, array.GetValue(i));
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        _compound.Pack(ref writer, array.GetValue(i));
                    }
                }
            }
        }

        private void PackRefence(ref WriterContextSerializer writer, Array array, int count)
        {
            _сount.Pack(ref writer, (uint)array.Length);
            _сount.Pack(ref writer, (uint)count);

            for (int i = 0; i < count; ++i)
            {
                writer.Pack(array.GetValue(i));
            }
        }

        private Array UnpackValue(ref ReaderContextSerializer reader, Type elementType)
        {
            var length = _сount.Unpack(ref reader);
            var count = _сount.Unpack(ref reader);

            var array = Array.CreateInstance(elementType, length);

            if (typeof(ISerialize).IsAssignableFrom(elementType))
            {
                for (int i = 0; i < count; ++i)
                {
                    var serialize = Activator.CreateInstance(elementType) as ISerialize;
                    serialize.Unpack(ref reader);
                    array.SetValue(serialize, i);
                }
            }
            else
            {
                if (reader.GetSerializer(elementType, out var serializer))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        array.SetValue(serializer.Unpack(ref reader, null), i);
                    }
                }
                else
                {
                    for (int i = 0; i < count; ++i)
                    {
                        array.SetValue(_compound.Unpack(ref reader, elementType), i);
                    }
                }
            }
            
            return array;
        }

        private Array UnpackReference(ref ReaderContextSerializer reader, Type elementType)
        {
            var length = _сount.Unpack(ref reader);
            var count = _сount.Unpack(ref reader);

            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < count; ++i)
            {
                array.SetValue(reader.Unpack(), i);
            }
            return array;
        }
    }
}
