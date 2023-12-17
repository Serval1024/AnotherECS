using System;
using System.Collections;
using System.Collections.Generic;

namespace AnotherECS.Serializer
{
    public struct TypeSerializer : IElementSerializer
    {
        public Type Type => typeof(Type);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write(writer.TypeToId((Type)@value));

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.IdToType(reader.ReadUInt32());
    }

    public struct Int16Serializer : IElementSerializer
    {
        public Type Type => typeof(short);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((short)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadInt16();
    }

    public struct Int32Serializer : IElementSerializer
    {
        public Type Type => typeof(int);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((int)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadInt32();
    }

    public struct Int64Serializer : IElementSerializer
    {
        public Type Type => typeof(long);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((long)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadInt64();
    }

    public struct UInt16Serializer : IElementSerializer
    {
        public Type Type => typeof(ushort);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((ushort)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadUInt16();
    }

    public struct UInt32Serializer : IElementSerializer
    {
        private const uint CODE_NEXT = 0b1000_0000;
        private const uint CODE_NEXT_NEGATIVE = ~CODE_NEXT;

        public Type Type => typeof(uint);

        public void PackConcrete(ref WriterContextSerializer writer, uint value)
        {
            uint nextValue = value;
            while(true)
            {
                nextValue >>= 7;

                if (nextValue == 0)
                {
                    writer.Write((byte)(value & CODE_NEXT_NEGATIVE));
                    break;
                }
                else
                {
                    writer.Write((byte)(value | CODE_NEXT));
                    value = nextValue;
                }
            }
        }

        public uint UnpackConcrete(ref ReaderContextSerializer reader)
        {
            int index = 0;
            uint value = 0;
            uint @byte;
            do
            {
                @byte = reader.ReadByte();
                value |= (@byte & CODE_NEXT_NEGATIVE) << index;
                index += 7;
            }
            while ((@byte & CODE_NEXT) != 0 && index != 35);

            return value;
        }

        public void Pack(ref WriterContextSerializer writer, object @value)
            => PackConcrete(ref writer, (uint)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => UnpackConcrete(ref reader);
    }

    public struct UInt64Serializer : IElementSerializer
    {
        public Type Type => typeof(ulong);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((ulong)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadUInt64();
    }

    public struct ByteSerializer : IElementSerializer
    {
        public Type Type => typeof(byte);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((byte)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadByte();
    }

    public struct SByteSerializer : IElementSerializer
    {
        public Type Type => typeof(sbyte);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((sbyte)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadByte();
    }

    public struct FloatSerializer : IElementSerializer
    {
        public Type Type => typeof(float);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((float)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadSingle();
    }

    public struct DoubleSerializer : IElementSerializer
    {
        public Type Type => typeof(double);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((double)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadDouble();
    }

    public struct StringSerializer : IElementSerializer
    {
        public Type Type => typeof(string);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((string)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadString();
    }

    public struct BooleanSerializer : IElementSerializer
    {
        public Type Type => typeof(bool);

        public void Pack(ref WriterContextSerializer writer, object @value)
            => writer.Write((bool)value);

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
            => reader.ReadBoolean();
    }

    public struct ListSerializer : IElementSerializer
    {
        private readonly CountMeta _countMeta;
        private readonly GenericMeta _typeMeta;

        public Type Type => typeof(List<>);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var list = @value as IList;
            var elementType = @value.GetType().GetGenericArguments()[0];
            _typeMeta.Pack(ref writer, elementType);

            _countMeta.Pack(ref writer, (uint)list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                writer.Pack(list[i]);
            }
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
        {
            var elementType = _typeMeta.Unpack(ref reader);
            var count = _countMeta.Unpack(ref reader);

            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), (int)count) as IList;
            for (int i = 0; i < count; ++i)
            {
                list.Add(reader.Unpack(constructArgs));
            }
            return list;
        }
    }

    public struct HashSetSerializer : IElementSerializer
    {
        private readonly CountMeta _countMeta;
        private readonly GenericMeta _typeMeta;

        public Type Type => typeof(HashSet<>);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var enumerable = @value as IEnumerable;
            
            var elementType = @value.GetType().GetGenericArguments()[0];
            _typeMeta.Pack(ref writer, elementType);

            int count = 0;
            foreach (var element in enumerable)
            {
                ++count;
            }

            _countMeta.Pack(ref writer, (uint)count);

            foreach (var element in enumerable)
            {
                writer.Pack(element);
            }
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
        {
            var elementType = _typeMeta.Unpack(ref reader);
            var count = _countMeta.Unpack(ref reader);

            var hashSet = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(elementType), (int)count);
            var addMethod = hashSet.GetType().GetMethod("Add");
            var parameters = new object[1];

            for (int i = 0; i < count; ++i)
            {
                parameters[0] = reader.Unpack(constructArgs);
                addMethod.Invoke(hashSet, parameters);
            }
            return hashSet;
        }
    }

    public struct DictionarySerializer : IElementSerializer
    {
        private readonly CountMeta _countMeta;
        private readonly GenericMeta _typeMeta;

        public Type Type => typeof(Dictionary<,>);

        public void Pack(ref WriterContextSerializer writer, object @value)
        {
            var dictionary = @value as IDictionary;
            var elementTypes = @value.GetType().GetGenericArguments();
            _typeMeta.Pack(ref writer, elementTypes[0]);
            _typeMeta.Pack(ref writer, elementTypes[1]);
            _countMeta.Pack(ref writer, (uint)dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
            {
                writer.Pack(entry.Key);
                writer.Pack(entry.Value);
            }
        }

        public object Unpack(ref ReaderContextSerializer reader, object[] constructArgs)
        {
            var elementType0 = _typeMeta.Unpack(ref reader);
            var elementType1 = _typeMeta.Unpack(ref reader);
            var count = _countMeta.Unpack(ref reader);

            var dictionary = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(elementType0, elementType1), (int)count) as IDictionary;
            for (int i = 0; i < count; ++i)
            {
                dictionary.Add(reader.Unpack(constructArgs), reader.Unpack(constructArgs));
            }
            return dictionary;
        }
    }
}
