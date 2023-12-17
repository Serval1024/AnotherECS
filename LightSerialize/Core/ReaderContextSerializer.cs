using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AnotherECS.Serializer.Exceptions;

namespace AnotherECS.Serializer
{
    public struct ReaderContextSerializer : IDisposable
    {
        private readonly LightSerializer _serializer;
        private Stream _stream;
        private Dictionary<Type, Dictionary<uint, object>> _depencies;

        public ReaderContextSerializer(LightSerializer serializer, byte[] data, IEnumerable<(uint, object)> depencies)
        {
            _serializer = serializer;
            _stream = new Stream(data);

            _depencies = new Dictionary<Type, Dictionary<uint, object>>();
            if (depencies != null)
            {
                foreach (var depency in depencies)
                {
                    AddDepency(depency.Item1, depency.Item2);
                }
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public uint Position
            => _stream.Position;

        public void AddDepency<T>(T depency)
        {
            AddDepencyInternal(typeof(T), 0, depency);
        }

        public void AddDepency<T>(uint depencyId, T depency)
        {
            AddDepencyInternal(typeof(T), depencyId, depency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddDepencyInternal(Type type, uint depencyId, object depency)
        {
            if (_depencies.TryGetValue(type, out var dict))
            {
                dict.Add(depencyId, depency);
            }
            else
            {
                _depencies.Add(type, new Dictionary<uint, object>() { { depencyId, depency } });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetDepency<T>(uint depencyId)
            => (T)_depencies[typeof(T)][depencyId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetDepency<T>()
            => (T)_depencies[typeof(T)][0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type IdToType(uint id)
        {
            try
            {
                return _serializer.IdToType(id);
            }
            catch (KeyNotFoundException e)
            {
                throw new UnknowTypeException(id, e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSerializer(Type type, out IElementSerializer serializer)
            => _serializer.GetSerializer(type, out serializer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint TypeToId(Type type)
        {
            try
            {
                return _serializer.TypeToId(type);
            }
            catch (KeyNotFoundException e)
            {
                throw new UnknowTypeException(type, e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Unpack(params object[] constructArgs)
            => _serializer.Unpack(ref this, constructArgs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Unpack<T>(params object[] constructArgs)
        {
            var unpack = Unpack(constructArgs);
            return (unpack == null) 
                ? default 
                : (T)unpack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadStruct<T>()
            where T : struct
            => _serializer.ReadStruct<T>(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
            => _stream.ReadBoolean();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
            => _stream.ReadByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte()
            => _stream.ReadSByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
            => _stream.ReadInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
            => _stream.ReadUInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32()
            => _stream.ReadInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
            => default(UInt32Serializer).UnpackConcrete(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
            => _stream.ReadInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
            => _stream.ReadUInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadSingle()
            => _stream.ReadSingle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
            => _stream.ReadDouble();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
            => _stream.ReadString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte[] buffer, uint index, uint count)
            => _stream.Read(buffer, index, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadBytes(uint count)
            => _stream.ReadBytes(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Read(void* buffer, uint length)
           => _stream.Read((byte*)buffer, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadUnmanagedArray<T>()
           where T : unmanaged
           => _serializer.ReadUnmanaged<T>(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ReadArray<T>()
           where T : struct
           => _serializer.ReadArray<T>(ref this);
    }
}

