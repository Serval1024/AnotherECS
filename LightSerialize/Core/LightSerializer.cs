using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnotherECS.Serializer
{
    public class LightSerializer : ISerializer
    { 
        public const byte START_CUSTOM_RANGE_CODES = 2;

        internal const byte CODE_NULL = 0;
        internal const byte CODE_ARRAY = 1;

        private readonly Dictionary<Type, IElementSerializer> _serializerByTypes = new();
        private readonly ITypeToUInt _converter;

        private readonly TypeMeta _typeMeta;
        private readonly GenericMeta _genericMeta;
        private readonly ArrayMeta _arrayMeta;
        private readonly ArrayUnmanagedMeta _arrayUnmanagedMeta;
        private readonly CompoundMeta _compound;
        private readonly ArrayPool<object> _typeArrayPool;

        public LightSerializer(SerializeToUInt typeToUIntProvider)
        {
            _converter = typeToUIntProvider;
            _typeArrayPool = new ArrayPool<object>(4);
            Init(Create(typeToUIntProvider.GetISerializeres()));
        }

        public static IEnumerable<IElementSerializer> Create((uint id, Type type)[] types)
            => types
            .Select(p => Activator.CreateInstance(p.type) as IElementSerializer);

        public void Add(IElementSerializer serializer)
        {
            _serializerByTypes.Add(serializer.Type, serializer);
        }

        public byte[] Pack(object data)
        {
            var context = new WriterContextSerializer(this);

            Pack(ref context, data);
            var result = context.ToArray();
            context.Dispose();
            return result;
        }

        public T Unpack<T>(byte[] data, params object[] constructArgs)
            => (T)Unpack(data, constructArgs);

        public object Unpack(byte[] data)
            => Unpack(data, null);

        public object Unpack(byte[] data, params object[] constructArgs)
        {
            var context = new ReaderContextSerializer(this, data);
            var result = Unpack(ref context, constructArgs);
            context.Dispose();
            return result;
        }

        public void Pack(ref WriterContextSerializer writer, object data)
        {
            if (data is null)
            {
                _typeMeta.Pack(ref writer, CODE_NULL);
                return;
            }

            var type = data.GetType();

            if (type.IsArray)
            {
                _typeMeta.Pack(ref writer, CODE_ARRAY);
                _arrayMeta.Pack(ref writer, (Array)data);
            }
            else
            {
                _genericMeta.Pack(ref writer, type);

                if (data is ISerialize serialize)
                {
                    serialize.Pack(ref writer);
                }
                else
                {
                    if (GetSerializer(type, out var serializer))
                    {
                        serializer.Pack(ref writer, data);
                    }
                    else
                    {
                        _compound.Pack(ref writer, data);
                    }
                }
            }
        }

        public object Unpack(ref ReaderContextSerializer reader, params object[] constructArgs)
        {
            var typeId = _typeMeta.Unpack(ref reader);

            if (typeId == CODE_NULL)
            {
                return null;
            }
            else if (typeId == CODE_ARRAY)
            {
                return _arrayMeta.Unpack(ref reader);
            }

            var type = _genericMeta.Unpack(ref reader, typeId);

            if (typeof(ISerialize).IsAssignableFrom(type))
            {
                if (typeof(ISerializeConstructor).IsAssignableFrom(type))
                {
                    var args = _typeArrayPool.Get(reader, constructArgs);
                    return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as ISerialize;
                }
                else
                {
                    var serialize = Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, constructArgs, null) as ISerialize;
                    serialize.Unpack(ref reader);
                    return serialize;
                }
            }
            else
            {
                if (GetSerializer(type, out var serializer))
                {
                    return serializer.Unpack(ref reader, constructArgs);
                }
                else
                {
                    return _compound.Unpack(ref reader, type);
                }
            }
        }

        public void WriteStruct(ref WriterContextSerializer writer, object data)
            => WriteStruct(ref writer, data.GetType(), data);

        public void WriteStruct(ref WriterContextSerializer writer, Type type, object data)
        {
            if (data is ISerialize serialize)
            {
                serialize.Pack(ref writer);
            }
            else
            {
                if (GetSerializer(type, out var serializer))
                {
                    serializer.Pack(ref writer, data);
                }
                else
                {
                    _compound.Pack(ref writer, data);
                }
            }
        }

        public T ReadStruct<T>(ref ReaderContextSerializer reader)
            => (T)ReadStruct(ref reader, typeof(T));

        public object ReadStruct(ref ReaderContextSerializer reader, Type type)
        {
            if (typeof(ISerialize).IsAssignableFrom(type))
            {
                var serialize = Activator.CreateInstance(type) as ISerialize;
                serialize.Unpack(ref reader);
                return serialize;
            }
            else
            {
                if (GetSerializer(type, out var serializer))
                {
                    return serializer.Unpack(ref reader, null);
                }
                else
                {
                    return _compound.Unpack(ref reader, type);
                }
            }
        }

        public void WriteArray<T>(ref WriterContextSerializer writer, T[] data, int count)
            where T : struct
            => _arrayMeta.Pack(ref writer, data, count);

        public T[] ReadArray<T>(ref ReaderContextSerializer reader)
            where T : struct
            => (T[])_arrayMeta.Unpack(ref reader);


        public unsafe void WriteUnmanaged<T>(ref WriterContextSerializer writer, T[] data, int count)
            where T : unmanaged
        {
            if (data == null)
            {
                _typeMeta.Pack(ref writer, CODE_NULL);
                return;
            }

            _typeMeta.Pack(ref writer, CODE_ARRAY);
            _arrayUnmanagedMeta.Write(ref writer, data, count);
        }

        public unsafe T[] ReadUnmanaged<T>(ref ReaderContextSerializer reader)
            where T : unmanaged
        {
            var type = _typeMeta.Unpack(ref reader);
            return (type == CODE_NULL)
                ? null
                : _arrayUnmanagedMeta.Read<T>(ref reader);
        }

        public Type IdToType(uint id)
            => _converter.IdToType(id);
        
        public uint TypeToId(Type type)
            => _converter.TypeToId(type);

        public bool GetSerializer(Type type, out IElementSerializer serializer)
            => _serializerByTypes.TryGetValue(type, out serializer)
                || (type.IsGenericType && _serializerByTypes.TryGetValue(type.GetGenericTypeDefinition(), out serializer));

        private void Init(IEnumerable<IElementSerializer> serializeres)
        {
            foreach (var serializer in serializeres)
            {
                Add(serializer);
            }
        }


        private struct ArrayPool<T>
        {
            public readonly T[][] arrays;

            public ArrayPool(int arrayCapacityMax)
            {
                arrays = new T[arrayCapacityMax][];
                for (int i = 0; i < arrayCapacityMax; ++i)
                {
                    arrays[i] = new T[i + 1];
                }
            }

            public T[] Get(T obj, T[] objs)
            {
                var array = arrays[objs.Length];
                array[0] = obj;
                for (int i = 0; i < objs.Length; ++i)
                {
                    array[i + 1] = objs[i];
                }
                return array;
            }
        }

    }
}
